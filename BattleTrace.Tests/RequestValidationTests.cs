using System.Net;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using IndexPlayers = BattleTrace.Features.Players.Actions.Index;
using IndexServers = BattleTrace.Features.Servers.Actions.Index;

namespace BattleTrace.Tests;

public class RequestValidationTests(AppFactory appFactory)
    : AppFactoryTests(appFactory)
{
    [Fact]
    public async Task IndexPlayers_ReturnsBadRequest_WhenInvalid()
    {
        using var scope = CreateScope();
        var sender = scope.GetRequiredService<ISender>();
        var request = new IndexPlayers.Query(null, null, null, Limit: 0);

        var response = await sender.Send(request);

        response.Should().BeOfType<BadRequest<ValidationProblemDetails>>()
            .Subject.Value!.Errors.Keys.Should().BeEquivalentTo(["Limit"]);
    }

    [Fact]
    public async Task IndexServers_ReturnsBadRequest_WhenInvalid()
    {
        using var scope = CreateScope();
        var sender = scope.GetRequiredService<ISender>();
        var request = new IndexServers.Query(null, null, null, Limit: 0);

        var response = await sender.Send(request);

        response.Should().BeOfType<BadRequest<ValidationProblemDetails>>()
            .Subject.Value!.Errors.Keys.Should().BeEquivalentTo(["Limit"]);
    }

    [Fact]
    public async Task GetPlayers_ReturnsOk()
    {
        var client = appFactory.CreateClient();

        var response = await client.GetAsync("/players");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetPlayersWithLimit0_ReturnsBadRequest()
    {
        var client = appFactory.CreateClient();

        var response = await client.GetAsync("/players?limit=0");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetServers_ReturnsOk()
    {
        var client = appFactory.CreateClient();

        var response = await client.GetAsync("/servers");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetServersWithLimit0_ReturnsBadRequest()
    {
        var client = appFactory.CreateClient();

        var response = await client.GetAsync("/servers?limit=0");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
