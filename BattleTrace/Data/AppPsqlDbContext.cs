using BattleTrace.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Teraa.Extensions.Configuration;

namespace BattleTrace.Data;

#pragma warning disable CS8618
public sealed class AppPsqlDbContext : DbContext
{
    public AppPsqlDbContext(DbContextOptions<AppPsqlDbContext> options)
        : base(options) { }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        optionsBuilder.UseSnakeCaseNamingConvention();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppPsqlDbContext).Assembly);
    }


    public DbSet<Player> Players { get; init; }
    public DbSet<PlayerScan> PlayerScans { get; init; }
    public DbSet<Server> Servers { get; init; }
    public DbSet<ServerScan> ServerScans { get; init; }
}

internal sealed class DesignTimeAppPsqlDbContextFactory : IDesignTimeDbContextFactory<AppPsqlDbContext>
{
    public AppPsqlDbContext CreateDbContext(string[] args)
    {
        var config = new ConfigurationBuilder()
            .AddUserSecrets<AppPsqlDbContext>(optional: true)
            .Build();

        var dbOptions = config.GetValidatedOptionsOrDefault<DbOptions>();

        var optionsBuilder = new DbContextOptionsBuilder<AppPsqlDbContext>()
            .UseNpgsql(dbOptions.ConnectionString,
                contextOptions =>
                {
                    contextOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                });

        return new AppPsqlDbContext(optionsBuilder.Options);
    }
}
