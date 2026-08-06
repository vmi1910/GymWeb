using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GymWeb.Models
{
    // Lịch sử check-in/check-out của hội viên vào phòng tập (kể cả khách vãng lai không đặt lịch trước)
    public class CheckInHistory
    {
        [Key]
        public int CheckInID { get; set; }

        [Required]
        public int MemberID { get; set; }
        [ForeignKey("MemberID")]
        public virtual Member? Member { get; set; }

        [Required]
        public int RoomID { get; set; }
        [ForeignKey("RoomID")]
        public virtual Room? Room { get; set; }

        public DateTime CheckInTime { get; set; } = DateTime.Now;

        // null nghĩa là vẫn đang có mặt trong phòng
        public DateTime? CheckOutTime { get; set; }
    }
}
