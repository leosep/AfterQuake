using AfterQuake.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace AfterQuake.Web.Middleware;

public class PasswordExpirationMiddleware
{
    private readonly RequestDelegate _next;
    private const int MaxPasswordAgeDays = 90;

    public PasswordExpirationMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var userManager = context.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
            var user = await userManager.GetUserAsync(context.User);
            if (user != null)
            {
                var lastPasswordChange = await userManager.GetAuthenticationTokenAsync(user, "PasswordExpiration", "LastChanged");
                if (string.IsNullOrEmpty(lastPasswordChange) ||
                    DateTime.TryParse(lastPasswordChange, out var lastChange) &&
                    (DateTime.UtcNow - lastChange).TotalDays > MaxPasswordAgeDays)
                {
                    if (!context.Request.Path.StartsWithSegments("/Account/ChangePassword") &&
                        !context.Request.Path.StartsWithSegments("/Account/Logout") &&
                        !(context.Request.Path.StartsWithSegments("/Account/Profile") &&
                          context.Request.Query["expired"] == "true"))
                    {
                        context.Response.Redirect("/Account/Profile?expired=true");
                        return;
                    }
                }
            }
        }
        await _next(context);
    }
}

public static class PasswordExpirationMiddlewareExtensions
{
    public static IApplicationBuilder UsePasswordExpiration(this IApplicationBuilder builder) =>
        builder.UseMiddleware<PasswordExpirationMiddleware>();
}
