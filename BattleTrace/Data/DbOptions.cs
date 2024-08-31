using FluentValidation;
using JetBrains.Annotations;

namespace BattleTrace.Data;

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
