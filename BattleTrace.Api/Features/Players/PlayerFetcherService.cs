using BattleTrace.Api.Options;
using BattleTrace.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
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
        await using (var scope = _scopeFactory.CreateAsyncScope())
        {
            var ctx = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var lastScan = await ctx.PlayerScans
                .OrderByDescending(x => x.Timestamp)
                .Select(x => x.Timestamp)
                .FirstOrDefaultAsync(stoppingToken);

            var initialDelay = lastScan + _interval - DateTimeOffset.UtcNow;
            if (initialDelay > TimeSpan.Zero)
            {
                _logger.LogInformation("Last player scan was at {LastScan}, delaying next scan by {Delay}",
                    lastScan, initialDelay);

                await Task.Delay(initialDelay, stoppingToken);
            }
        }

        var timer = new PeriodicTimer(_interval);

        do
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();

            try
            {
                await sender.Send(new Fetch.Command(), stoppingToken);
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching");
            }
        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}
