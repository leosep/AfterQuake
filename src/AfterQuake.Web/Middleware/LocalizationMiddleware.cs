using System.Globalization;
using AfterQuake.Web.Services;

namespace AfterQuake.Web.Middleware;

public class LocalizationMiddleware
{
    private readonly RequestDelegate _next;
    private const string _cookieName = "AfterQuake.Culture";

    public LocalizationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ILocalizationService localization)
    {
        var culture = ResolveCulture(context);

        if (!string.IsNullOrEmpty(culture))
        {
            try
            {
                var ci = new CultureInfo(culture);
                CultureInfo.CurrentCulture = ci;
                CultureInfo.CurrentUICulture = ci;
                localization.SetCulture(culture);
            }
            catch { }
        }

        await _next(context);
    }

    private static string? ResolveCulture(HttpContext context)
    {
        var queryLang = context.Request.Query["lang"].FirstOrDefault();
        if (!string.IsNullOrEmpty(queryLang))
        {
            SetCultureCookie(context, queryLang);
            return MapToCulture(queryLang);
        }

        var cookieLang = context.Request.Cookies[_cookieName];
        if (!string.IsNullOrEmpty(cookieLang))
            return MapToCulture(cookieLang);

        if (context.User.Identity?.IsAuthenticated == true)
        {
            var userLang = context.User.FindFirst("Culture")?.Value;
            if (!string.IsNullOrEmpty(userLang))
                return MapToCulture(userLang);
        }

        var acceptLang = context.Request.Headers["Accept-Language"].FirstOrDefault();
        if (!string.IsNullOrEmpty(acceptLang))
        {
            var lang = acceptLang.Split(',')[0].Trim();
            if (lang.StartsWith("en", StringComparison.OrdinalIgnoreCase))
                return "en-US";
        }

        return "es-ES";
    }

    private static string MapToCulture(string lang)
    {
        return lang.StartsWith("en", StringComparison.OrdinalIgnoreCase) ? "en-US" : "es-ES";
    }

    private static void SetCultureCookie(HttpContext context, string lang)
    {
        context.Response.Cookies.Append(_cookieName, lang, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            MaxAge = TimeSpan.FromDays(365),
            IsEssential = true
        });
    }
}

public static class LocalizationMiddlewareExtensions
{
    public static IApplicationBuilder UseLocalization(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<LocalizationMiddleware>();
    }
}
