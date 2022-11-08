using BattleTrace.Api.Options;
using MediatR;
using Microsoft.Extensions.Options;

namespace BattleTrace.Api.Features.Players;

public class PlayerFetcherService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<PlayerFetcherService> _logger;
    private readonly TimeSpan _interval;

    public PlayerFetcherService(
        IServiceScopeFactory scopeFactory,
        IOptions<PlayerFetcherOptions> options,
        ILogger<PlayerFetcherService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _interval = options.Value.Interval;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var timer = new PeriodicTimer(_interval);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();

            try
            {
                await sender.Send(new Fetch.Command(), stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching");
            }
        }
    }
}
