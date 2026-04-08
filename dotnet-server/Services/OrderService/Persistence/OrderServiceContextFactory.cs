using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using SharedDatabaseHelper;
using SharedDatabaseHelper.Settings;

namespace OrderService.Persistence
{
    public class OrderServiceContextFactory : IDesignTimeDbContextFactory<OrderServiceContext>
    {
        public OrderServiceContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<OrderServiceContext>();

            var templateConnectionString = TemplateConnectionString.Get();

            var dbConfiguration = new PostgreConfiguration(templateConnectionString);

            optionsBuilder.UseNpgsql(dbConfiguration.GetConnectionString(),
                opts => opts.CommandTimeout((int)TimeSpan.FromMinutes(10).TotalSeconds));

            return new OrderServiceContext(optionsBuilder.Options);
        }
    }
}
