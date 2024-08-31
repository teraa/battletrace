using BattleTrace.Data.Models;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BattleTrace.Data.Models
{
    [PublicAPI]
    public sealed class PlayerScan
    {
        public long Id { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public int PlayerCount { get; set; }

        public sealed class EntityTypeConfiguration : IEntityTypeConfiguration<PlayerScan>
        {
            public void Configure(EntityTypeBuilder<PlayerScan> builder)
            {
                builder.HasIndex(x => x.Timestamp);
            }
        }
    }
}

namespace BattleTrace.Data
{
    public partial class AppDbContext
    {
        public DbSet<PlayerScan> PlayerScans { get; init; }
    }
}
