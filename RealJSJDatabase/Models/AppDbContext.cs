using Microsoft.EntityFrameworkCore;

namespace RealJSJDatabase.Models
{
    public class AppDbContext: DbContext
    {

        public DbSet<Employee> Employees { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<Expense> Expenses { get; set; }
        public DbSet<ExpenseLine> ExpenseLines { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }






        protected override void OnModelCreating(ModelBuilder builder) { }
    }
}
