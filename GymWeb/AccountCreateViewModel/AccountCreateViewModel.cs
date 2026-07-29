using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace GymWeb.ViewModels
{
    public class AccountCreateViewModel
    {
        // --- Phần thông tin Account ---
        [Required(ErrorMessage = "Tên đăng nhập không được để trống")]
        public string Username { get; set; } = string.Empty;

        // Không đặt [Required] ở đây vì màn Sửa (Edit) cho phép để trống = không đổi mật khẩu.
        // Màn Tạo mới (Create) và Đăng ký (Register) tự kiểm tra bắt buộc trong controller.
        public string RawPassword { get; set; } = string.Empty;

        // Chỉ dùng cho form Đăng ký công khai (Member tự đăng ký), Admin Create/Edit không cần
        [Compare("RawPassword", ErrorMessage = "Mật khẩu nhập lại không khớp")]
        public string ConfirmPassword { get; set; } = string.Empty;

        public string Role { get; set; } = "Member";

        // --- Phần thông tin Cá nhân (Dùng chung cho Member/Staff/Trainer) ---
        [Required(ErrorMessage = "Họ và tên không được để trống")]
        public string FullName { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = string.Empty;

        public string Gender { get; set; } = "Nam";

        // --- Chỉ áp dụng khi Role = "Trainer" ---
        public string? Specialty { get; set; }
        public string? Certification { get; set; }
        public IFormFile? CertificateImage { get; set; }

        // Dùng cho màn Sửa: hiển thị ảnh chứng chỉ hiện có, không bắt buộc chọn ảnh mới
        public string? ExistingCertificateImagePath { get; set; }
    }
}
