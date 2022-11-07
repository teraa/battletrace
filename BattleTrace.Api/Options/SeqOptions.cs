using FluentValidation;
using JetBrains.Annotations;
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace BattleTrace.Api.Options;

#pragma warning disable CS8618
public class SeqOptions
{
    public Uri ServerUrl { get; init; } = new("http://localhost:5341");
    public string? ApiKey { get; init; }

    [UsedImplicitly]
    public class Validator : AbstractValidator<SeqOptions>
    {
        public Validator()
        {
            RuleFor(x => x.ServerUrl).NotEmpty();
        }
    }
}
