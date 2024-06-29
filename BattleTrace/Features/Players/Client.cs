using System.Threading.RateLimiting;
using Microsoft.Extensions.Options;

namespace BattleTrace.Features.Players;

public sealed class Client
{
    private readonly HttpClient _client;

    public Client(HttpClient client)
    {
        _client = client;
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
    }

    public Task<HttpResponseMessage> GetServerSnapshot(string serverId, CancellationToken cancellationToken)
    {
        return _client.GetAsync($"https://keeper.battlelog.com/snapshot/{serverId}", cancellationToken);
    }


    public sealed class Handler : DelegatingHandler
    {
        private readonly RateLimiter _limiter;

        public Handler(IOptions<PlayerFetcherOptions> options)
        {
            _limiter = new TokenBucketRateLimiter(new TokenBucketRateLimiterOptions
            {
                ReplenishmentPeriod = options.Value.BatchDelay,
                TokensPerPeriod = options.Value.BatchSize,
                TokenLimit = options.Value.BatchSize,
                QueueLimit = int.MaxValue,
            });
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            using var lease = await _limiter.AcquireAsync(1, cancellationToken);

            if (!lease.IsAcquired)
                throw new InvalidOperationException();

            return await base.SendAsync(request, cancellationToken);
        }
    }
};
