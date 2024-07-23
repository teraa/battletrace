using System.Diagnostics;
using BattleTrace.Data;
using JetBrains.Annotations;
using LinqToDB.EntityFrameworkCore;
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
        private readonly IBattlelogApi _api;

        public Handler(
            IOptionsMonitor<ServerFetcherOptions> options,
            AppDbContext ctx,
            ILogger<Handler> logger,
            IBattlelogApi api)
        {
            _ctx = ctx;
            _logger = logger;
            _api = api;
            _options = options.CurrentValue;
        }

        public async Task Handle(Command request, CancellationToken cancellationToken)
        {
            var sw = Stopwatch.StartNew();

            var servers = new Dictionary<string, IBattlelogApi.Server>();
            int requestIndex = 0;
            int lastSuccessfulIndex = 0;

            do
            {
                int offset = requestIndex * _options.Offset;
                var response = await _api.GetServers(offset, cancellationToken: cancellationToken);

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

            var entities = servers.Values.Select(x => new Data.Models.Server
            {
                Id = x.Guid,
                Name = x.Name,
                IpAddress = x.Ip,
                Port = x.Port,
                Country = x.Country,
                TickRate = x.TickRate,
                UpdatedAt = now,
            });

            // Just delete and re-add all entries instead of bothering with change-tracking

            await _ctx.Servers
                .Where(x => servers.Keys.Contains(x.Id))
                .ExecuteDeleteAsync(cancellationToken);

            await _ctx.BulkCopyAsync(entities, cancellationToken);


            _ctx.ServerScans.Add(new Data.Models.ServerScan
            {
                Timestamp = now,
                ServerCount = servers.Count,
            });

            await _ctx.SaveChangesAsync(cancellationToken);
        }
    }
}
