using BattleTrace.Data;
using JetBrains.Annotations;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BattleTrace.Api.Features.Servers.Actions;

public static class Index
{
    public record Query : IRequest<IActionResult>;

    [UsedImplicitly]
    public record Result(
        string Id,
        string Name,
        DateTimeOffset UpdatedAt);

    [UsedImplicitly]
    public class Handler : IRequestHandler<Query, IActionResult>
    {
        private readonly AppDbContext _ctx;

        public Handler(AppDbContext ctx)
        {
            _ctx = ctx;
        }

        public async Task<IActionResult> Handle(Query request, CancellationToken cancellationToken)
        {
            var results = await _ctx.Servers
                .Select(x => new Result(x.Id, x.Name, x.UpdatedAt))
                .ToListAsync(cancellationToken);

            return new OkObjectResult(results);
        }
    }
}
