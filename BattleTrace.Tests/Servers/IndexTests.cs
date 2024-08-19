using BattleTrace.Data;
using BattleTrace.Data.Models;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Index = BattleTrace.Features.Servers.Actions.Index;

namespace BattleTrace.Tests.Servers;

public class IndexTests : AppFactoryTests
{
    private readonly AppFactory _appFactory;

    public IndexTests(AppFactory appFactory) : base(appFactory)
    {
        _appFactory = appFactory;
    }

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

        using (var scope = CreateScope())
        {
            var ctx = scope.GetRequiredService<AppDbContext>();

            ctx.Servers.Add(server);

            await ctx.SaveChangesAsync();
        }

        var query = new Index.Query();
        IResult result;


        // Act
        using (var scope = CreateScope())
        {
            var sender = scope.GetRequiredService<ISender>();

            result = await sender.Send(query);
        }


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
        using (var scope = CreateScope())
        {
            var ctx = scope.GetRequiredService<AppDbContext>();

            ctx.Servers.AddRange(
                [
                    Server.Db with {Id = "a"},
                    Server.Db with {Id = "b"},
                ]
            );

            await ctx.SaveChangesAsync();
        }

        var query = new Index.Query();
        IResult result;


        // Act
        using (var scope = CreateScope())
        {
            var sender = scope.GetRequiredService<ISender>();

            result = await sender.Send(query);
        }


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
