using Microsoft.EntityFrameworkCore;

namespace OpenRace.Data.Ef
{
    public class ConnectionContext : DbContext
    {
        public ConnectionContext(DbContextOptions<ConnectionContext> options) : base(options)
        {
        }
    }
}