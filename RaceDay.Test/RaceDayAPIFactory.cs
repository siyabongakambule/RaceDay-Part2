using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RaceDay.Data;

namespace RaceDay.Tests;

public class ApiFactory : WebApplicationFactoryglobal::Program
{
private readonly string _dbName = Guid.NewGuid().ToString();

protected override void ConfigureWebHost(IWebHostBuilder builder)
{
    builder.ConfigureServices(services =>
    {
        // Remove the existing SQL Server database configuration.
        services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
        services.RemoveAll<ApplicationDbContext>();
        // Register an isolated in-memory database for tests.
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseInMemoryDatabase(_dbName);
        });
    });
}
protected override void ConfigureClient(HttpClient client)
{
    base.ConfigureClient(client);
    // Initialize the test database and seed the roles.
    using var scope = Services.CreateScope();
    var db = scope.ServiceProvider
        .GetRequiredService<ApplicationDbContext>();
    db.Database.EnsureCreated();
}

}
