using Microsoft.EntityFrameworkCore;

namespace RealJSJDatabase.Models
{
    public class AppDbContext: DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }






        protected override void OnModelCreating(ModelBuilder builder) { }
    }
}
