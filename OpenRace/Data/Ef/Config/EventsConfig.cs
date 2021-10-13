using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenRace.Entities;

namespace OpenRace.Data.Ef.Config
{
    // See: https://docs.microsoft.com/ru-ru/ef/core/managing-schemas/migrations/?tabs=dotnet-core-cli
    //1. dotnet ef migrations add AddEvents --context AppDbContext
    //2. dotnet ef database update --context AppDbContext
    public class EventsConfig : IEntityTypeConfiguration<RaceEvent>
    {
        public void Configure(EntityTypeBuilder<RaceEvent> builder)
        {
            builder.ToTable("Events");
            builder.HasKey(ci => ci.Id);
            builder.HasIndex(ci => ci.Distance);
            builder.HasIndex(ci => ci.RaceId);
            builder.HasIndex(ci => ci.MemberNumber);
            builder.HasIndex(ci => ci.EventType);
            builder.HasIndex(ci => ci.TimeStamp);
        }
    }
}
