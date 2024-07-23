using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using BattleTrace.Data.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

#pragma warning disable CS8618
namespace BattleTrace.Data.Models
{
    [PublicAPI]
    public class Player
    {
        public string Id { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public string ServerId { get; set; }
        public int Faction { get; set; }
        public int Team { get; set; }
        public string Name { get; set; }
        public string NormalizedName { get; set; }
        public string Tag { get; set; }
        public int Rank { get; set; }
        public long Score { get; set; }
        public int Kills { get; set; }
        public int Deaths { get; set; }
        public int Squad { get; set; }
        public int Role { get; set; }

        public Server Server { get; set; }
    }

    public class PlayerConfiguration : IEntityTypeConfiguration<Player>
    {
        public void Configure(EntityTypeBuilder<Player> builder)
        {
            builder.HasIndex(x => x.NormalizedName);
            builder.HasIndex(x => x.Tag);
            builder.HasIndex(x => x.UpdatedAt);
        }
    }
}

namespace BattleTrace.Data
{
    public partial class AppDbContext
    {
        public DbSet<Player> Players { get; init; }
    }
}
