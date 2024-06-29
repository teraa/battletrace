using System.Collections.Concurrent;
using System.Diagnostics;
using BattleTrace.Data;
using BattleTrace.Data.Models;
using JetBrains.Annotations;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

// ReSharper disable ClassNeverInstantiated.Local

namespace BattleTrace.Features.Players;

public static class Fetch
{
    public record Command : IRequest;

    [UsedImplicitly]
    public class Handler : IRequestHandler<Command>
    {
        private readonly PlayerFetcherOptions _options;
        private readonly AppDbContext _ctx;
        private readonly ILogger<Handler> _logger;
        private readonly Client _client;

        public Handler(
            IOptionsMonitor<PlayerFetcherOptions> options,
            AppDbContext ctx,
            ILogger<Handler> logger,
            Client client)
        {
            _ctx = ctx;
            _logger = logger;
            _client = client;
            _options = options.CurrentValue;
        }

        public async Task Handle(Command request, CancellationToken cancellationToken)
        {
            var sw = Stopwatch.StartNew();

            var scanAt = DateTimeOffset.UtcNow;
            var minTimestamp = scanAt - _options.MaxServerAge;

            var servers = await _ctx.Servers
                .Where(x => x.UpdatedAt > minTimestamp)
                .ToListAsync(cancellationToken);

            _logger.LogDebug("Fetching players for {Servers} servers ", servers.Count);

            var responses = new ConcurrentBag<(string serverId, DateTimeOffset updatedAt, Client.Response response)>();

            await Parallel.ForEachAsync(servers, cancellationToken, async (server, ct) =>
            {
                var response = await _client.GetServerSnapshot(server.Id, ct);
                if (response is null)
                    return;

                var updatedAt = DateTimeOffset.UtcNow;

                server.UpdatedAt = updatedAt;
                responses.Add((server.Id, updatedAt, response));
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
                .Select(x => new Data.Models.Player
                {
                    Id = x.Player.Key,
                    UpdatedAt = x.UpdatedAt,
                    ServerId = x.ServerId,
                    Faction = x.Team.Value.Faction,
                    Team = int.Parse(x.Team.Key),
                    Name = x.Player.Value.Name,
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
}
