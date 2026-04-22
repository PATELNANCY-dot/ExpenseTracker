using Microsoft.EntityFrameworkCore;
using ExpenseTracker.Models;

namespace ExpenseTracker.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<UserModel> Users { get; set; }

        public DbSet<ExpenseModel> Expenses { get; set; }

        public DbSet<IncomeModel> Incomes { get; set; }

        public DbSet<ThemeModel> Themes { get; set; }
    }
}