using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenRace.Entities;

namespace OpenRace.Data.Ef.Config
{
    public class PaymentsConfig : IEntityTypeConfiguration<Payment>
    {
        public void Configure(EntityTypeBuilder<Payment> builder)
        {
            builder.ToTable("Payments");
            
            builder.HasIndex(ci => ci.Id);
            builder.HasIndex(ci => ci.PaidAt);
            builder.HasIndex(ci => ci.Hash);
        }
    }
}
