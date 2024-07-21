using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Teraa.Extensions.Configuration;

namespace BattleTrace.Data;

[UsedImplicitly]
internal sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var config = new ConfigurationBuilder()
            .AddUserSecrets<Program>(optional: true)
            .Build();

        var dbOptions = config.GetValidatedOptionsOrDefault<DbOptions>();

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(dbOptions.ConnectionString,
                contextOptions =>
                {
                    contextOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                });

        return new AppDbContext(optionsBuilder.Options);
    }
}
