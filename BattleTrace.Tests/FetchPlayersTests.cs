using System.Net;
using BattleTrace.Data;
using BattleTrace.Data.Models;
using BattleTrace.Features.Players;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Refit;

namespace BattleTrace.Tests;

public class FetchPlayersTests(AppFactory appFactory)
    : AppFactoryTests(appFactory)
{
    [Fact]
    public async Task KeepsPlayers()
    {
        using var scope = _appFactory.Services.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var server = new Server
        {
            Name = "",
            IpAddress = "",
            Country = "",
            UpdatedAt = DateTimeOffset.UtcNow,
        };

        var player = (
            db: new Player
            {
                // Id =
                Name = "",
                NormalizedName = "",
                Tag = "",
                // ServerId =
            },
            api: new IKeeperBattlelogApi.Player("", "", 0, 0, 0, 0, 0, 0)
        );

        ctx.Servers.AddRange([
            server with
            {
                Id = "a",
                Players =
                [
                    player.db with {Id = "a1"},
                    player.db with {Id = "a2"},
                ],
            },
            server with
            {
                Id = "b",
                Players =
                [
                    player.db with {Id = "b1"},
                ],
            }
        ]);

        await ctx.SaveChangesAsync();
        ctx.ChangeTracker.Clear();

        _appFactory.KeeperBattlelogApiMock.Setup(x => x.GetSnapshot("a", It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new ApiResponse<IKeeperBattlelogApi.SnapshotResponse>(
                    new HttpResponseMessage(HttpStatusCode.OK),
                    new IKeeperBattlelogApi.SnapshotResponse(
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
                            })),
                    null!
                )
            );

        _appFactory.KeeperBattlelogApiMock.Setup(x => x.GetSnapshot("b", It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new ApiResponse<IKeeperBattlelogApi.SnapshotResponse>(
                    new HttpResponseMessage(HttpStatusCode.NotFound),
                    null,
                    null!
                )
            );

        var handler = scope.ServiceProvider.GetRequiredService<FetchPlayers>();


        await handler.Handle();


        var servers = await ctx.Servers.ToListAsync();

        foreach (var s in servers)
        {
            s.UpdatedAt = default;
        }

        servers.Should().BeEquivalentTo([
            server with {Id = "a", UpdatedAt = default},
            server with {Id = "b", UpdatedAt = default},
        ]);

        var players = await ctx.Players.ToListAsync();

        foreach (var p in players)
        {
            p.UpdatedAt = default;
        }

        players.Should().BeEquivalentTo([
            player.db with {Id = "a1", ServerId = "a", UpdatedAt = default},
            player.db with {Id = "a2", ServerId = "a", UpdatedAt = default},
            player.db with {Id = "a3", ServerId = "a", UpdatedAt = default},
            player.db with {Id = "b1", ServerId = "b", UpdatedAt = default},
        ]);
    }
}
