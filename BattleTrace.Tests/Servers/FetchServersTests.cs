﻿using BattleTrace.Data;
using BattleTrace.Data.Models;
using BattleTrace.Features.Servers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace BattleTrace.Tests.Servers;

public class FetchServersTests : AppFactoryTests
{
    private readonly AppFactory _appFactory;

    public FetchServersTests(AppFactory appFactory) : base(appFactory)
    {
        _appFactory = appFactory;
    }

    [Fact]
    public async Task KeepsPlayers()
    {
        var server = (
            db: new Server
            {
                Country = "",
                IpAddress = "",
                Name = "",
            },
            api: new IBattlelogApi.Server(
                Guid: "",
                Name: "",
                Map: "",
                MapMode: "",
                Country: "",
                TickRate: 0,
                Ip: "",
                Port: 0
            )
        );

        var playerDb = new Player
        {
            Id = "",
            Name = "",
            NormalizedName = "",
            Tag = "",
        };

        using (var scope = CreateScope())
        {
            var ctx = scope.GetRequiredService<AppDbContext>();

            ctx.Servers.AddRange(
                [
                    server.db with {Id = "a"},
                    server.db with {Id = "b"},
                ]
            );

            ctx.Players.AddRange(
                [
                    playerDb with {Id = "1", ServerId = "a"},
                    playerDb with {Id = "2", ServerId = "b"},
                ]
            );

            await ctx.SaveChangesAsync();
        }

        _appFactory.BattlelogApiMock.Setup(x => x.GetServers(0, It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new IBattlelogApi.ServersResponse(
                    [
                        server.api with {Guid = "b"},
                        server.api with {Guid = "c"},
                    ]
                )
            );

        _appFactory.BattlelogApiMock
            .Setup(x => x.GetServers(It.IsNotIn(0), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new IBattlelogApi.ServersResponse([]));

        var time = DateTimeOffset.Parse("2000-01-01T00:00Z");
        _appFactory.TimeProviderMock.Setup(x => x.GetUtcNow()).Returns(time);


        using (var scope = CreateScope())
        {
            var handler = scope.GetRequiredService<FetchServers>();

            await handler.Handle();
        }


        using (var scope = CreateScope())
        {
            var ctx = scope.GetRequiredService<AppDbContext>();

            var servers = await ctx.Servers.ToListAsync();

            servers.Should().BeEquivalentTo(
                [
                    server.db with {Id = "a"},
                    server.db with {Id = "b", UpdatedAt = time},
                    server.db with {Id = "c", UpdatedAt = time},
                ]
            );

            var players = await ctx.Players.ToListAsync();

            players.Should().BeEquivalentTo(
                [
                    playerDb with {Id = "1", ServerId = "a"},
                    playerDb with {Id = "2", ServerId = "b"},
                ]
            );

            var serverScans = await ctx.ServerScans.ToListAsync();

            serverScans.Should().BeEquivalentTo(
                [
                    new ServerScan
                    {
                        ServerCount = 2,
                        Timestamp = time,
                    },
                ],
                options => options.Excluding(x => x.Id)
            );
        }
    }
}