using System.ComponentModel.DataAnnotations;

namespace GymWeb.Models
{
    public class Account
    {
        [Key]
        public int AccountID { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
        public string Username { get; set; } = string.Empty;

        // Lưu mật khẩu đã được hash, không lưu pass thô
        public string PasswordHash { get; set; } = string.Empty;

        public string Role { get; set; } = "Member"; // Mặc định là Member, admin sẽ là "Admin"

        // Email dùng để nhận mã OTP khôi phục mật khẩu (đồng bộ từ Member/Staff khi tạo/sửa)
        public string? Email { get; set; }

        // Mã OTP khôi phục mật khẩu đang chờ xác nhận (null nếu không có yêu cầu nào đang chờ)
        public string? ResetOtpCode { get; set; }
        public DateTime? ResetOtpExpiryUtc { get; set; }
    }
}