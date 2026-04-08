using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace SharedDatabaseHelper
{
    public static class DatabaseMigrationHelper
    {
        /// <summary>
        /// Ensures that the database is migrated to the latest version.
        /// </summary>
        public static async Task EnsureMigratedAsync<TContext>(
            IServiceProvider sp,
            ILogger logger) where TContext : DbContext
        {
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<TContext>();

            var pending = await db.Database.GetPendingMigrationsAsync();
            if (pending.Any())
            {
                logger.LogInformation($"Found migrations: {string.Join(", ", pending)}");
                await db.Database.MigrateAsync();
            }
            else
            {
                logger.LogInformation("Database is up to date.");
            }
        }
    }
}
