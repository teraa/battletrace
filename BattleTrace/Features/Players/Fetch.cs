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
        private readonly HttpClient _client;

        public Handler(
            IOptionsMonitor<PlayerFetcherOptions> options,
            AppDbContext ctx,
            ILogger<Handler> logger,
            HttpClient client)
        {
            _ctx = ctx;
            _logger = logger;
            _client = client;
            _options = options.CurrentValue;
        }

        public async Task Handle(Command request, CancellationToken cancellationToken)
        {
            var sw = Stopwatch.StartNew();
            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");

            var scanAt = DateTimeOffset.UtcNow;
            var minTimestamp = scanAt - _options.MaxServerAge;

            var servers = await _ctx.Servers
                .ToListAsync(cancellationToken);

            var serverIds = servers
                .Where(x => x.UpdatedAt > minTimestamp)
                .Select(x => x.Id)
                .ToList();

            int batchCount = (int) Math.Ceiling((double) serverIds.Count / _options.BatchSize);

            _logger.LogDebug("Fetching players for {Servers} servers in {Batches} batches with minimum expected duration of {Duration}",
                serverIds.Count, batchCount, batchCount * _options.BatchDelay);

            var players = new Dictionary<string, Data.Models.Player>();

            for (int i = 0; i < serverIds.Count; i += _options.BatchSize)
            {
                if (i != 0)
                {
                    _logger.LogDebug("Delaying next batch by {Delay}", _options.BatchDelay);
                    await Task.Delay(_options.BatchDelay, cancellationToken);
                }

                _logger.LogDebug("Request batch: {CurrentBatch}/{BatchCount}",
                    i / _options.BatchSize, batchCount);

                var serverTasks = serverIds
                    .Skip(i)
                    .Take(_options.BatchSize)
                    .Select(x => new
                    {
                        ServerId = x,
                        Task = _client.GetAsync($"https://keeper.battlelog.com/snapshot/{x}", cancellationToken)
                    })
                    .ToList();

                await Task.WhenAll(serverTasks.Select(x => x.Task));
                var now = DateTimeOffset.UtcNow;

                foreach (var serverTask in serverTasks)
                {
                    using var httpResponse = await serverTask.Task;

                    if (!httpResponse.IsSuccessStatusCode)
                    {
                        _logger.LogDebug("Failed fetching players for {ServerId}, server returned: {StatusCode}: {ReasonPhrase}",
                            serverTask.ServerId, (int)httpResponse.StatusCode, httpResponse.ReasonPhrase);
                        continue;
                    }

                    var server = await _ctx.Servers
                        .Where(x => x.Id == serverTask.ServerId)
                        .FirstAsync(cancellationToken);

                    server.UpdatedAt = now;

                    var response =
                        await httpResponse.Content.ReadFromJsonAsync<Response>(cancellationToken: cancellationToken);

                    Debug.Assert(response is { });

                    var playersBatch = response.Snapshot.TeamInfo
                        .SelectMany(t => t.Value.Players
                            .Select(p => new Data.Models.Player
                            {
                                Id = p.Key,
                                UpdatedAt = now,
                                ServerId = serverTask.ServerId,
                                Faction = t.Value.Faction,
                                Team = int.Parse(t.Key),
                                Name = p.Value.Name,
                                Tag = p.Value.Tag,
                                Rank = p.Value.Rank,
                                Score = p.Value.Score,
                                Kills = p.Value.Kills,
                                Deaths = p.Value.Deaths,
                                Squad = p.Value.Squad,
                                Role = p.Value.Role,
                            }));

                    foreach (var player in playersBatch)
                        players[player.Id] = player;
                }
            }

            sw.Stop();
            _logger.LogInformation("Fetched {Players} players from {Servers} servers in {Duration}",
                players.Count, servers.Count, sw.Elapsed);

            _ctx.PlayerScans.Add(new PlayerScan
            {
                Timestamp = scanAt,
                PlayerCount = players.Count,
            });

            var playersToUpdate = await _ctx.Players
                .Where(x => players.Keys.Contains(x.Id))
                .ToListAsync(cancellationToken);

            // Just delete and re-add all entries instead of bothering with change-tracking
            _ctx.Players.RemoveRange(playersToUpdate);
            _ctx.Players.AddRange(players.Values);
            await _ctx.SaveChangesAsync(cancellationToken);
        }


        private record Response(Snapshot Snapshot);

        private record Snapshot(Dictionary<string, TeamInfo> TeamInfo);

        private record TeamInfo(int Faction, Dictionary<string, Player> Players);

        private record Player(
            string Name,
            string Tag,
            int Rank,
            long Score,
            int Kills,
            int Deaths,
            int Squad,
            int Role);
    }
}
