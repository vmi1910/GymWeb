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
        }
    }
}