using BattleTrace.Data;
using BattleTrace.Data.Models;
using JetBrains.Annotations;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BattleTrace.Api.Features.Players.Actions;

public static class Index
{
    public record Query(
        IReadOnlyList<string>? Id,
        bool ActiveOnly = false
    ) : IRequest<IActionResult>;

    [UsedImplicitly]
    public record Result(
        string Id,
        string Name,
        string Tag,
        string ServerId,
        string ServerName,
        DateTimeOffset UpdatedAt,
        int Faction,
        int Team,
        int Rank,
        long Score,
        int Kills,
        int Deaths,
        int Squad,
        int Role);

    [UsedImplicitly]
    public class Handler : IRequestHandler<Query, IActionResult>
    {
        private readonly AppDbContext _ctx;
        private readonly PlayerFetcherService _service;

        public Handler(AppDbContext ctx, PlayerFetcherService service)
        {
            _ctx = ctx;
            _service = service;
        }

        public async Task<IActionResult> Handle(Query request, CancellationToken cancellationToken)
        {
            IQueryable<Player> query = _ctx.Players;

            if (request.Id is {Count: > 0})
                query = query.Where(x => request.Id.Contains(x.Id));

            if (request.ActiveOnly)
                query = query.Where(x => x.UpdatedAt >= _service.SyncedAt);

            var results = await query
                .Select(x => new Result(
                    x.Id,
                    x.Name,
                    x.Tag,
                    x.ServerId,
                    x.Server.Name,
                    x.UpdatedAt,
                    x.Faction,
                    x.Team,
                    x.Rank,
                    x.Score,
                    x.Kills,
                    x.Deaths,
                    x.Squad,
                    x.Role))
                .ToListAsync(cancellationToken);

            return new OkObjectResult(results);
        }
    }
}
