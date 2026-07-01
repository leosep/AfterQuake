using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using AfterQuake.Infrastructure.Data;
using AfterQuake.Web;

namespace AfterQuake.Tests.Integration;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private static readonly SqliteConnection _sharedConnection;

    static CustomWebApplicationFactory()
    {
        var dbName = Guid.NewGuid().ToString();
        var connectionString = $"Data Source=file:{dbName}.db;Mode=Memory;Cache=Shared";
        _sharedConnection = new SqliteConnection(connectionString);
        _sharedConnection.Open();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
            if (descriptor != null) services.Remove(descriptor);

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlite(_sharedConnection);
            });
        });
    }
}
