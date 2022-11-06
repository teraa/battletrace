using MediatR;
using Microsoft.AspNetCore.Mvc;
using Index = BattleTrace.Api.Features.Servers.Actions.Index;

namespace BattleTrace.Api.Features.Servers;

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
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
        => await _sender.Send(new Index.Query(), cancellationToken);
}
