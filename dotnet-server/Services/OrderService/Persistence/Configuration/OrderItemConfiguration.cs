using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderService.Models;

namespace OrderService.Persistence.Configuration
{
    public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
    {
        public void Configure(EntityTypeBuilder<OrderItem> builder)
        {
            builder.ToTable("order_item");

            builder.Property(x => x.Id)
                .ValueGeneratedNever();

            builder.Property(x => x.OrderId);
            builder.Property(x => x.ProductId);
            builder.Property(x => x.Quantity);
            builder.Property(x => x.UnitPrice);

            // Внешние ключи с правильными именами
            builder.HasOne(e => e.Order)
                  .WithMany(m => m.Items)
                  .HasForeignKey(e => e.OrderId)
                  .HasConstraintName("fk_order_order_item_order_id")
                  .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
