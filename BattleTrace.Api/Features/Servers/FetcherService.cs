﻿using BattleTrace.Api.Options;
using MediatR;
using Microsoft.Extensions.Options;

namespace BattleTrace.Api.Features.Servers;

public class FetcherService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<FetcherService> _logger;
    private readonly TimeSpan _interval;

    public FetcherService(
        IServiceScopeFactory scopeFactory,
        IOptions<FetcherOptions> options,
        ILogger<FetcherService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _interval = options.Value.Interval;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var timer = new PeriodicTimer(_interval);
        bool firstRun = true;

        while (firstRun || await timer.WaitForNextTickAsync(stoppingToken))
        {
            firstRun = false;

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
