using Microsoft.EntityFrameworkCore;

namespace GymWeb.Models
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }

        public DbSet<Account> Accounts => Set<Account>();
        public DbSet<Member> Members => Set<Member>();
        public DbSet<Staff> Staffs => Set<Staff>();
        public DbSet<MembershipPackage> MembershipPackages => Set<MembershipPackage>();
        public DbSet<Room> Rooms => Set<Room>();
        public DbSet<TrainerProfile> TrainerProfiles => Set<TrainerProfile>();
        public DbSet<StaffSchedule> StaffSchedules => Set<StaffSchedule>();
        public DbSet<Subscription> Subscriptions => Set<Subscription>();
        public DbSet<Payment> Payments => Set<Payment>();
        public DbSet<RoomBooking> RoomBookings => Set<RoomBooking>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // SQL Server báo lỗi "multiple cascade paths" vì Payment có thể bị xóa dây chuyền
            // theo 2 nhánh khác nhau: Account -> Member -> Subscription -> Payment (CASCADE)
            // và Account -> Staff -> Payment. SQL Server tính cả SET NULL/SET DEFAULT là một
            // "cascading action", nên chỉ có DeleteBehavior.Restrict (=> NO ACTION) mới thực sự
            // phá vỡ đường đi thứ 2 và cho phép tạo bảng.
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Staff)
                .WithMany()
                .HasForeignKey(p => p.StaffID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RoomBooking>()
                .HasOne(b => b.Room)
                .WithMany()
                .HasForeignKey(b => b.RoomID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RoomBooking>()
                .HasOne(b => b.ProcessedByStaff)
                .WithMany()
                .HasForeignKey(b => b.ProcessedByStaffID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RoomBooking>()
                .HasIndex(b => new { b.RoomID, b.StartTime, b.EndTime });

            modelBuilder.Entity<RoomBooking>()
                .HasIndex(b => new { b.MemberID, b.Status });

            modelBuilder.Entity<RoomBooking>()
                .Property(b => b.Status)
                .HasMaxLength(20);
        }
    }
}
