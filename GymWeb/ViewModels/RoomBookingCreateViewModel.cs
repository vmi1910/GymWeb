using System.ComponentModel.DataAnnotations;

namespace GymWeb.ViewModels
{
    public class RoomBookingCreateViewModel
    {
        [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn phòng tập.")]
        public int RoomID { get; set; }

        [Display(Name = "Thời gian bắt đầu")]
        public DateTime StartTime { get; set; }

        [Display(Name = "Thời gian kết thúc")]
        public DateTime EndTime { get; set; }

        [StringLength(500, ErrorMessage = "Ghi chú không được vượt quá 500 ký tự.")]
        public string? Note { get; set; }
    }
}
