// ReSharper disable ClassNeverInstantiated.Global

namespace BattleTrace.Features.Players;

public sealed class Client
{
    private readonly HttpClient _client;
    private readonly ILogger<Client> _logger;

    public Client(HttpClient client, ILogger<Client> logger)
    {
        _client = client;
        _logger = logger;
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
    }

    public async Task<Response?> GetServerSnapshot(string serverId,
        CancellationToken cancellationToken)
    {
        using var httpResponse =
            await _client.GetAsync($"https://keeper.battlelog.com/snapshot/{serverId}", cancellationToken);

        if (httpResponse.IsSuccessStatusCode)
            return await httpResponse.Content.ReadFromJsonAsync<Response>(cancellationToken);

        _logger.LogDebug(
            "Failed fetching players for {ServerId}, server returned: {StatusCode}: {ReasonPhrase}", serverId,
            (int) httpResponse.StatusCode, httpResponse.ReasonPhrase);

        return null;
    }


    public record Response(Snapshot Snapshot);

    public record Snapshot(Dictionary<string, TeamInfo> TeamInfo);

    public record TeamInfo(int Faction, Dictionary<string, Player> Players);

    public record Player(
        string Name,
        string Tag,
        int Rank,
        long Score,
        int Kills,
        int Deaths,
        int Squad,
        int Role);
};
