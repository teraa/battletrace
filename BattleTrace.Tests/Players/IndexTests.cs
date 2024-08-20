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
    public IndexTests(AppFactory appFactory)
        : base(appFactory) { }

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


    private async Task AddPlayers(params Player[] players)
    {
        using var scope = CreateScope();
        var ctx = scope.GetRequiredService<AppDbContext>();

        ctx.Players.AddRange(players);

        await ctx.SaveChangesAsync();
    }

    private async Task<IResult> Send(Index.Query query)
    {
        using var scope = CreateScope();
        var sender = scope.GetRequiredService<ISender>();

        return await sender.Send(query);
    }


    [Fact]
    public async Task MapsResultCorrectly()
    {
        // Arrange
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

        await AddPlayers(player);

        var query = new Index.Query();


        // Act
        var result = await Send(query);


        // Assert
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
        // Arrange
        await AddPlayers(
            [
                Player.Db with {Id = "1", Server = Server with {Id = "a"}},
                Player.Db with {Id = "2", Server = Server with {Id = "b"}},
            ]
        );

        var query = new Index.Query();


        // Act
        var result = await Send(query);


        // Assert
        result.Should().BeOfType<Ok<List<Index.Result>>>()
            .Subject.Value.Should().Equal(
                [
                    Player.Api with {Id = "1", ServerId = "a"},
                    Player.Api with {Id = "2", ServerId = "b"},
                ]
            );
    }

    [Fact]
    public async Task IdsQuery_ReturnsMatching()
    {
        // Arrange
        using (var scope = CreateScope())
        {
            var ctx = scope.GetContext();

            var server = Server with {Id = "s1"};
            var player = Player.Db with {Server = server};

            ctx.Servers.Add(server);

            ctx.Players.AddRange(
                [
                    player with {Id = "a"},
                    player with {Id = "b"},
                    player with {Id = "c"},
                ]
            );

            await ctx.SaveChangesAsync();
        }

        var query = new Index.Query(Ids: ["a", "b"]);


        // Act
        var result = await Send(query);


        result.Should().BeOfType<Ok<List<Index.Result>>>()
            .Subject.Value.Should().Equal(
                [
                    Player.Api with {Id = "a", ServerId = "s1"},
                    Player.Api with {Id = "b", ServerId = "s1"},
                ]
            );
    }

    [Fact]
    public async Task ActiveOnlyQuery_ReturnsActive()
    {
        // Arrange
        var time = DateTimeOffset.Parse("2000-01-01T00:00Z");
        var second = TimeSpan.FromSeconds(1);

        using (var scope = CreateScope())
        {
            var ctx = scope.GetContext();

            var server = Server with {Id = "s1"};
            var player = Player.Db with {Server = server};

            ctx.Players.AddRange(
                [
                    player with {Id = "a", UpdatedAt = time + second},
                    player with {Id = "b", UpdatedAt = time},
                    player with {Id = "c", UpdatedAt = time - second},
                ]
            );

            ctx.PlayerScans.Add(
                new PlayerScan
                {
                    PlayerCount = 1,
                    Timestamp = time,
                }
            );

            await ctx.SaveChangesAsync();
        }

        var query = new Index.Query(ActiveOnly: true);


        // Act
        var result = await Send(query);
        result.Should().BeOfType<Ok<List<Index.Result>>>()
            .Subject.Value.Should().BeEquivalentTo(
                [
                    Player.Api with {Id = "a", ServerId = "s1", UpdatedAt = time + second},
                    Player.Api with {Id = "b", ServerId = "s1", UpdatedAt = time},
                ]
            );
    }

    [Fact]
    public async Task NamePatternQuery_ReturnsMatching()
    {
        // Arrange
        using (var scope = CreateScope())
        {
            var ctx = scope.GetContext();

            var server = Server with {Id = "s1"};
            var player = Player.Db with {Server = server};

            ctx.Servers.Add(server);

            ctx.Players.AddRange(
                [
                    player with {Id = "a", Name = "foo", NormalizedName = "foo"},
                    player with {Id = "b", Name = "bar", NormalizedName = "bar"},
                    player with {Id = "c", Name = "baz!", NormalizedName = "baz!"},
                ]
            );

            await ctx.SaveChangesAsync();
        }

        var query = new Index.Query(NamePattern: "ba?");


        // Act
        var result = await Send(query);


        result.Should().BeOfType<Ok<List<Index.Result>>>()
            .Subject.Value.Should().Equal(
                [
                    Player.Api with {Id = "b", ServerId = "s1", Name = "bar"},
                ]
            );
    }

    [Fact]
    public async Task TagPatternQuery_ReturnsMatching()
    {
        // Arrange
        using (var scope = CreateScope())
        {
            var ctx = scope.GetContext();

            var server = Server with {Id = "s1"};
            var player = Player.Db with {Server = server};

            ctx.Servers.Add(server);

            ctx.Players.AddRange(
                [
                    player with {Id = "a", Tag = "foo"},
                    player with {Id = "b", Tag = "bar"},
                    player with {Id = "c", Tag = "baz!"},
                ]
            );

            await ctx.SaveChangesAsync();
        }

        var query = new Index.Query(TagPattern: "ba?");


        // Act
        var result = await Send(query);


        result.Should().BeOfType<Ok<List<Index.Result>>>()
            .Subject.Value.Should().Equal(
                [
                    Player.Api with {Id = "b", ServerId = "s1", Tag = "bar"},
                ]
            );
    }

    [Fact]
    public async Task Limit_ReturnsAtMostLimit()
    {
        // Arrange
        using (var scope = CreateScope())
        {
            var ctx = scope.GetContext();

            var server = Server with {Id = "s1"};
            var players = Player.Db with {Server = server};

            ctx.Servers.Add(server);
            ctx.Players.AddRange(
                [
                    players with {Id = "a"},
                    players with {Id = "b"},
                    players with {Id = "c"},
                ]
            );

            await ctx.SaveChangesAsync();
        }

        var query = new Index.Query(Limit: 2);


        // Act
        var result = await Send(query);


        // Assert
        result.Should().BeOfType<Ok<List<Index.Result>>>()
            .Subject.Value.Should().Equal(
                [
                    Player.Api with {Id = "a", ServerId = "s1"},
                    Player.Api with {Id = "b", ServerId = "s1"},
                ]
            );
    }
}
