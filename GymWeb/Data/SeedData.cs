using Microsoft.AspNetCore.Identity;
using GymWeb.Models;

namespace GymManagement.Data
{
    public static class SeedData
    {
        public static void EnsurePopulated(DataContext context)
        {
            // Kiểm tra nếu chưa có tài khoản nào thì mới thêm
            if (!context.Accounts.Any())
            {
                var hasher = new PasswordHasher<Account>();
                var admin = new Account
                {
                    Username = "admin",
                    Role = "Admin"
                };
                
                // Mã hóa mật khẩu "Admin123@"
                admin.PasswordHash = hasher.HashPassword(admin, "Admin123@");

                context.Accounts.Add(admin);
                context.SaveChanges();
            }

            // Dữ liệu mẫu cho Gói tập
            if (!context.MembershipPackages.Any())
            {
                context.MembershipPackages.AddRange(
                    new MembershipPackage { PackageName = "Gói 1 tháng", DurationDays = 30, Price = 500000, Description = "Tập luyện tự do tại tất cả chi nhánh trong 1 tháng.", IsActive = true },
                    new MembershipPackage { PackageName = "Gói 6 tháng", DurationDays = 180, Price = 2700000, Description = "Tiết kiệm hơn cho hội viên tập luyện dài hạn, tặng kèm 2 buổi PT.", IsActive = true },
                    new MembershipPackage { PackageName = "Gói 12 tháng", DurationDays = 365, Price = 4800000, Description = "Ưu đãi tốt nhất, tặng kèm 4 buổi PT và khóa Yoga.", IsActive = true }
                );
                context.SaveChanges();
            }

            // Dữ liệu mẫu cho Phòng tập
            if (!context.Rooms.Any())
            {
                context.Rooms.AddRange(
                    new Room { RoomName = "Phòng Gym A", Capacity = 60, Floor = 1, Status = "Đang hoạt động" },
                    new Room { RoomName = "Phòng Yoga", Capacity = 25, Floor = 2, Status = "Đang hoạt động" },
                    new Room { RoomName = "Phòng Cardio", Capacity = 40, Floor = 1, Status = "Đang hoạt động" }
                );
                context.SaveChanges();
            }
        }
    }
}