using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SharedDatabaseHelper.Settings;

namespace SharedDatabaseHelper
{
    public static class DependencyInjectionHelper
    {
        public static IServiceCollection AddDatabase<TContext>(this IServiceCollection services, IConfiguration configuration) where TContext : DbContext
        {
            var templateConnectionString = configuration.GetConnectionString("MasterConnection");
            if (templateConnectionString == null || string.IsNullOrWhiteSpace(templateConnectionString))
                throw new Exception("DB master connection string is empty");

            var dbConfiguration = new PostgreConfiguration(templateConnectionString);

            services.AddDbContext<TContext>(options =>
                options.UseNpgsql(dbConfiguration.GetConnectionString())
                        .EnableSensitiveDataLogging());

            return services;
        }
    }
}
