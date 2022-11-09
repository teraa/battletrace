using Extensions.Hosting.AsyncInitialization;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using BattleTrace.Data;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace BattleTrace.Api.Initializers;

[UsedImplicitly]
public class MigrationInitializer : IAsyncInitializer
{
    private readonly AppDbContext _ctx;

    public MigrationInitializer(AppDbContext ctx)
    {
        _ctx = ctx;
    }

    public async Task InitializeAsync()
    {
        await _ctx.Database.MigrateAsync();

        var converter = new DateTimeOffsetToBinaryConverter();
        var func = converter.ConvertToProviderExpression.Compile();

        var players = await _ctx.Players.ToListAsync();
        foreach (var player in players)
        {
            player.UpdatedAt2 = func(player.UpdatedAt);
        }

        var servers = await _ctx.Servers.ToListAsync();
        foreach (var server in servers)
        {
            server.UpdatedAt2 = func(server.UpdatedAt);
        }

        await _ctx.SaveChangesAsync();
    }
}
