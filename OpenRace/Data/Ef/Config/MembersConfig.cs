using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenRace.Entities;

namespace OpenRace.Data.Ef.Config
{
    // See: https://docs.microsoft.com/ru-ru/ef/core/managing-schemas/migrations/?tabs=dotnet-core-cli
    //1. dotnet ef migrations add {MIGRATION_NAME} --context RaceDbContext
    //2. dotnet ef database update --context RaceDbContext
    public class MembersConfig : IEntityTypeConfiguration<Member>
    {
        public void Configure(EntityTypeBuilder<Member> builder)
        {
            builder.ToTable("Members2021");
            
            builder.HasKey(ci => ci.Id);
            builder.HasIndex(ci => ci.Phone);
            builder.HasIndex(ci => ci.Email);
            builder.HasIndex(ci => ci.FullName);

            builder.Property(cb => cb.Email)
                .IsRequired()
                .HasMaxLength(100);
        }
    }
}
