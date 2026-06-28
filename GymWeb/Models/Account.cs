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
    }
}