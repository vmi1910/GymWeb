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
                    new MembershipPackage
                    {
                        PackageName = "Gói 1 tháng",
                        DurationDays = 30,
                        Price = 500000,
                        Description = "Tập luyện tự do tại tất cả chi nhánh trong 1 tháng.",
                        Highlights = "Tập luyện tự do tất cả chi nhánh;Sử dụng phòng Gym, Cardio, Yoga;Tủ đồ & nước uống miễn phí;Tư vấn dinh dưỡng cơ bản",
                        IsActive = true
                    },
                    new MembershipPackage
                    {
                        PackageName = "Gói 6 tháng",
                        DurationDays = 180,
                        Price = 2700000,
                        Description = "Tiết kiệm hơn cho hội viên tập luyện dài hạn, tặng kèm 2 buổi PT.",
                        Highlights = "Toàn bộ quyền lợi gói 1 tháng;Tặng 2 buổi tập cùng PT;Ưu tiên đặt lịch lớp nhóm;Đo chỉ số cơ thể InBody định kỳ",
                        BadgeText = "Phổ biến nhất",
                        IsActive = true
                    },
                    new MembershipPackage
                    {
                        PackageName = "Gói 12 tháng",
                        DurationDays = 365,
                        Price = 4800000,
                        Description = "Ưu đãi tốt nhất, tặng kèm 4 buổi PT và khóa Yoga.",
                        Highlights = "Toàn bộ quyền lợi gói 6 tháng;Tặng 4 buổi PT + khóa Yoga;Miễn phí đóng băng gói khi vắng;Ưu đãi 10% dịch vụ phục hồi",
                        BadgeText = "Tiết kiệm nhất",
                        IsFeatured = true,
                        IsActive = true
                    }
                );
                context.SaveChanges();
            }
            else
            {
                // Vá lại nội dung cho 3 gói tập mẫu đã tạo trước khi có Highlights/BadgeText,
                // để trang "Đăng ký gói tập" không bị hiển thị trống trơn / mất thẩm mỹ.
                var packageDefaults = new Dictionary<string, (string Highlights, string? BadgeText, bool IsFeatured)>
                {
                    ["Gói 1 tháng"] = ("Tập luyện tự do tất cả chi nhánh;Sử dụng phòng Gym, Cardio, Yoga;Tủ đồ & nước uống miễn phí;Tư vấn dinh dưỡng cơ bản", null, false),
                    ["Gói 6 tháng"] = ("Toàn bộ quyền lợi gói 1 tháng;Tặng 2 buổi tập cùng PT;Ưu tiên đặt lịch lớp nhóm;Đo chỉ số cơ thể InBody định kỳ", "Phổ biến nhất", false),
                    ["Gói 12 tháng"] = ("Toàn bộ quyền lợi gói 6 tháng;Tặng 4 buổi PT + khóa Yoga;Miễn phí đóng băng gói khi vắng;Ưu đãi 10% dịch vụ phục hồi", "Tiết kiệm nhất", true),
                };

                var changedPackages = false;
                foreach (var pkg in context.MembershipPackages.Where(p => string.IsNullOrWhiteSpace(p.Highlights)))
                {
                    if (packageDefaults.TryGetValue(pkg.PackageName, out var d))
                    {
                        pkg.Highlights = d.Highlights;
                        pkg.BadgeText ??= d.BadgeText;
                        if (d.IsFeatured) pkg.IsFeatured = true;
                        changedPackages = true;
                    }
                }
                if (changedPackages) context.SaveChanges();
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

            // Đồng bộ Email cho các tài khoản được tạo TRƯỚC khi có cột Account.Email
            // (cột này mới thêm để phục vụ gửi OTP quên mật khẩu) - lấy lại từ Member/Staff tương ứng.
            // Chạy mỗi lần khởi động để luôn tự "vá" các tài khoản còn thiếu Email.
            var accountsMissingEmail = context.Accounts.Where(a => string.IsNullOrEmpty(a.Email)).ToList();
            if (accountsMissingEmail.Any())
            {
                var memberEmailByAccount = context.Members.ToDictionary(m => m.AccountID, m => m.Email);
                var staffEmailByAccount = context.Staffs.ToDictionary(s => s.AccountID, s => s.Email);
                var changed = false;

                foreach (var acc in accountsMissingEmail)
                {
                    string? email = memberEmailByAccount.TryGetValue(acc.AccountID, out var memberEmail) ? memberEmail
                        : staffEmailByAccount.TryGetValue(acc.AccountID, out var staffEmail) ? staffEmail
                        : null;

                    if (!string.IsNullOrWhiteSpace(email))
                    {
                        acc.Email = email;
                        changed = true;
                    }
                }

                if (changed) context.SaveChanges();
            }
        }
    }
}