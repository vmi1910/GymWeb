using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace GymWeb.ViewModels
{
    public class AccountCreateViewModel
    {
        // --- Phần thông tin Account ---
        [Required(ErrorMessage = "Tên đăng nhập không được để trống")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        public string RawPassword { get; set; } = string.Empty;

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
    }
}
