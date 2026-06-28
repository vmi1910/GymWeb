using Microsoft.EntityFrameworkCore;

namespace GymWeb.Models
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }

        public DbSet<Account> Accounts => Set<Account>();
    }
}