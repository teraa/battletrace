﻿using System.Diagnostics;
using BattleTrace.Data;
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
        private readonly Client _client;

        public Handler(
            IOptionsMonitor<ServerFetcherOptions> options,
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

            var servers = new Dictionary<string, Client.Server>();
            int requestIndex = 0;
            int lastSuccessfulIndex = 0;

            do
            {
                int offset = requestIndex * _options.Offset;
                var response = await _client.GetServers(offset, cancellationToken);

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

            _ctx.ServerScans.Add(new Data.Models.ServerScan
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
                Country = x.Country,
                TickRate = x.TickRate,
                UpdatedAt = now,
            }));

            await _ctx.SaveChangesAsync(cancellationToken);
        }
    }
}
