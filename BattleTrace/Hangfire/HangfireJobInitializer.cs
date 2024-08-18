using BattleTrace.Features.Players;
using BattleTrace.Features.Servers;
using Extensions.Hosting.AsyncInitialization;
using Hangfire;
using Microsoft.Extensions.Options;

namespace BattleTrace.Hangfire;

public sealed class HangfireJobInitializer : IAsyncInitializer
{
    private readonly IRecurringJobManager _recurringJobManager;
    private readonly HangfireOptions _options;

    public HangfireJobInitializer(
        IRecurringJobManager recurringJobManager,
        IOptions<HangfireOptions> options)
    {
        _recurringJobManager = recurringJobManager;
        _options = options.Value;
    }

    public Task InitializeAsync(CancellationToken cancellationToken)
    {
        _recurringJobManager.AddOrUpdate<FetchPlayers>(
            nameof(Features.Players),
            handler => handler.Handle(CancellationToken.None),
            _options.PlayersCron
        );

        _recurringJobManager.AddOrUpdate<FetchServers>(
            nameof(Features.Servers),
            handler => handler.Handle(CancellationToken.None),
            _options.ServersCron
        );

        return Task.CompletedTask;
    }
}
