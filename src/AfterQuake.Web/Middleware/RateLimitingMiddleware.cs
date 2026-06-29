using System.Collections.Concurrent;
using System.Security.Claims;

namespace AfterQuake.Web.Middleware;

public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ConcurrentDictionary<string, SlidingWindowCounter> _counters = new();
    private readonly Timer _cleanupTimer;

    private static readonly HashSet<string> _limitedPaths = new(StringComparer.OrdinalIgnoreCase)
    {
        "/Emergency/Report",
        "/Person/ReportMissing",
        "/Person/ReportFound",
        "/HelpRequest/Create",
        "/Account/Login",
        "/Account/Register"
    };

    private static readonly TimeSpan _window = TimeSpan.FromMinutes(1);
    private static readonly TimeSpan _cleanupInterval = TimeSpan.FromMinutes(5);
    private const int _anonymousLimit = 5;
    private const int _authenticatedLimit = 30;

    public RateLimitingMiddleware(RequestDelegate next)
    {
        _next = next;
        _cleanupTimer = new Timer(CleanupStaleEntries, null, _cleanupInterval, _cleanupInterval);
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Method == "POST" && _limitedPaths.Contains(context.Request.Path.Value ?? ""))
        {
            var key = GetClientKey(context);
            var counter = _counters.GetOrAdd(key, _ => new SlidingWindowCounter(_window, _anonymousLimit));
            var limit = context.User.Identity?.IsAuthenticated == true ? _authenticatedLimit : _anonymousLimit;

            if (!counter.TryIncrement(limit, out var retryAfter))
            {
                context.Response.StatusCode = 429;
                context.Response.Headers["Retry-After"] = retryAfter?.TotalSeconds.ToString("0");
                context.Response.Headers["X-RateLimit-Reset"] = DateTimeOffset.UtcNow.Add(retryAfter ?? _window).ToUnixTimeSeconds().ToString();
                await context.Response.WriteAsJsonAsync(new { error = "Demasiadas solicitudes. Intente nuevamente en un minuto." });
                return;
            }

            context.Response.Headers["X-RateLimit-Remaining"] = (limit - counter.Count).ToString();
        }

        await _next(context);
    }

    private void CleanupStaleEntries(object? state)
    {
        var cutoff = DateTime.UtcNow - _window;
        foreach (var kvp in _counters)
        {
            if (kvp.Value.LastActivity < cutoff)
                _counters.TryRemove(kvp.Key, out _);
        }
    }

    private static string GetClientKey(HttpContext context)
    {
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId != null)
            return $"user:{userId}";
        return $"ip:{context.Connection.RemoteIpAddress?.ToString() ?? "unknown"}";
    }

    private class SlidingWindowCounter
    {
        private readonly object _lock = new();
        private readonly Queue<DateTime> _timestamps = new();
        private readonly TimeSpan _window;
        private DateTime _lastActivity;

        public int Count
        {
            get { lock (_lock) { TrimOld(); return _timestamps.Count; } }
        }

        public DateTime LastActivity => _lastActivity;

        public SlidingWindowCounter(TimeSpan window, int limit)
        {
            _window = window;
            _lastActivity = DateTime.UtcNow;
        }

        public bool TryIncrement(int limit, out TimeSpan? retryAfter)
        {
            lock (_lock)
            {
                TrimOld();
                _lastActivity = DateTime.UtcNow;
                if (_timestamps.Count >= limit)
                {
                    var oldest = _timestamps.Peek();
                    retryAfter = oldest.Add(_window) - DateTime.UtcNow;
                    return false;
                }
                _timestamps.Enqueue(DateTime.UtcNow);
                retryAfter = null;
                return true;
            }
        }

        private void TrimOld()
        {
            var cutoff = DateTime.UtcNow - _window;
            while (_timestamps.Count > 0 && _timestamps.Peek() < cutoff)
                _timestamps.Dequeue();
        }
    }
}

public static class RateLimitingMiddlewareExtensions
{
    public static IApplicationBuilder UseRateLimiting(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RateLimitingMiddleware>();
    }
}
