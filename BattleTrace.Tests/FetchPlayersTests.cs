﻿using System.Net;
using BattleTrace.Data;
using BattleTrace.Data.Models;
using BattleTrace.Features.Players;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Refit;

namespace BattleTrace.Tests;

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
        using var scope = _appFactory.Services.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<AppDbContext>();

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
                // Id =
                Name = "",
                NormalizedName = "",
                Tag = "",
                // ServerId =,
                UpdatedAt = time - TimeSpan.FromMinutes(10),
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

        _appFactory.TimeProviderMock.Setup(x => x.GetUtcNow()).Returns(time);

        var handler = scope.ServiceProvider.GetRequiredService<FetchPlayers>();


        await handler.Handle();


        var servers = await ctx.Servers.ToListAsync();

        servers.Should().BeEquivalentTo([
            server with {Id = "a"},
            server with {Id = "b"},
        ]);

        var players = await ctx.Players.ToListAsync();

        players.Should().BeEquivalentTo([
            player.db with {Id = "a1", ServerId = "a", UpdatedAt = time},
            player.db with {Id = "a2", ServerId = "a"},
            player.db with {Id = "a3", ServerId = "a", UpdatedAt = time},
            player.db with {Id = "b1", ServerId = "b"},
        ]);
    }
}
