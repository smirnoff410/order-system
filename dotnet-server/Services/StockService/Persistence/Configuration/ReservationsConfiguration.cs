using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockService.Models;

namespace StockService.Persistence.Configuration
{
    public class ReservationsConfiguration : IEntityTypeConfiguration<Reservation>
    {
        public void Configure(EntityTypeBuilder<Reservation> builder)
        {
            builder.Property(x => x.Id)
                .ValueGeneratedNever();

            builder.Property(x => x.Quantity);
            builder.Property(x => x.ReservedAt);
            builder.Property(x => x.IsActive);
            builder.Property(x => x.ProductId);
            builder.Property(x => x.OrderId);
        }
    }
}
