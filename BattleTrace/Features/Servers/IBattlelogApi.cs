using Refit;

namespace BattleTrace.Features.Servers;

public interface IBattlelogApi
{
    [Get("/bf4/servers/getServers/pc/")]
    Task<ServersResponse> GetServers(
        int offset,
        int count = 60,
        CancellationToken cancellationToken = default
    );

    public sealed record ServersResponse(IReadOnlyList<Server> Data);

    public sealed record Server(
        string Guid,
        string Name,
        string Map,
        string MapMode,
        string Country,
        int TickRate,
        string Ip,
        int Port
    );
}
