using BattleTrace.Common;
using BattleTrace.Data;
using FluentValidation;
using Immediate.Handlers.Shared;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BattleTrace.Features.Players.Actions;

[Handler]
public static partial class Index
{
    public sealed record Query(
        [FromQuery(Name = "id")] string[]? Ids = null,
        string? NamePattern = null,
        string? TagPattern = null,
        [FromQuery(Name = "active")] bool ActiveOnly = false,
        int? Limit = null
    );

    [UsedImplicitly]
    public sealed class QueryValidator : AbstractValidator<Query>
    {
        public QueryValidator()
        {
            RuleFor(x => x.Limit).GreaterThan(0);
        }
    }

    [UsedImplicitly]
    public sealed record Result(
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

    private static async ValueTask<IResult> HandleAsync(
        Query request,
        AppDbContext ctx,
        CancellationToken cancellationToken)
    {
        var query = ctx.Players.AsQueryable();

        if (request.Ids is {Length: > 0})
            query = query.Where(x => request.Ids.AsEnumerable().Contains(x.Id));

        if (request.NamePattern is {Length: > 0})
        {
            var pattern = Helpers.StringToLikePattern(request.NamePattern.ToLowerInvariant());

            query = query.Where(x => EF.Functions.Like(x.NormalizedName, pattern, @"\"));
        }

        if (request.TagPattern is {Length: > 0})
        {
            var pattern = Helpers.StringToLikePattern(request.TagPattern);

            query = query.Where(x => EF.Functions.ILike(x.Tag, pattern, @"\"));
        }

        if (request.ActiveOnly)
        {
            var lastScan = await ctx.PlayerScans
                .Select(x => x.Timestamp)
                .OrderByDescending(x => x)
                .FirstOrDefaultAsync(cancellationToken);

            if (lastScan == default)
                return Results.Ok(Array.Empty<Result>());

            query = query.Where(x => x.UpdatedAt >= lastScan);
        }

        query = query.OrderBy(x => x.NormalizedName);

        if (request.Limit is not null)
            query = query.Take(request.Limit.Value);

        var results = await query
            .Select(
                x => new Result(
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
                    x.Role
                )
            )
            .ToListAsync(cancellationToken);

        return Results.Ok(results);
    }
}
