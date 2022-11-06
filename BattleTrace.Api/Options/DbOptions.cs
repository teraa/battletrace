using FluentValidation;
using JetBrains.Annotations;
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace BattleTrace.Api.Options;

#pragma warning disable CS8618
public class DbOptions
{
    public string ConnectionString { get; init; }

    [UsedImplicitly]
    public class Validator : AbstractValidator<DbOptions>
    {
        public Validator()
        {
            RuleFor(x => x.ConnectionString).NotEmpty();
        }
    }
}
