using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using IndexPlayers = BattleTrace.Features.Players.Actions.Index;
using IndexServers = BattleTrace.Features.Servers.Actions.Index;

namespace BattleTrace.Tests;

public class ValidationTests(AppFactory appFactory)
    : AppFactoryTests(appFactory)
{
    [Fact]
    public async Task IndexPlayers_ReturnsBadRequest_WhenInvalid()
    {
        using var scope = CreateScope();
        var sender = scope.GetRequiredService<ISender>();
        var request = new IndexPlayers.Query(null, null, null, Limit: 0);

        var response = await sender.Send(request);

        response.Should().BeOfType<BadRequestObjectResult>()
            .Subject.Value.Should().BeOfType<ValidationProblemDetails>();
    }

    [Fact]
    public async Task IndexServers_ReturnsBadRequest_WhenInvalid()
    {
        using var scope = CreateScope();
        var sender = scope.GetRequiredService<ISender>();
        var request = new IndexServers.Query(null, null, null, Limit: 0);

        var response = await sender.Send(request);

        response.Should().BeOfType<BadRequestObjectResult>()
            .Subject.Value.Should().BeOfType<ValidationProblemDetails>();
    }
}
