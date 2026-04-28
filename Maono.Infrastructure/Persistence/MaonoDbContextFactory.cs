using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Maono.Infrastructure.Persistence;

/// <summary>
/// Design-time factory for EF Core migrations.
/// Used by `dotnet ef migrations add` and `dotnet ef database update`.
/// </summary>
public class MaonoDbContextFactory : IDesignTimeDbContextFactory<MaonoDbContext>
{
    public MaonoDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "..", "Maono.Api"))
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<MaonoDbContext>();
        optionsBuilder.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));

        return new MaonoDbContext(optionsBuilder.Options);
    }
}
