using System.Reflection;
using Microsoft.EntityFrameworkCore;
using OpenRace.Entities;

namespace OpenRace.Data.Ef
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        
        public DbSet<Member> Members => Set<Member>();
        public DbSet<RaceEvent> Events => Set<RaceEvent>();
        public DbSet<MemberResult> Results => Set<MemberResult>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }
    }
}