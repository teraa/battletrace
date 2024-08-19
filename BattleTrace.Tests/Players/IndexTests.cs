using BattleTrace.Data;
using BattleTrace.Data.Models;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Index = BattleTrace.Features.Players.Actions.Index;

namespace BattleTrace.Tests.Players;

public class IndexTests : AppFactoryTests
{
    private readonly AppFactory _appFactory;

    public IndexTests(AppFactory appFactory) : base(appFactory)
    {
        _appFactory = appFactory;
    }

    [Fact]
    public async Task DefaultQuery_ReturnsAllPlayers()
    {
        var time = DateTimeOffset.Parse("2000-01-01T00:00Z");

        var server = new Server
        {
            Id = "",
            Name = "",
            IpAddress = "",
            Country = "",
            UpdatedAt = time,
        };

        var player = (
            db: new Player
            {
                Id = "",
                Name = "",
                NormalizedName = "",
                Tag = "",
                ServerId = "",
                UpdatedAt = time,
                Faction = 0,
                Team = 0,
                Rank = 0,
                Score = 0,
                Kills = 0,
                Deaths = 0,
                Squad = 0,
                Role = 0,
            },
            api: new Index.Result(
                Id: "",
                Name: "",
                Tag: "",
                ServerId: "",
                ServerName: "",
                UpdatedAt: time,
                Faction: 0,
                Team: 0,
                Rank: 0,
                Score: 0,
                Kills: 0,
                Deaths: 0,
                Squad: 0,
                Role: 0
            )
        );

        using (var scope = CreateScope())
        {
            var ctx = scope.GetRequiredService<AppDbContext>();

            ctx.Servers.AddRange(
                [
                    server with {Id = "a"},
                    server with {Id = "b"},
                ]
            );

            ctx.Players.AddRange(
                [
                    player.db with {Id = "1", ServerId = "a"},
                    player.db with {Id = "2", ServerId = "b"},
                ]
            );

            await ctx.SaveChangesAsync();
        }

        var query = new Index.Query();
        IResult result;

        using (var scope = CreateScope())
        {
            var sender = scope.GetRequiredService<ISender>();

            result = await sender.Send(query);
        }

        result.Should().BeOfType<Ok<List<Index.Result>>>()
            .Subject.Value.Should().Equal(
                [
                    player.api with {Id = "1", ServerId = "a"},
                    player.api with {Id = "2", ServerId = "b"},
                ]
            );
    }
}
