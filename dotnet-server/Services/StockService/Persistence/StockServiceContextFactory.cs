using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using SharedDatabaseHelper;
using SharedDatabaseHelper.Settings;

namespace StockService.Persistence
{
    public class StockServiceContextFactory : IDesignTimeDbContextFactory<StockServiceContext>
    {
        public StockServiceContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<StockServiceContext>();

            var templateConnectionString = TemplateConnectionString.Get();

            var dbConfiguration = new PostgreConfiguration(templateConnectionString);

            optionsBuilder.UseNpgsql(dbConfiguration.GetConnectionString(),
                opts => opts.CommandTimeout((int)TimeSpan.FromMinutes(10).TotalSeconds));

            return new StockServiceContext(optionsBuilder.Options);
        }
    }
}
