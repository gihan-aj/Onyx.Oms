using Microsoft.EntityFrameworkCore;

namespace Onyx.Oms.Infrastructure.Persistence.Seeding
{
    public class DatabaseSeeder
    {
        private readonly AppDbContext _context;

        public DatabaseSeeder(AppDbContext context)
        {
            _context = context;
        }

        public async Task SeedAsync()
        {
            await _context.Database.MigrateAsync();
        }
    }
}
