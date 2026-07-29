using System.ComponentModel.DataAnnotations;

namespace GymWeb.Models
{
    public class Room
    {
        [Key]
        public int RoomID { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên phòng tập")]
        public string RoomName { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "Sức chứa phải lớn hơn 0")]
        public int Capacity { get; set; }

        public int Floor { get; set; }

        public string Status { get; set; } = "Đang hoạt động";
    }
}
