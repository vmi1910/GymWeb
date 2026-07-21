using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GymWeb.Models
{
    // Thông tin mở rộng cho nhân viên có Role = "Trainer" (Huấn luyện viên)
    public class TrainerProfile
    {
        [Key]
        [ForeignKey("Staff")]
        public int StaffID { get; set; }

        public string Specialty { get; set; } = string.Empty; // Chuyên môn

        public string Certification { get; set; } = string.Empty; // Chứng chỉ nghề

        // Đường dẫn (tương đối tới wwwroot) tới ảnh chứng chỉ đã upload, VD: /uploads/certificates/xxx.jpg
        public string? CertificateImagePath { get; set; }

        public virtual Staff? Staff { get; set; }
    }
}
