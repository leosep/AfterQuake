using Microsoft.AspNetCore.Mvc.Filters;

namespace AfterQuake.Web.Filters;

public class SecurityHeadersFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context) { }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        if (context.HttpContext.Response.Headers.IsReadOnly) return;
        context.HttpContext.Response.Headers["X-Content-Type-Options"] = "nosniff";
        context.HttpContext.Response.Headers["X-Frame-Options"] = "DENY";
        // X-XSS-Protection is deprecated; CSP handles this
        context.HttpContext.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
        context.HttpContext.Response.Headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=(self)";
        context.HttpContext.Response.Headers["Content-Security-Policy-Report-Only"] =
            "default-src 'self'; " +
            "script-src 'self' 'unsafe-inline' 'unsafe-eval' https://unpkg.com https://cdn.jsdelivr.net https://cdn.tailwindcss.com; " +
            "style-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net https://cdn.tailwindcss.com; " +
            "img-src 'self' data: https:; " +
            "font-src 'self' https://cdn.jsdelivr.net; " +
            "connect-src 'self' https://unpkg.com; " +
            "frame-src 'none'; " +
            "object-src 'none'; " +
            "report-uri /csp-report";
    }
}
