using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace BridgeApi.Persistence.Contexts;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        // Assembly: Persistence/bin/Debug/net8.0 -> 5 seviye yukarı = solution root (BridgeApi)
        var assemblyDir = Path.GetDirectoryName(typeof(DesignTimeDbContextFactory).Assembly.Location) ?? ".";
        var solutionRoot = Path.GetFullPath(Path.Combine(assemblyDir, "..", "..", "..", "..", ".."));
        var apiPath = Path.Combine(solutionRoot, "Presentation", "BridgeApi.API");

        IConfiguration configuration = new ConfigurationBuilder()
            .SetBasePath(apiPath)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
