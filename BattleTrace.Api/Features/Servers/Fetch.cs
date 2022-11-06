using System.Diagnostics;
using BattleTrace.Api.Options;
using BattleTrace.Data;
using JetBrains.Annotations;
using MediatR;
using Microsoft.Extensions.Options;

namespace BattleTrace.Api.Features.Servers;

public static class Fetch
{
    public record Command : IRequest;

    [UsedImplicitly]
    public class Handler : AsyncRequestHandler<Command>
    {
        private readonly FetcherOptions _options;
        private readonly AppDbContext _ctx;
        private readonly ILogger<Handler> _logger;
        private readonly HttpClient _client;

        public Handler(
            IOptionsMonitor<FetcherOptions> options,
            AppDbContext ctx,
            ILogger<Handler> logger,
            HttpClient client)
        {
            _ctx = ctx;
            _logger = logger;
            _client = client;
            _options = options.CurrentValue;
        }

        protected override async Task Handle(Command request, CancellationToken cancellationToken)
        {
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

                _logger.LogDebug("Request {Request}: Found {NewServers} new servers, {TotalServers} total", requestIndex, servers.Count - serversCount, servers.Count);

                requestIndex++;
            } while (requestIndex < lastSuccessfulIndex + _options.Threshold);
        }


        private record Response(
            IReadOnlyList<Server> Data);

        public record Server(
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
