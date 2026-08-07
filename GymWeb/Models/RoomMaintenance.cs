using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GymWeb.Models
{
    // Lịch bảo trì phòng tập do Admin đặt trước - phòng sẽ hiện "Đang bảo trì" trong khung giờ này,
    // khác với Room.Status (trạng thái vận hành lâu dài, chỉnh trực tiếp qua Quản lý phòng tập).
    public class RoomMaintenance
    {
        [Key]
        public int RoomMaintenanceID { get; set; }

        [Required]
        public int RoomID { get; set; }
        [ForeignKey("RoomID")]
        public virtual Room? Room { get; set; }

        [Required]
        public DateTime StartTime { get; set; }

        [Required]
        public DateTime EndTime { get; set; }

        public string? Reason { get; set; }

        // Nullable vì tài khoản Admin gốc (tạo từ SeedData) không có bản ghi Staff liên kết
        public int? CreatedByStaffID { get; set; }
        [ForeignKey("CreatedByStaffID")]
        public virtual Staff? CreatedByStaff { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
