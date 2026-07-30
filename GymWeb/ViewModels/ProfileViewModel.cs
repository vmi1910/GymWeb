using System.ComponentModel.DataAnnotations;

namespace GymWeb.ViewModels
{
    // Dùng cho trang "Hồ sơ của tôi" - Hội viên tự xem và chỉnh sửa thông tin cá nhân
    public class ProfileViewModel
    {
        // Chỉ hiển thị, không cho sửa qua form này
        public string Username { get; set; } = string.Empty;
        public DateTime RegisterDate { get; set; }
        public string Status { get; set; } = string.Empty;

        [Required(ErrorMessage = "Họ và tên không được để trống")]
        public string FullName { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }

        public string Gender { get; set; } = "Nam";

        public string PhoneNumber { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;

        // Đổi mật khẩu (tùy chọn) - để trống nếu không muốn đổi
        public string? NewPassword { get; set; }

        [Compare("NewPassword", ErrorMessage = "Mật khẩu nhập lại không khớp")]
        public string? ConfirmNewPassword { get; set; }
    }
}