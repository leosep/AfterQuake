using System.Collections.Concurrent;

namespace AfterQuake.Web.Middleware;

public class IpBlockingMiddleware
{
    private readonly RequestDelegate _next;
    private static readonly ConcurrentDictionary<string, FailedAttempts> _failedAttempts = new();
    private static readonly HashSet<string> _blockedIps = new();
    private static readonly Timer _cleanupTimer;

    private const int MaxAttempts = 10;
    private static readonly TimeSpan BlockDuration = TimeSpan.FromMinutes(15);
    private static readonly TimeSpan AttemptWindow = TimeSpan.FromMinutes(5);

    static IpBlockingMiddleware()
    {
        _cleanupTimer = new Timer(state =>
        {
            var cutoff = DateTime.UtcNow - AttemptWindow;
            foreach (var kvp in _failedAttempts)
            {
                if (kvp.Value.LastAttempt < cutoff)
                {
                    _failedAttempts.TryRemove(kvp.Key, out _);
                }
            }

            var unblockCutoff = DateTime.UtcNow - BlockDuration;
            foreach (var ip in _blockedIps)
            {
                if (_failedAttempts.TryGetValue(ip, out var fa) && fa.BlockedAt < unblockCutoff)
                    _blockedIps.Remove(ip);
            }
        }, null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
    }

    public IpBlockingMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        if (_blockedIps.Contains(ip))
        {
            context.Response.StatusCode = 429;
            await context.Response.WriteAsJsonAsync(new { error = "Demasiados intentos fallidos. Intenta en 15 minutos." });
            return;
        }

        context.Items["ClientIp"] = ip;

        await _next(context);
    }

    public static void RecordFailure(string ip)
    {
        var entry = _failedAttempts.GetOrAdd(ip, _ => new FailedAttempts());
        lock (entry)
        {
            entry.Count++;
            entry.LastAttempt = DateTime.UtcNow;
            if (entry.Count >= MaxAttempts)
            {
                entry.BlockedAt = DateTime.UtcNow;
                _blockedIps.Add(ip);
            }
        }
    }

    public static void RecordSuccess(string ip)
    {
        _failedAttempts.TryRemove(ip, out _);
        _blockedIps.Remove(ip);
    }

    private class FailedAttempts
    {
        public int Count { get; set; }
        public DateTime LastAttempt { get; set; }
        public DateTime? BlockedAt { get; set; }
    }
}

public static class IpBlockingMiddlewareExtensions
{
    public static IApplicationBuilder UseIpBlocking(this IApplicationBuilder builder) =>
        builder.UseMiddleware<IpBlockingMiddleware>();
}
