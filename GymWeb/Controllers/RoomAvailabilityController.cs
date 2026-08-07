using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using GymWeb.Models;
using GymWeb.ViewModels;

namespace GymWeb.Controllers
{
    // Thống kê phòng tập nào đang trống/đang bận trong ngày hôm nay - hỗ trợ lễ tân xếp khách vãng lai (không đặt lịch trước),
    // check-in/check-out thủ công và Admin đặt lịch bảo trì phòng.
    public class RoomAvailabilityController : Controller
    {
        private readonly DataContext _context;
        public RoomAvailabilityController(DataContext context) { _context = context; }

        private bool IsAllowed() => HttpContext.Session.GetString("Role") is "Admin" or "Staff" or "Trainer";
        private bool IsAdmin() => HttpContext.Session.GetString("Role") == "Admin";

        private int? GetCurrentStaffId()
        {
            var username = HttpContext.Session.GetString("Username");
            if (username == null) return null;
            var account = _context.Accounts.FirstOrDefault(a => a.Username == username);
            return account == null ? null : _context.Staffs.Where(s => s.AccountID == account.AccountID).Select(s => (int?)s.StaffID).FirstOrDefault();
        }

        public IActionResult Index()
        {
            if (!IsAllowed()) return RedirectToAction("Login", "Account");

            var now = DateTime.Now;
            var today = DateTime.Today;

            var rooms = _context.Rooms.OrderBy(r => r.Floor).ThenBy(r => r.RoomName).ToList();
            var staffNames = _context.Staffs.ToDictionary(s => s.StaffID, s => s.FullName);
            var memberNames = _context.Members.ToDictionary(m => m.MemberID, m => m.FullName);

            var todayShifts = _context.StaffSchedules.Where(s => s.ShiftDate == today).ToList();
            var todayBookings = _context.RoomBookings
                .Where(b => b.Status == "Approved" && b.StartTime.Date == today)
                .ToList();
            var todayMaintenance = _context.RoomMaintenances
                .Where(m => m.StartTime.Date == today || m.EndTime.Date == today || (m.StartTime < today.AddDays(1) && m.EndTime >= today))
                .ToList();
            var upcomingMaintenance = _context.RoomMaintenances
                .Where(m => m.EndTime >= now)
                .OrderBy(m => m.StartTime)
                .ToList();
            var activeCheckIns = _context.CheckInHistories
                .Where(c => c.CheckOutTime == null)
                .ToList();

            var result = new List<RoomAvailabilityViewModel>();
            foreach (var room in rooms)
            {
                var blocks = new List<RoomBusyBlock>();

                foreach (var s in todayShifts.Where(s => s.RoomID == room.RoomID))
                {
                    blocks.Add(new RoomBusyBlock { Start = s.StartTime, End = s.EndTime, Label = "HLV/NV: " + staffNames.GetValueOrDefault(s.StaffID, "—") });
                }
                foreach (var b in todayBookings.Where(b => b.RoomID == room.RoomID))
                {
                    blocks.Add(new RoomBusyBlock { Start = b.StartTime.TimeOfDay, End = b.EndTime.TimeOfDay, Label = "Đặt phòng: " + memberNames.GetValueOrDefault(b.MemberID, "—") });
                }
                foreach (var m in todayMaintenance.Where(m => m.RoomID == room.RoomID))
                {
                    var blockStart = m.StartTime.Date == today ? m.StartTime.TimeOfDay : TimeSpan.Zero;
                    var blockEnd = m.EndTime.Date == today ? m.EndTime.TimeOfDay : new TimeSpan(23, 59, 0);
                    blocks.Add(new RoomBusyBlock { Start = blockStart, End = blockEnd, Label = "Bảo trì" + (string.IsNullOrWhiteSpace(m.Reason) ? "" : ": " + m.Reason), IsMaintenance = true });
                }

                blocks = blocks.OrderBy(x => x.Start).ToList();
                var current = blocks.FirstOrDefault(x => x.Start <= now.TimeOfDay && now.TimeOfDay < x.End);
                var maintenanceNow = todayMaintenance.Any(m => m.RoomID == room.RoomID && m.StartTime <= now && now < m.EndTime);

                var roomCheckIns = activeCheckIns.Where(c => c.RoomID == room.RoomID).ToList();

                result.Add(new RoomAvailabilityViewModel
                {
                    RoomID = room.RoomID,
                    RoomName = room.RoomName,
                    Floor = room.Floor,
                    Capacity = room.Capacity,
                    RoomStatus = room.Status,
                    IsUnderMaintenance = maintenanceNow,
                    MaintenanceReason = current?.IsMaintenance == true ? current.Label : null,
                    IsFreeNow = !maintenanceNow && current == null && room.Status == "Đang hoạt động",
                    CurrentActivity = (current != null && !current.IsMaintenance) ? current.Label : null,
                    BusyBlocks = blocks,
                    CurrentOccupancy = roomCheckIns.Count,
                    CheckedInPeople = roomCheckIns.Select(c => new CheckedInPerson
                    {
                        CheckInID = c.CheckInID,
                        MemberName = memberNames.GetValueOrDefault(c.MemberID, "—"),
                        CheckInTime = c.CheckInTime
                    }).OrderBy(c => c.CheckInTime).ToList(),
                    UpcomingMaintenance = upcomingMaintenance.Where(m => m.RoomID == room.RoomID).Select(m => new UpcomingMaintenanceItem
                    {
                        RoomMaintenanceID = m.RoomMaintenanceID,
                        StartTime = m.StartTime,
                        EndTime = m.EndTime,
                        Reason = m.Reason
                    }).ToList()
                });
            }

            var members = _context.Members.OrderBy(m => m.FullName)
                .Select(m => new { m.MemberID, Display = m.FullName + " - " + m.PhoneNumber })
                .ToList();
            ViewBag.MemberList = new SelectList(members, "MemberID", "Display");
            ViewBag.Now = now;
            return View(result);
        }

        [HttpPost]
        public IActionResult CheckIn(int roomId, int memberId)
        {
            if (!IsAllowed()) return RedirectToAction("Login", "Account");

            var room = _context.Rooms.Find(roomId);
            var member = _context.Members.Find(memberId);
            if (room == null || member == null)
            {
                TempData["Error"] = "Phòng hoặc hội viên không hợp lệ.";
                return RedirectToAction(nameof(Index));
            }

            var maintenanceNow = _context.RoomMaintenances.Any(m => m.RoomID == roomId && m.StartTime <= DateTime.Now && DateTime.Now < m.EndTime);
            if (maintenanceNow || room.Status != "Đang hoạt động")
            {
                TempData["Error"] = "Phòng đang bảo trì / ngừng hoạt động, không thể check-in.";
                return RedirectToAction(nameof(Index));
            }

            var alreadyIn = _context.CheckInHistories.Any(c => c.MemberID == memberId && c.CheckOutTime == null);
            if (alreadyIn)
            {
                TempData["Error"] = "Hội viên này đang check-in ở một phòng khác, cần check-out trước.";
                return RedirectToAction(nameof(Index));
            }

            var currentOccupancy = _context.CheckInHistories.Count(c => c.RoomID == roomId && c.CheckOutTime == null);
            if (currentOccupancy >= room.Capacity)
            {
                TempData["Error"] = "Phòng đã đạt sức chứa tối đa.";
                return RedirectToAction(nameof(Index));
            }

            _context.CheckInHistories.Add(new CheckInHistory { MemberID = memberId, RoomID = roomId });
            _context.SaveChanges();
            TempData["Success"] = $"Đã check-in {member.FullName} vào {room.RoomName}.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult CheckOut(int id)
        {
            if (!IsAllowed()) return RedirectToAction("Login", "Account");

            var checkIn = _context.CheckInHistories.Find(id);
            if (checkIn == null || checkIn.CheckOutTime != null) return RedirectToAction(nameof(Index));

            checkIn.CheckOutTime = DateTime.Now;
            _context.SaveChanges();
            TempData["Success"] = "Đã check-out.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult ScheduleMaintenance(int roomId, DateTime startTime, DateTime endTime, string? reason)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var room = _context.Rooms.Find(roomId);
            if (room == null)
            {
                TempData["Error"] = "Phòng không hợp lệ.";
                return RedirectToAction(nameof(Index));
            }
            if (endTime <= startTime)
            {
                TempData["Error"] = "Thời gian kết thúc phải sau thời gian bắt đầu.";
                return RedirectToAction(nameof(Index));
            }
            if (endTime < DateTime.Now)
            {
                TempData["Error"] = "Không thể đặt lịch bảo trì trong quá khứ.";
                return RedirectToAction(nameof(Index));
            }

            _context.RoomMaintenances.Add(new RoomMaintenance
            {
                RoomID = roomId,
                StartTime = startTime,
                EndTime = endTime,
                Reason = reason?.Trim(),
                CreatedByStaffID = GetCurrentStaffId()
            });
            _context.SaveChanges();
            TempData["Success"] = $"Đã đặt lịch bảo trì {room.RoomName}.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult CancelMaintenance(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var maintenance = _context.RoomMaintenances.Find(id);
            if (maintenance != null)
            {
                _context.RoomMaintenances.Remove(maintenance);
                _context.SaveChanges();
                TempData["Success"] = "Đã hủy lịch bảo trì.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
