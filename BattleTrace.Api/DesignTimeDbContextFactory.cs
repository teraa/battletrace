using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Teraa.Extensions.Configuration;
using BattleTrace.Api.Options;
using BattleTrace.Data;

namespace BattleTrace.Api;

[UsedImplicitly]
internal class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var config = new ConfigurationBuilder()
            .AddUserSecrets<Program>(optional: false)
            .Build();

        var dbOptions = config.GetOptions<DbOptions>();

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(dbOptions.ConnectionString,
                contextOptions =>
                {
                    contextOptions.MigrationsAssembly(typeof(Program).Assembly.FullName);
                    contextOptions.CommandTimeout(600);
                });

        return new AppDbContext(optionsBuilder.Options);
    }
}
