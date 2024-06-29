namespace BattleTrace.Features.Players;

public sealed class Client
{
    private readonly HttpClient _client;

    public Client(HttpClient client)
    {
        _client = client;
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
    }

    public Task<HttpResponseMessage> GetServerSnapshot(string serverId, CancellationToken cancellationToken)
    {
        return _client.GetAsync($"https://keeper.battlelog.com/snapshot/{serverId}", cancellationToken);
    }
};
