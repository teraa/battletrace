using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using BattleTrace.Data.Models;

#pragma warning disable CS8618
namespace BattleTrace.Data.Models
{
    [PublicAPI]
    public class Server
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }

        public ICollection<Player> Players { get; set; }
    }
}

namespace BattleTrace.Data
{
    public partial class AppDbContext
    {
        public DbSet<Server> Servers { get; init; }
    }
}
