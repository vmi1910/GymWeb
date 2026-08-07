using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GymWeb.Models
{
    // Trang thiết bị đặt tại các phòng tập
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

        // Đang sử dụng / Bảo trì / Hỏng - ngừng sử dụng
        public string Status { get; set; } = "Đang sử dụng";

        [Required(ErrorMessage = "Vui lòng chọn phòng đặt thiết bị")]
        public int RoomID { get; set; }
        [ForeignKey("RoomID")]
        public virtual Room? Room { get; set; }
    }
}
