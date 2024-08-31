using Microsoft.AspNetCore.Http;
using Index = BattleTrace.Features.Servers.Actions.Index;

namespace BattleTrace.Tests.Servers;

public class IndexTests(AppFactory appFactory) : AppTests(appFactory)
{
    private (Server Db, Index.Result Api) Server { get; } = (
        new Server
        {
            Id = "",
            Name = "",
            IpAddress = "",
            Country = "",
        },
        new Index.Result(
            Id: "",
            Name: "",
            IpAddress: "",
            Port: 0,
            UpdatedAt: default,
            Players: 0
        )
    );

    private Player Player { get; } = new()
    {
        Name = "",
        NormalizedName = "",
        Tag = "",
    };


    private async Task AddServers(params Server[] servers)
    {
        using var scope = CreateScope();
        var ctx = scope.GetRequiredService<AppDbContext>();

        ctx.Servers.AddRange(servers);

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
        var server = new Server
        {
            Id = "id",
            Name = "name",
            IpAddress = "ip",
            Port = 10,
            Country = "country",
            TickRate = 20,
            UpdatedAt = DateTimeOffset.Parse("2000-01-01T00:00Z"),
            Players =
            [
                Player with {Id = "1"},
                Player with {Id = "2"},
            ],
        };

        await AddServers(server);

        var query = new Index.Query();


        // Act
        var result = await Send(query);


        // Assert
        result.Should().BeOfType<Ok<List<Index.Result>>>()
            .Subject.Value.Should().Equal(
                [
                    new Index.Result(
                        Id: server.Id,
                        Name: server.Name,
                        IpAddress: server.IpAddress,
                        Port: server.Port,
                        UpdatedAt: server.UpdatedAt,
                        Players: server.Players.Count
                    ),
                ]
            );
    }


    [Fact]
    public async Task DefaultQuery_ReturnsAllServers()
    {
        // Arrange
        await AddServers(
            [
                Server.Db with {Id = "a"},
                Server.Db with {Id = "b"},
            ]
        );


        // Act
        var result = await Send(new Index.Query());


        // Assert
        result.Should().BeOfType<Ok<List<Index.Result>>>()
            .Subject.Value.Should().Equal(
                [
                    Server.Api with {Id = "a"},
                    Server.Api with {Id = "b"},
                ]
            );
    }

    [Fact]
    public async Task NamePatternQuery_ReturnsMatching()
    {
        // Arrange
        await AddServers(
            [
                Server.Db with {Id = "1", Name = "foo"},
                Server.Db with {Id = "2", Name = "bar"},
                Server.Db with {Id = "3", Name = "baz"},
            ]
        );

        var query = new Index.Query(NamePattern: "ba?");


        // Act
        var result = await Send(query);


        // Assert
        result.Should().BeOfType<Ok<List<Index.Result>>>()
            .Subject.Value.Should().Equal(
                [
                    Server.Api with {Id = "2", Name = "bar"},
                    Server.Api with {Id = "3", Name = "baz"},
                ]
            );
    }

    [Fact]
    public async Task IdQuery_ReturnsMatching()
    {
        // Arrange
        var servers = Enumerable.Range(0, 2)
            .Select(_ => Server.Db with {Id = Guid.NewGuid().ToString()})
            .ToArray();

        await AddServers(servers);

        var query = new Index.Query(Id: Guid.Parse(servers[1].Id));


        // Act
        var result = await Send(query);


        // Assert
        result.Should().BeOfType<Ok<List<Index.Result>>>()
            .Subject.Value.Should().Equal(
                [
                    Server.Api with {Id = servers[1].Id},
                ]
            );
    }

    [Fact]
    public async Task IpAddressQuery_ReturnsMatching()
    {
        // Arrange
        await AddServers(
            [
                Server.Db with {Id = "a", IpAddress = "1"},
                Server.Db with {Id = "b", IpAddress = "2"},
            ]
        );

        var query = new Index.Query(IpAddress: "2");


        // Act
        var result = await Send(query);


        // Assert
        result.Should().BeOfType<Ok<List<Index.Result>>>()
            .Subject.Value.Should().Equal(
                [
                    Server.Api with {Id = "b", IpAddress = "2"},
                ]
            );
    }

    [Fact]
    public async Task Limit_ReturnsAtMostLimit()
    {
        // Arrange
        await AddServers(
            [
                Server.Db with {Id = "a"},
                Server.Db with {Id = "b"},
                Server.Db with {Id = "c"},
            ]
        );

        var query = new Index.Query(Limit: 2);


        // Act
        var result = await Send(query);


        // Assert
        result.Should().BeOfType<Ok<List<Index.Result>>>()
            .Subject.Value.Should().Equal(
                [
                    Server.Api with {Id = "a"},
                    Server.Api with {Id = "b"},
                ]
            );
    }
}
