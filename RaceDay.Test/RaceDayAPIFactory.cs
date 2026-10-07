using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RaceDay.Data;

namespace RaceDay.Tests;

// Spins up the real API pipeline (routing, [Authorize], JWT validation, Swagger)
// but swaps the SQL Server database for a fresh, isolated in-memory one per test class.
public class ApiFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = Guid.NewGuid().ToString();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<DbContext>));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<DbContext>(options =>
            {
                options.UseInMemoryDatabase(_dbName);
            });

            // Ensure the database (and seeded roles) exist for this test run.
            using var scope = services.BuildServiceProvider().CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<DbContext>();
            db.Database.EnsureCreated();
        });
    }
}
