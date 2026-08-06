using System.ComponentModel.DataAnnotations;

namespace GymWeb.ViewModels
{
    public class TrainerBookingCreateViewModel
    {
        [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn huấn luyện viên.")]
        public int StaffID { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn một ca làm việc cụ thể của huấn luyện viên.")]
        public int ScheduleID { get; set; }

        [StringLength(500, ErrorMessage = "Ghi chú không được vượt quá 500 ký tự.")]
        public string? Note { get; set; }
    }
}
