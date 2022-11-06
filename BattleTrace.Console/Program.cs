using System.Diagnostics;
using System.Net.Http.Json;

using var client = new HttpClient()
{
    DefaultRequestHeaders =
    {
        {"X-Requested-With", "XMLHttpRequest"},
    }
};

var servers = new Dictionary<string, Server>();
var i = 0;
var lastNewIndex = 0;
do
{
    if (i != 0)
        await Task.Delay(500);

    int offset = i * 45;
    Console.Write($"Request: {i}, Offset: {offset} ... ");

    var response = await client.GetFromJsonAsync<Response>(GetServersUrl(offset));
    Debug.Assert(response is not null);

    int serversCount = servers.Count;
    foreach (var server in response.Data)
    {
        servers[server.Guid] = server;
    }

    Console.WriteLine($"Got {servers.Count - serversCount} new servers, {servers.Count} total.");

    if (serversCount != servers.Count)
        lastNewIndex = i;

    i++;
} while (i < lastNewIndex + 10);

Console.WriteLine($"Discovered {servers.Count} servers");

static string GetServersUrl(int offset)
    => $"https://battlelog.battlefield.com/bf4/servers/getServers/pc/?offset={offset}&count=60";

record Response(
    IReadOnlyList<Server> Data);

record Server(
    string Guid,
    string Name,
    string Map,
    long MapMode,
    string Country,
    int TickRate,
    string Ip,
    int Port);
