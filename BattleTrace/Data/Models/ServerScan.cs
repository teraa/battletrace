using BattleTrace.Data.Models;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BattleTrace.Data.Models
{
    [PublicAPI]
    public sealed class ServerScan
    {
        public long Id { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public int ServerCount { get; set; }

        public sealed class EntityTypeConfiguration : IEntityTypeConfiguration<ServerScan>
        {
            public void Configure(EntityTypeBuilder<ServerScan> builder)
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
        public DbSet<ServerScan> ServerScans { get; init; }
    }
}
