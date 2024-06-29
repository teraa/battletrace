using System.Diagnostics;

namespace BattleTrace.Features.Servers;

public sealed class Client
{
    private readonly HttpClient _client;

    public Client(HttpClient client)
    {
        _client = client;
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
    }

    public async Task<Fetch.Handler.Response> GetServers(int offset, CancellationToken cancellationToken)
    {
        var response = await _client.GetFromJsonAsync<Fetch.Handler.Response>(
            $"https://battlelog.battlefield.com/bf4/servers/getServers/pc/?offset={offset}&count=60",
            cancellationToken);

        Debug.Assert(response is not null);

        return response;
    }
};
