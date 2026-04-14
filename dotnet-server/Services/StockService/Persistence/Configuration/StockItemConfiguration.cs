using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockService.Models;

namespace StockService.Persistence.Configuration
{
    public class StockItemConfiguration : IEntityTypeConfiguration<StockItem>
    {
        public void Configure(EntityTypeBuilder<StockItem> builder)
        {
            builder.ToTable("stock_item");
            builder.Property(x => x.Id)
                .ValueGeneratedNever();

            builder.Property(x => x.ProductId);
            builder.Property(x => x.QuantityAvailable);
        }
    }
}
