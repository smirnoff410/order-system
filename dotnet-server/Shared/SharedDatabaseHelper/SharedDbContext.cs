using Microsoft.EntityFrameworkCore;

namespace SharedDatabaseHelper
{
    public class SharedDbContext : DbContext
    {
        public SharedDbContext(DbContextOptions<SharedDbContext> options) : base(options)
        {
            
        }
    }
}
