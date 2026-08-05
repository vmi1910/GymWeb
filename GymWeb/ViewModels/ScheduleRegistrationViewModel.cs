using System.ComponentModel.DataAnnotations;

namespace GymWeb.ViewModels
{
    public class ScheduleRegistrationViewModel
    {
        [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn phòng tập.")]
        public int RoomID { get; set; }

        [Display(Name = "Ngày làm việc")]
        public DateTime ShiftDate { get; set; } = DateTime.Today;

        [Display(Name = "Giờ bắt đầu")]
        public TimeSpan StartTime { get; set; } = new TimeSpan(8, 0, 0);

        [Display(Name = "Giờ kết thúc")]
        public TimeSpan EndTime { get; set; } = new TimeSpan(17, 0, 0);
    }
}
