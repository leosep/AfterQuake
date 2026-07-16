using System.Collections.Concurrent;

namespace AfterQuake.Web.Middleware;

public class IpBlockingMiddleware
{
    private readonly RequestDelegate _next;
    private static readonly ConcurrentDictionary<string, FailedAttempts> _failedAttempts = new();
    private static readonly ConcurrentDictionary<string, byte> _blockedIps = new();
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
            var toUnblock = _blockedIps.Keys.Where(ip =>
            {
                if (_failedAttempts.TryGetValue(ip, out var fa))
                    return fa.BlockedAt < unblockCutoff;
                return true;
            }).ToList();

            foreach (var ip in toUnblock)
            {
                _blockedIps.TryRemove(ip, out _);
            }
        }, null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
    }

    public IpBlockingMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        if (_blockedIps.ContainsKey(ip))
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
                _blockedIps.TryAdd(ip, 0);
            }
        }
    }

    public static void RecordSuccess(string ip)
    {
        _failedAttempts.TryRemove(ip, out _);
        _blockedIps.TryRemove(ip, out _);
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
