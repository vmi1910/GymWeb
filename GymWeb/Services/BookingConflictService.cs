using GymWeb.Models;

namespace GymWeb.Services
{
    public class BookingConflictService
    {
        private readonly DataContext _context;

        public BookingConflictService(DataContext context)
        {
            _context = context;
        }

        public bool HasApprovedRoomConflict(int roomId, DateTime startTime, DateTime endTime, int? excludedBookingId = null)
        {
            return _context.RoomBookings.Any(b =>
                b.RoomID == roomId &&
                b.Status == "Approved" &&
                (!excludedBookingId.HasValue || b.RoomBookingID != excludedBookingId.Value) &&
                b.StartTime < endTime && startTime < b.EndTime);
        }

        // Khung giờ hội viên muốn tập cùng PT phải nằm trọn trong 1 ca làm việc mà PT đó đã tự đăng ký
        // (qua MyScheduleController) - đảm bảo PT thực sự có mặt tại phòng tập lúc đó.
        public bool IsWithinTrainerShift(int staffId, DateTime startTime, DateTime endTime)
        {
            if (startTime.Date != endTime.Date) return false;

            return _context.StaffSchedules.Any(s =>
                s.StaffID == staffId &&
                s.ShiftDate == startTime.Date &&
                s.StartTime <= startTime.TimeOfDay &&
                s.EndTime >= endTime.TimeOfDay);
        }

        // Một PT chỉ kèm được 1 hội viên tại 1 thời điểm - chặn 2 yêu cầu đã duyệt bị chồng giờ nhau
        public bool HasApprovedTrainerConflict(int staffId, DateTime startTime, DateTime endTime, int? excludedBookingId = null)
        {
            return _context.TrainerBookings.Any(b =>
                b.StaffID == staffId &&
                b.Status == "Approved" &&
                (!excludedBookingId.HasValue || b.TrainerBookingID != excludedBookingId.Value) &&
                b.StartTime < endTime && startTime < b.EndTime);
        }
    }
}
