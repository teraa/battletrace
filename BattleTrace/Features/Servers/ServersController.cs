using MediatR;
using Microsoft.AspNetCore.Mvc;
using Index = BattleTrace.Features.Servers.Actions.Index;

namespace BattleTrace.Features.Servers;

[ApiController]
[Route("[controller]")]
public class ServersController : ControllerBase
{
    private readonly ISender _sender;

    public ServersController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] Index.Query query, CancellationToken cancellationToken)
        => await _sender.Send(query, cancellationToken);
}
