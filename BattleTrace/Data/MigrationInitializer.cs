using Extensions.Hosting.AsyncInitialization;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace BattleTrace.Data;

[UsedImplicitly]
public class MigrationInitializer : IAsyncInitializer
{
    private readonly AppDbContext _ctx;

    public MigrationInitializer(AppDbContext ctx)
    {
        _ctx = ctx;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        if (_ctx.Database.HasPendingModelChanges())
            throw new InvalidOperationException("Changes have been made to the model since the last migration. Add a new migration.");

        await _ctx.Database.MigrateAsync(cancellationToken);
    }
}
