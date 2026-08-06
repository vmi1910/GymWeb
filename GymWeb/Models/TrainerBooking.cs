using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GymWeb.Models
{
    // Yêu cầu hội viên đăng ký tập cùng một huấn luyện viên cụ thể, trong khung giờ PT đó đã đăng ký ca làm
    public class TrainerBooking
    {
        [Key]
        public int TrainerBookingID { get; set; }

        [Required]
        public int MemberID { get; set; }
        [ForeignKey("MemberID")]
        public virtual Member? Member { get; set; }

        // Huấn luyện viên được chọn (Staff có Role = "Trainer")
        [Required]
        public int StaffID { get; set; }
        [ForeignKey("StaffID")]
        public virtual Staff? Staff { get; set; }

        [Required]
        public DateTime StartTime { get; set; }

        [Required]
        public DateTime EndTime { get; set; }

        // Pending / Approved / Rejected / Canceled
        public string Status { get; set; } = "Pending";

        public string? Note { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Huấn luyện viên hoặc nhân viên/admin đã duyệt/từ chối yêu cầu này
        public int? ProcessedByStaffID { get; set; }
        [ForeignKey("ProcessedByStaffID")]
        public virtual Staff? ProcessedByStaff { get; set; }
        public DateTime? ProcessedAt { get; set; }
    }
}
