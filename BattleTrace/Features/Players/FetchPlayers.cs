using System.Collections.Concurrent;
using System.Diagnostics;
using BattleTrace.Data;
using BattleTrace.Data.Models;
using JetBrains.Annotations;
using LinqToDB.Data;
using LinqToDB.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace BattleTrace.Features.Players;

[UsedImplicitly]
public sealed class FetchPlayers
{
    private readonly PlayerFetcherOptions _options;
    private readonly AppDbContext _ctx;
    private readonly ILogger<FetchPlayers> _logger;
    private readonly IKeeperBattlelogApi _api;
    private readonly TimeProvider _time;

    public FetchPlayers(
        IOptionsMonitor<PlayerFetcherOptions> options,
        AppDbContext ctx,
        ILogger<FetchPlayers> logger,
        IKeeperBattlelogApi api,
        TimeProvider time)
    {
        _ctx = ctx;
        _logger = logger;
        _api = api;
        _time = time;
        _options = options.CurrentValue;
    }

    public async Task Handle(CancellationToken cancellationToken = default)
    {
        var sw = Stopwatch.StartNew();

        var scanAt = _time.GetUtcNow();
        var minTimestamp = scanAt - _options.MaxServerAge;

        var servers = await _ctx.Servers
            .Where(x => x.UpdatedAt > minTimestamp)
            .ToListAsync(cancellationToken);

        _logger.LogDebug("Fetching players for {Servers} servers ", servers.Count);

        var responses = new ConcurrentBag
            <(string serverId, DateTimeOffset updatedAt, IKeeperBattlelogApi.SnapshotResponse response)>();

        await Parallel.ForEachAsync(
            servers,
            cancellationToken,
            async (server, ct) =>
            {
                var httpResponse = await _api.GetSnapshot(server.Id, ct);
                if (!httpResponse.IsSuccessStatusCode)
                {
                    _logger.LogDebug(
                        "Failed fetching players for {ServerId}, server returned: {StatusCode} ({ReasonPhrase})",
                        server.Id,
                        (int) httpResponse.StatusCode,
                        httpResponse.ReasonPhrase
                    );

                    return;
                }

                var updatedAt = _time.GetUtcNow();

                server.UpdatedAt = updatedAt;
                responses.Add((server.Id, updatedAt, httpResponse.Content!));
            }
        );

        var players = responses.SelectMany(
                x => x.response.Snapshot.TeamInfo
                    .SelectMany(
                        t => t.Value.Players
                            .Select(
                                p => new
                                {
                                    ServerId = x.serverId,
                                    UpdatedAt = x.updatedAt,
                                    Team = t,
                                    Player = p,
                                }
                            )
                    )
            )
            .GroupBy(x => x.Player.Key)
            .Select(group => group.MaxBy(x => x.UpdatedAt)!)
            .Select(
                x => new Player
                {
                    Id = x.Player.Key,
                    UpdatedAt = x.UpdatedAt,
                    ServerId = x.ServerId,
                    Faction = x.Team.Value.Faction,
                    Team = int.Parse(x.Team.Key),
                    Name = x.Player.Value.Name,
                    NormalizedName = x.Player.Value.Name.ToLowerInvariant(),
                    Tag = x.Player.Value.Tag,
                    Rank = x.Player.Value.Rank,
                    Score = x.Player.Value.Score,
                    Kills = x.Player.Value.Kills,
                    Deaths = x.Player.Value.Deaths,
                    Squad = x.Player.Value.Squad,
                    Role = x.Player.Value.Role,
                }
            )
            .ToList();

        sw.Stop();
        _logger.LogInformation(
            "Fetched {Players} players from {Servers} servers in {Duration}",
            players.Count,
            servers.Count,
            sw.Elapsed
        );

        await using var tsc = await _ctx.Database.BeginTransactionAsync(cancellationToken);

        _ctx.PlayerScans.Add(
            new PlayerScan
            {
                Timestamp = scanAt,
                PlayerCount = players.Count,
            }
        );

        await _ctx.SaveChangesAsync(cancellationToken);

        var playerIds = players.Select(x => x.Id).ToList();

        await _ctx.Players
            .Where(x => playerIds.Contains(x.Id))
            .ExecuteDeleteAsync(cancellationToken);

        await _ctx.BulkCopyAsync(
            new BulkCopyOptions(),
            players,
            cancellationToken
        );

        await tsc.CommitAsync(cancellationToken);
    }
}
