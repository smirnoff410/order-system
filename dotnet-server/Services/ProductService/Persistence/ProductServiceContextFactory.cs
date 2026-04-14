using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using SharedDatabaseHelper;
using SharedDatabaseHelper.Settings;

namespace ProductService.Persistence
{
    public class ProductServiceContextFactory : IDesignTimeDbContextFactory<ProductServiceContext>
    {
        public ProductServiceContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ProductServiceContext>();

            var templateConnectionString = TemplateConnectionString.Get();

            var dbConfiguration = new PostgreConfiguration(templateConnectionString);

            optionsBuilder.UseNpgsql(dbConfiguration.GetConnectionString(),
                opts => opts.CommandTimeout((int)TimeSpan.FromMinutes(10).TotalSeconds));

            return new ProductServiceContext(optionsBuilder.Options);
        }
    }
}
