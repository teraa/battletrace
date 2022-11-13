using BattleTrace.Data;
using BattleTrace.Data.Models;
using FluentValidation;
using JetBrains.Annotations;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BattleTrace.Api.Features.Servers.Actions;

public static class Index
{
    public record Query(
        string? NamePattern,
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
            var query = _ctx.Servers.AsQueryable();

            if (request.NamePattern is {Length: > 0})
            {
                query = query.Where(x =>
                    EF.Functions.Glob(x.Name.ToLower(), request.NamePattern.ToLowerInvariant()));
            }

            if (request.Limit is { })
                query = query.Take(request.Limit.Value);

            var results = await query
                .Select(x => new Result(x.Id, x.Name, x.UpdatedAt))
                .ToListAsync(cancellationToken);

            return new OkObjectResult(results);
        }
    }
}
