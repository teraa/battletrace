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

    private (Player Db, Index.Result Api) Player { get; } = (
        new Player
        {
            Name = "",
            NormalizedName = "",
            Tag = "",
        },
        new Index.Result(
            Id: "",
            Name: "",
            Tag: "",
            ServerId: "",
            ServerName: "",
            UpdatedAt: default,
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

    private Server Server { get; } = new()
    {
        Name = "",
        IpAddress = "",
        Country = "",
    };


    [Fact]
    public async Task MapsResultCorrectly()
    {
        var player = new Player
        {
            Id = "id",
            UpdatedAt = DateTimeOffset.Parse("2000-01-01T00:00Z"),
            Faction = 1,
            Team = 2,
            Name = "name",
            NormalizedName = "normalized_name",
            Tag = "tag",
            Rank = 3,
            Score = 4,
            Kills = 5,
            Deaths = 6,
            Squad = 7,
            Role = 8,
            Server = Server with
            {
                Id = "server.id",
                Name = "server.name",
            },
        };


        using (var scope = CreateScope())
        {
            var ctx = scope.GetRequiredService<AppDbContext>();

            ctx.Players.Add(player);

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
                    new Index.Result(
                        Id: player.Id,
                        Name: player.Name,
                        Tag: player.Tag,
                        ServerId: player.Server.Id,
                        ServerName: player.Server.Name,
                        UpdatedAt: player.UpdatedAt,
                        Faction: player.Faction,
                        Team: player.Team,
                        Rank: player.Rank,
                        Score: player.Score,
                        Kills: player.Kills,
                        Deaths: player.Deaths,
                        Squad: player.Squad,
                        Role: player.Role
                    ),
                ]
            );
    }

    [Fact]
    public async Task DefaultQuery_ReturnsAllPlayers()
    {
        using (var scope = CreateScope())
        {
            var ctx = scope.GetRequiredService<AppDbContext>();

            ctx.Servers.AddRange(
                [
                    Server with {Id = "a"},
                    Server with {Id = "b"},
                ]
            );

            ctx.Players.AddRange(
                [
                    Player.Db with {Id = "1", ServerId = "a"},
                    Player.Db with {Id = "2", ServerId = "b"},
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
                    Player.Api with {Id = "1", ServerId = "a"},
                    Player.Api with {Id = "2", ServerId = "b"},
                ]
            );
    }
}
