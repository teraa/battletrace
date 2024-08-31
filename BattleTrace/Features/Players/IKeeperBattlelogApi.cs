using Refit;

namespace BattleTrace.Features.Players;

public interface IKeeperBattlelogApi
{
    [Get("/snapshot/{serverId}")]
    Task<ApiResponse<SnapshotResponse>> GetSnapshot(
        string serverId,
        CancellationToken cancellationToken
    );

    public record SnapshotResponse(Snapshot Snapshot);

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
}

