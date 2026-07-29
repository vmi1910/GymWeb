using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace GymWeb.ViewModels
{
    // Dùng để hiển thị / chỉnh sửa gộp thông tin Staff + TrainerProfile cho Huấn luyện viên
    public class TrainerViewModel
    {
        public int StaffID { get; set; }

        [Required(ErrorMessage = "Họ và tên không được để trống")]
        public string FullName { get; set; } = string.Empty;

        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; } = "Nam";
        public string PhoneNumber { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;
        public string Status { get; set; } = "Active";

        public string Specialty { get; set; } = string.Empty;
        public string Certification { get; set; } = string.Empty;

        // Ảnh chứng chỉ hiện có (để hiển thị preview) + file mới upload khi chỉnh sửa
        public string? CertificateImagePath { get; set; }
        public IFormFile? CertificateImage { get; set; }
    }
}
