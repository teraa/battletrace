using System.Net;
using BattleTrace.Data;
using BattleTrace.Data.Models;
using BattleTrace.Features.Players;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Refit;

namespace BattleTrace.Tests.Players;

public class FetchPlayersTests : AppFactoryTests
{
    private readonly AppFactory _appFactory;

    public FetchPlayersTests(AppFactory appFactory) : base(appFactory)
    {
        _appFactory = appFactory;
    }

    [Fact]
    public async Task KeepsPlayers()
    {
        var time = DateTimeOffset.Parse("2000-01-01T00:00Z");

        var server = new Server
        {
            Name = "",
            IpAddress = "",
            Country = "",
            UpdatedAt = time - TimeSpan.FromHours(1),
        };

        var player = (
            db: new Player
            {
                Id = "",
                Name = "",
                NormalizedName = "",
                Tag = "",
                ServerId = "",
                UpdatedAt = time - TimeSpan.FromMinutes(10),
            },
            api: new IKeeperBattlelogApi.Player(
                Name: "",
                Tag: "",
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
                    player.db with {Id = "a1", ServerId = "a"},
                    player.db with {Id = "a2", ServerId = "a"},
                    player.db with {Id = "b1", ServerId = "b"},
                ]
            );

            await ctx.SaveChangesAsync();
        }

        _appFactory.KeeperBattlelogApiMock.Setup(x => x.GetSnapshot("a", It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new ApiResponse<IKeeperBattlelogApi.SnapshotResponse>(
                    response: new HttpResponseMessage(HttpStatusCode.OK),
                    content: new IKeeperBattlelogApi.SnapshotResponse(
                        new IKeeperBattlelogApi.Snapshot(
                            new Dictionary<string, IKeeperBattlelogApi.TeamInfo>
                            {
                                ["0"] = new(
                                    Faction: 0,
                                    Players: new Dictionary<string, IKeeperBattlelogApi.Player>
                                    {
                                        ["a1"] = player.api with { },
                                        ["a3"] = player.api with { },
                                    }
                                ),
                            }
                        )
                    ),
                    settings: null!
                )
            );

        _appFactory.KeeperBattlelogApiMock.Setup(x => x.GetSnapshot("b", It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new ApiResponse<IKeeperBattlelogApi.SnapshotResponse>(
                    response: new HttpResponseMessage(HttpStatusCode.NotFound),
                    content: null,
                    settings: null!
                )
            );

        _appFactory.TimeProviderMock.Setup(x => x.GetUtcNow()).Returns(time);


        using (var scope = CreateScope())
        {
            var handler = scope.GetRequiredService<FetchPlayers>();

            await handler.Handle();
        }


        using (var scope = CreateScope())
        {
            var ctx = scope.GetRequiredService<AppDbContext>();

            var servers = await ctx.Servers.ToListAsync();

            servers.Should().BeEquivalentTo(
                [
                    server with {Id = "a"},
                    server with {Id = "b"},
                ]
            );

            var players = await ctx.Players.ToListAsync();

            players.Should().BeEquivalentTo(
                [
                    player.db with {Id = "a1", ServerId = "a", UpdatedAt = time},
                    player.db with {Id = "a2", ServerId = "a"},
                    player.db with {Id = "a3", ServerId = "a", UpdatedAt = time},
                    player.db with {Id = "b1", ServerId = "b"},
                ]
            );

            var playerScans = await ctx.PlayerScans.ToListAsync();

            playerScans.Should().BeEquivalentTo(
                [
                    new PlayerScan
                    {
                        PlayerCount = 2,
                        Timestamp = time,
                    },
                ],
                options => options.Excluding(x => x.Id)
            );
        }
    }
}
