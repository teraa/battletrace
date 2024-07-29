using System.Collections.Concurrent;
using System.Diagnostics;
using BattleTrace.Data;
using BattleTrace.Data.Models;
using JetBrains.Annotations;
using LinqToDB.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

// ReSharper disable ClassNeverInstantiated.Local

namespace BattleTrace.Features.Players;

[UsedImplicitly]
public class FetchPlayers
{
    private readonly PlayerFetcherOptions _options;
    private readonly AppDbContext _ctx;
    private readonly ILogger<FetchPlayers> _logger;
    private readonly IKeeperBattlelogApi _api;

    public FetchPlayers(
        IOptionsMonitor<PlayerFetcherOptions> options,
        AppDbContext ctx,
        ILogger<FetchPlayers> logger,
        IKeeperBattlelogApi api)
    {
        _ctx = ctx;
        _logger = logger;
        _api = api;
        _options = options.CurrentValue;
    }

    public async Task Handle(CancellationToken cancellationToken = default)
    {
        var sw = Stopwatch.StartNew();

        var scanAt = DateTimeOffset.UtcNow;
        var minTimestamp = scanAt - _options.MaxServerAge;

        var servers = await _ctx.Servers
            .Where(x => x.UpdatedAt > minTimestamp)
            .ToListAsync(cancellationToken);

        _logger.LogDebug("Fetching players for {Servers} servers ", servers.Count);

        var responses = new ConcurrentBag<(string serverId, DateTimeOffset updatedAt, IKeeperBattlelogApi.SnapshotResponse response)>();

        await Parallel.ForEachAsync(servers, cancellationToken, async (server, ct) =>
        {
            var httpResponse = await _api.GetSnapshot(server.Id, ct);
            if (!httpResponse.IsSuccessStatusCode)
            {
                _logger.LogDebug(
                    "Failed fetching players for {ServerId}, server returned: {StatusCode} ({ReasonPhrase})",
                    server.Id, (int) httpResponse.StatusCode, httpResponse.ReasonPhrase);

                return;
            }

            var updatedAt = DateTimeOffset.UtcNow;

            server.UpdatedAt = updatedAt;
            responses.Add((server.Id, updatedAt, httpResponse.Content!));
        });

        var players = responses.SelectMany(x => x.response.Snapshot.TeamInfo
                .SelectMany(t => t.Value.Players
                    .Select(p => new
                    {
                        ServerId = x.serverId,
                        UpdatedAt = x.updatedAt,
                        Team = t,
                        Player = p,
                    })))
            .GroupBy(x => x.Player.Key)
            .Select(group => group.MaxBy(x => x.UpdatedAt)!)
            .Select(x => new Player
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
            })
            .ToList();

        sw.Stop();
        _logger.LogInformation("Fetched {Players} players from {Servers} servers in {Duration}", players.Count,
            servers.Count, sw.Elapsed);

        _ctx.PlayerScans.Add(new PlayerScan
        {
            Timestamp = scanAt,
            PlayerCount = players.Count,
        });

        var playerIds = players.Select(x => x.Id).ToList();
        var playersToUpdate = await _ctx.Players
            .Where(x => playerIds.Contains(x.Id))
            .ToListAsync(cancellationToken);

        // Just delete and re-add all entries instead of bothering with change-tracking
        _ctx.Players.RemoveRange(playersToUpdate);
        _ctx.Players.AddRange(players);
        await _ctx.SaveChangesAsync(cancellationToken);
    }
}
