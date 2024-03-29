﻿using FluentValidation;
using JetBrains.Annotations;
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace BattleTrace.Options;

#pragma warning disable CS8618
public class PlayerFetcherOptions
{
    public TimeSpan Interval { get; init; } = TimeSpan.FromMinutes(5);
    public TimeSpan BatchDelay { get; init; } = TimeSpan.FromSeconds(2);
    public int BatchSize { get; init; } = 60;
    public TimeSpan MaxServerAge { get; set; } = TimeSpan.FromDays(2);

    [UsedImplicitly]
    public class Validator : AbstractValidator<PlayerFetcherOptions>
    {
        public Validator()
        {
            RuleFor(x => x.Interval).GreaterThan(TimeSpan.Zero);
            RuleFor(x => x.BatchDelay).GreaterThan(TimeSpan.Zero);
            RuleFor(x => x.BatchSize).GreaterThan(0);
            RuleFor(x => x.MaxServerAge).GreaterThan(TimeSpan.Zero);
        }
    }
}
