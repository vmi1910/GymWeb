using Microsoft.EntityFrameworkCore;

namespace GymWeb.Models
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }

        public DbSet<Account> Accounts => Set<Account>();
        public DbSet<Member> Members => Set<Member>();
        public DbSet<Staff> Staffs => Set<Staff>();
    }
}