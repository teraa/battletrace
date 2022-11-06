using MediatR;
using Microsoft.AspNetCore.Mvc;

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
    public IActionResult Index(CancellationToken cancellationToken)
        => Ok("Hi");
}
