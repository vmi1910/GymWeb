using GymWeb.Models;

namespace GymWeb.Services
{
    public class ScheduleConflictService
    {
        private readonly DataContext _context;

        public ScheduleConflictService(DataContext context)
        {
            _context = context;
        }

        public bool HasConflict(int staffId, int roomId, DateTime shiftDate, TimeSpan startTime, TimeSpan endTime, int? excludedScheduleId = null)
        {
            var schedules = _context.StaffSchedules.Where(s =>
                s.ShiftDate == shiftDate.Date &&
                (!excludedScheduleId.HasValue || s.ScheduleID != excludedScheduleId.Value) &&
                s.StartTime < endTime && startTime < s.EndTime);

            return schedules.Any(s => s.StaffID == staffId || s.RoomID == roomId);
        }
    }
}
