using AfterQuake.Domain.Entities;
using AfterQuake.Infrastructure.Data;
using AfterQuake.Infrastructure.Seed;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AfterQuake.Infrastructure.Services;

public class DatabaseSeedService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;

    public DatabaseSeedService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await context.Database.EnsureCreatedAsync(cancellationToken);
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        await ApplicationDbContextSeed.SeedAsync(context, userManager, roleManager);

        foreach (var user in userManager.Users.ToList())
        {
            var token = await userManager.GetAuthenticationTokenAsync(user, "PasswordExpiration", "LastChanged");
            if (string.IsNullOrEmpty(token))
            {
                await userManager.SetAuthenticationTokenAsync(user, "PasswordExpiration", "LastChanged", DateTime.UtcNow.ToString("O"));
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
