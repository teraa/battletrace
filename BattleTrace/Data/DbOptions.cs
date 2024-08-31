using FluentValidation;
using JetBrains.Annotations;

namespace BattleTrace.Data;

public sealed class DbOptions
{
    public string ConnectionString { get; init; }

    [UsedImplicitly]
    public sealed class Validator : AbstractValidator<DbOptions>
    {
        public Validator()
        {
            RuleFor(x => x.ConnectionString).NotEmpty();
        }
    }
}
