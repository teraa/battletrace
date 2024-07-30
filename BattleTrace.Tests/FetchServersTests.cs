﻿using BattleTrace.Data;
using BattleTrace.Data.Models;
using BattleTrace.Features.Servers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace BattleTrace.Tests;

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
        using var scope = _appFactory.Services.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<AppDbContext>();

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

        ctx.Servers.AddRange([
            server.db with {Id = "a", Players = [playerDb with {Id = "1"}]},
            server.db with {Id = "b", Players = [playerDb with {Id = "2"}]},
        ]);

        await ctx.SaveChangesAsync();
        ctx.ChangeTracker.Clear();

        _appFactory.BattlelogApiMock.Setup(x => x.GetServers(0, It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new IBattlelogApi.ServersResponse([
                    server.api with {Guid = "b", Port = 2},
                    server.api with {Guid = "c", Port = 2},
                ])
            );

        _appFactory.BattlelogApiMock
            .Setup(x => x.GetServers(It.IsNotIn(0), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new IBattlelogApi.ServersResponse([]));

        var handler = scope.ServiceProvider.GetRequiredService<FetchServers>();


        await handler.Handle();


        var servers = await ctx.Servers.ToListAsync();

        foreach (var s in servers)
        {
            s.UpdatedAt = default;
        }

        servers.Should().BeEquivalentTo([
            server.db with {Id = "a"},
            server.db with {Id = "b", Port = 2},
            server.db with {Id = "c", Port = 2},
        ]);

        var players = await ctx.Players.ToListAsync();

        players.Should().BeEquivalentTo([
            playerDb with {Id = "1", ServerId = "a"},
            playerDb with {Id = "2", ServerId = "b"},
        ]);
    }
}