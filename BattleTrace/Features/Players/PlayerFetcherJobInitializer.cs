using Extensions.Hosting.AsyncInitialization;
using Hangfire;
using Microsoft.Extensions.Options;

namespace BattleTrace.Features.Players;

public sealed class PlayerFetcherJobInitializer : IAsyncInitializer
{
    private readonly IRecurringJobManager _recurringJobManager;
    private readonly PlayerFetcherOptions _options;

    public PlayerFetcherJobInitializer(
        IRecurringJobManager recurringJobManager,
        IOptions<PlayerFetcherOptions> options)
    {
        _recurringJobManager = recurringJobManager;
        _options = options.Value;
    }

    public Task InitializeAsync(CancellationToken cancellationToken)
    {
        _recurringJobManager.AddOrUpdate<Fetch>(
            typeof(Fetch).FullName,
            handler => handler.Handle(CancellationToken.None),
            _options.Cron
        );

        return Task.CompletedTask;
    }
}
