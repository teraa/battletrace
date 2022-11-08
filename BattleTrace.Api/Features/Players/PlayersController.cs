using MediatR;
using Microsoft.AspNetCore.Mvc;
using Index = BattleTrace.Api.Features.Players.Actions.Index;

namespace BattleTrace.Api.Features.Players;

[ApiController]
[Route("[controller]")]
public class PlayersController : ControllerBase
{
    private readonly ISender _sender;

    public PlayersController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
        => await _sender.Send(new Index.Query(), cancellationToken);
}
