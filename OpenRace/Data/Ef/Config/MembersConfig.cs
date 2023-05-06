using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenRace.Entities;

namespace OpenRace.Data.Ef.Config
{
    // See: https://docs.microsoft.com/ru-ru/ef/core/managing-schemas/migrations/?tabs=dotnet-core-cli
    //1. dotnet ef migrations add {MIGRATION_NAME} --context AppDbContext
    //2. dotnet ef database update --context AppDbContext
    public class MembersConfig : IEntityTypeConfiguration<Member>
    {
        public void Configure(EntityTypeBuilder<Member> builder)
        {
            builder.ToTable("Members");
            
            builder.HasKey(ci => ci.Id);
            builder.HasIndex(ci => ci.Phone);
            builder.HasIndex(ci => ci.Email);
            builder.HasIndex(ci => ci.FullName);
            builder.Navigation(it => it.Payment).AutoInclude();
        }
    }
}
