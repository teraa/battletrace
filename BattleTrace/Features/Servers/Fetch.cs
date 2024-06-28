using System.Diagnostics;
using BattleTrace.Data;
using BattleTrace.Data.Models;
using JetBrains.Annotations;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace BattleTrace.Features.Servers;

public static class Fetch
{
    public record Command : IRequest;

    [UsedImplicitly]
    public class Handler : IRequestHandler<Command>
    {
        private readonly ServerFetcherOptions _options;
        private readonly AppDbContext _ctx;
        private readonly ILogger<Handler> _logger;
        private readonly HttpClient _client;

        public Handler(
            IOptionsMonitor<ServerFetcherOptions> options,
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

            var servers = new Dictionary<string, Server>();
            int requestIndex = 0;
            int lastSuccessfulIndex = 0;

            do
            {
                if (requestIndex != 0)
                    await Task.Delay(_options.Delay, cancellationToken);

                int offset = requestIndex * _options.Offset;
                var response = await _client.GetFromJsonAsync<Response>(
                    $"https://battlelog.battlefield.com/bf4/servers/getServers/pc/?offset={offset}&count=60",
                    cancellationToken);

                Debug.Assert(response is not null);

                int serversCount = servers.Count;
                foreach (var server in response.Data)
                {
                    servers[server.Guid] = server;
                }

                if (serversCount != servers.Count)
                    lastSuccessfulIndex = requestIndex;

                _logger.LogDebug("Request {Request}: Found {NewServers} new servers, {TotalServers} total",
                    requestIndex, servers.Count - serversCount, servers.Count);

                requestIndex++;
            } while (requestIndex < lastSuccessfulIndex + _options.Threshold);

            sw.Stop();
            _logger.LogInformation("Found {Servers} servers in {Requests} requests within {Duration}",
                servers.Count, requestIndex, sw.Elapsed);

            var now = DateTimeOffset.UtcNow;

            _ctx.ServerScans.Add(new ServerScan
            {
                Timestamp = now,
                ServerCount = servers.Count,
            });

            var serversToUpdate = await _ctx.Servers
                .Where(x => servers.Keys.Contains(x.Id))
                .ToListAsync(cancellationToken);

            _ctx.Servers.RemoveRange(serversToUpdate);

            _ctx.Servers.AddRange(servers.Values.Select(x => new Data.Models.Server
            {
                Id = x.Guid,
                Name = x.Name,
                IpAddress = x.Ip,
                Port = x.Port,
                UpdatedAt = now,
            }));

            await _ctx.SaveChangesAsync(cancellationToken);
        }


        private record Response(IReadOnlyList<Server> Data);

        // ReSharper disable once ClassNeverInstantiated.Local
        private record Server(
            string Guid,
            string Name,
            string Map,
            long MapMode,
            string Country,
            int TickRate,
            string Ip,
            int Port);
    }
}
