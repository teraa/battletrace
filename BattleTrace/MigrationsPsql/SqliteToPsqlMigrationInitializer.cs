using BattleTrace.Data;
using Extensions.Hosting.AsyncInitialization;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace BattleTrace.MigrationsPsql;

[UsedImplicitly]
public sealed class SqliteToPsqlMigrationInitializer(
    AppDbContext oldCtx,
    AppPsqlDbContext newCtx,
    ILogger<SqliteToPsqlMigrationInitializer> logger
) : IAsyncInitializer
{
    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        var isMigrated =
            await newCtx.Players.AnyAsync(cancellationToken) ||
            await newCtx.Servers.AnyAsync(cancellationToken) ||
            await newCtx.PlayerScans.AnyAsync(cancellationToken) ||
            await newCtx.ServerScans.AnyAsync(cancellationToken);

        if (isMigrated)
        {
            logger.LogInformation("Already migrated");
            return;
        }

        var playerScans = await oldCtx.PlayerScans.ToListAsync(cancellationToken);
        newCtx.PlayerScans.AddRange(playerScans);

        var serverScans = await oldCtx.ServerScans.ToListAsync(cancellationToken);
        newCtx.ServerScans.AddRange(serverScans);

        var players = await oldCtx.Players.ToListAsync(cancellationToken);
        newCtx.Players.AddRange(players);

        var servers = await oldCtx.Servers.ToListAsync(cancellationToken);
        newCtx.Servers.AddRange(servers);

        await newCtx.SaveChangesAsync(cancellationToken);
    }
}
