using Extensions.Hosting.AsyncInitialization;
using Hangfire;
using Microsoft.Extensions.Options;

namespace BattleTrace.Features.Servers;

public sealed class ServerFetcherJobInitializer : IAsyncInitializer
{
    private readonly IRecurringJobManager _recurringJobManager;
    private readonly ServerFetcherOptions _options;

    public ServerFetcherJobInitializer(
        IRecurringJobManager recurringJobManager,
        IOptions<ServerFetcherOptions> options)
    {
        _recurringJobManager = recurringJobManager;
        _options = options.Value;
    }

    public Task InitializeAsync(CancellationToken cancellationToken)
    {
        _recurringJobManager.AddOrUpdate<FetchServers>(
            nameof(FetchServers),
            handler => handler.Handle(CancellationToken.None),
            _options.Cron
        );

        return Task.CompletedTask;
    }
}
