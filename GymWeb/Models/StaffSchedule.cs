using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GymWeb.Models
{
    // Lịch làm việc / lịch tập của nhân viên - huấn luyện viên tại một phòng tập
    public class StaffSchedule
    {
        [Key]
        public int ScheduleID { get; set; }

        [Required]
        public int StaffID { get; set; }
        [ForeignKey("StaffID")]
        public virtual Staff? Staff { get; set; }

        [Required]
        public int RoomID { get; set; }
        [ForeignKey("RoomID")]
        public virtual Room? Room { get; set; }

        [DataType(DataType.Date)]
        public DateTime ShiftDate { get; set; } = DateTime.Today;

        public TimeSpan StartTime { get; set; }

        public TimeSpan EndTime { get; set; }
    }
}
