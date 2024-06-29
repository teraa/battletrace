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


    public sealed class Handler : RateLimitingHandler
    {
        public Handler(IOptions<PlayerFetcherOptions> options)
            : base(new TokenBucketRateLimiter(new TokenBucketRateLimiterOptions
            {
                ReplenishmentPeriod = options.Value.BatchDelay,
                TokensPerPeriod = options.Value.BatchSize,
                TokenLimit = options.Value.BatchSize,
                QueueLimit = int.MaxValue,
            })) { }
    }
};
