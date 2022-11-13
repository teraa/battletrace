using BattleTrace.Data;
using FluentValidation;
using JetBrains.Annotations;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BattleTrace.Api.Features.Players.Actions;

public static class Index
{
    public record Query(
        [ModelBinder(Name = "id")] IReadOnlyList<string>? Ids,
        string? NamePattern,
        string? TagPattern,
        [ModelBinder(Name = "active")] bool ActiveOnly = false,
        int? Limit = null
    ) : IRequest<IActionResult>;

    [UsedImplicitly]
    public class QueryValidator : AbstractValidator<Query>
    {
        public QueryValidator()
        {
            RuleFor(x => x.Limit).GreaterThan(0);
        }
    }

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

        public Handler(AppDbContext ctx)
        {
            _ctx = ctx;
        }

        public async Task<IActionResult> Handle(Query request, CancellationToken cancellationToken)
        {
            var query = _ctx.Players.AsQueryable();

            if (request.Ids is {Count: > 0})
                query = query.Where(x => request.Ids.Contains(x.Id));

            if (request.NamePattern is {Length: > 0})
            {
                query = query.Where(x =>
                    EF.Functions.Glob(x.Name.ToLower(), request.NamePattern.ToLowerInvariant()));
            }

            if (request.TagPattern is {Length: > 0})
            {
                query = query.Where(x =>
                    EF.Functions.Glob(x.Tag.ToLower(), request.TagPattern.ToLowerInvariant()));
            }

            if (request.ActiveOnly)
            {
                var lastScan = await _ctx.PlayerScans
                    .Select(x => x.Timestamp)
                    .OrderByDescending(x => x)
                    .FirstOrDefaultAsync(cancellationToken);

                if (lastScan == default)
                    return new OkObjectResult(Array.Empty<Result>());

                query = query.Where(x => x.UpdatedAt >= lastScan);
            }

            query = query.OrderBy(x => x.Name);

            if (request.Limit is { })
                query = query.Take(request.Limit.Value);

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
