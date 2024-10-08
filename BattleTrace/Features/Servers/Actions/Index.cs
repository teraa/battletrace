﻿using BattleTrace.Common;
using BattleTrace.Data;
using FluentValidation;
using JetBrains.Annotations;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BattleTrace.Features.Servers.Actions;

public static class Index
{
    public sealed record Query(
        string? NamePattern = null,
        Guid? Id = null,
        [FromQuery(Name = "ip")] string? IpAddress = null,
        int? Limit = null
    ) : IRequest<IResult>;

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
        string IpAddress,
        int Port,
        DateTimeOffset UpdatedAt,
        int Players);

    [UsedImplicitly]
    public sealed class Handler : IRequestHandler<Query, IResult>
    {
        private readonly AppDbContext _ctx;

        public Handler(AppDbContext ctx)
        {
            _ctx = ctx;
        }

        public async Task<IResult> Handle(Query request, CancellationToken cancellationToken)
        {
            var query = _ctx.Servers.AsQueryable();

            if (request.Id is not null)
            {
                query = query.Where(x => x.Id == request.Id.ToString());
            }

            if (request.IpAddress is not null)
            {
                query = query.Where(x => x.IpAddress == request.IpAddress);
            }

            if (request.NamePattern is {Length: > 0})
            {
                var pattern = Helpers.StringToLikePattern(request.NamePattern);

                query = query.Where(x => EF.Functions.ILike(x.Name, pattern, @"\"));
            }

            var lastPlayerScan = await _ctx.PlayerScans
                .Select(x => x.Timestamp)
                .OrderByDescending(x => x)
                .FirstOrDefaultAsync(cancellationToken);

            var finalQuery = query
                .Select(
                    x => new
                    {
                        x.Id,
                        x.Name,
                        x.IpAddress,
                        x.Port,
                        x.UpdatedAt,
                        Players = x.Players.Count(p => p.UpdatedAt >= lastPlayerScan),
                    }
                )
                .OrderByDescending(x => x.Players)
                .AsQueryable();

            if (request.Limit is not null)
                finalQuery = finalQuery.Take(request.Limit.Value);

            var results = await finalQuery
                .Select(x => new Result(x.Id, x.Name, x.IpAddress, x.Port, x.UpdatedAt, x.Players))
                .ToListAsync(cancellationToken);

            return Results.Ok(results);
        }
    }
}
