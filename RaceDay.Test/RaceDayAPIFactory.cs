using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RaceDay.Data;

namespace RaceDay.Tests;

public class ApiFactory : WebApplicationFactory
{
private readonly string _dbName = Guid.NewGuid().ToString();

protected override void ConfigureWebHost(IWebHostBuilder builder)
{
    builder.ConfigureServices(services =>
    {
        // Remove the existing SQL Server database configuration.
        services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
        services.RemoveAll<ApplicationDbContext>();
        // Use an isolated in-memory database for automated tests.
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseInMemoryDatabase(_dbName);
        });
    });
}

}
