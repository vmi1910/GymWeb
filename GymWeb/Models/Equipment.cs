using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GymWeb.Models
{
    // Trang thiết bị đặt tại các phòng tập - quản lý theo LOẠI thiết bị + số lượng,
    // không quản lý từng máy đơn lẻ (đủ dùng cho quy mô phòng gym vừa và nhỏ).
    public class Equipment
    {
        [Key]
        public int EquipmentID { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên thiết bị")]
        public string EquipmentName { get; set; } = string.Empty;

        // VD: Máy chạy bộ, Tạ đơn, Máy tập cơ...
        public string Category { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        public DateTime PurchaseDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Vui lòng chọn phòng đặt thiết bị")]
        public int RoomID { get; set; }
        [ForeignKey("RoomID")]
        public virtual Room? Room { get; set; }

        // Tổng số máy thuộc loại này
        [Range(1, int.MaxValue, ErrorMessage = "Tổng số lượng phải lớn hơn 0")]
        public int TotalQuantity { get; set; } = 1;

        // Trong tổng số trên, bao nhiêu máy đang bảo trì / đã hỏng ngừng dùng
        [Range(0, int.MaxValue, ErrorMessage = "Số lượng không hợp lệ")]
        public int MaintenanceQuantity { get; set; } = 0;

        [Range(0, int.MaxValue, ErrorMessage = "Số lượng không hợp lệ")]
        public int BrokenQuantity { get; set; } = 0;

        // Số máy đang hoạt động bình thường = Tổng - Bảo trì - Hỏng (không lưu DB, tính lại mỗi lần đọc)
        [NotMapped]
        public int ActiveQuantity => Math.Max(0, TotalQuantity - MaintenanceQuantity - BrokenQuantity);
    }
}
