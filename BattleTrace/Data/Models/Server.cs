using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using BattleTrace.Data.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BattleTrace.Data.Models
{
    [PublicAPI]
    public record Server
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string IpAddress { get; set; }
        public int Port { get; set; }
        public string Country { get; set; }
        public int TickRate { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }

        public ICollection<Player> Players { get; set; }
    }

    public class ServerConfiguration : IEntityTypeConfiguration<Server>
    {
        public void Configure(EntityTypeBuilder<Server> builder)
        {
            builder.HasIndex(x => x.Name);
            builder.HasIndex(x => x.IpAddress);
        }
    }
}

namespace BattleTrace.Data
{
    public partial class AppDbContext
    {
        public DbSet<Server> Servers { get; init; }
    }
}
