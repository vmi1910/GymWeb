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
    }
}
