using GymWeb.Models;
using GymWeb.Services;
using GymWeb.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GymWeb.Controllers
{
    public class RoomBookingController : Controller
    {
        private readonly DataContext _context;
        private readonly BookingConflictService _bookingConflictService;

        public RoomBookingController(DataContext context, BookingConflictService bookingConflictService)
        {
            _context = context;
            _bookingConflictService = bookingConflictService;
        }

        private bool IsAdminOrStaff()
        {
            var role = HttpContext.Session.GetString("Role");
            return role == "Admin" || role == "Staff";
        }

        private Member? GetCurrentMember()
        {
            var username = HttpContext.Session.GetString("Username");
            if (username == null || HttpContext.Session.GetString("Role") != "Member") return null;

            var account = _context.Accounts.FirstOrDefault(a => a.Username == username);
            return account == null ? null : _context.Members.FirstOrDefault(m => m.AccountID == account.AccountID);
        }

        private int? GetCurrentStaffId()
        {
            var username = HttpContext.Session.GetString("Username");
            if (username == null) return null;

            var account = _context.Accounts.FirstOrDefault(a => a.Username == username);
            return account == null ? null : _context.Staffs.Where(s => s.AccountID == account.AccountID).Select(s => (int?)s.StaffID).FirstOrDefault();
        }

        private void LoadRooms(int? roomId = null)
        {
            var rooms = _context.Rooms.OrderBy(r => r.RoomName)
                .Select(r => new { r.RoomID, Display = r.RoomName + " - tầng " + r.Floor + " (" + r.Capacity + " người)" })
                .ToList();
            ViewBag.RoomList = new SelectList(rooms, "RoomID", "Display", roomId);
        }

        [HttpGet]
        public IActionResult Create()
        {
            if (GetCurrentMember() == null) return RedirectToAction("Login", "Account");

            LoadRooms();
            var start = DateTime.Today.AddDays(1).AddHours(8);
            return View(new RoomBookingCreateViewModel { StartTime = start, EndTime = start.AddHours(1) });
        }

        [HttpPost]
        public IActionResult Create(RoomBookingCreateViewModel model)
        {
            var member = GetCurrentMember();
            if (member == null) return RedirectToAction("Login", "Account");

            if (model.EndTime <= model.StartTime)
            {
                ModelState.AddModelError(string.Empty, "Thời gian kết thúc phải sau thời gian bắt đầu.");
            }
            if (model.StartTime < DateTime.Now)
            {
                ModelState.AddModelError("StartTime", "Chỉ có thể đặt phòng ở thời điểm hiện tại hoặc trong tương lai.");
            }
            if (!_context.Rooms.Any(r => r.RoomID == model.RoomID))
            {
                ModelState.AddModelError("RoomID", "Phòng tập không hợp lệ.");
            }
            if (ModelState.IsValid && _bookingConflictService.HasApprovedRoomConflict(model.RoomID, model.StartTime, model.EndTime))
            {
                ModelState.AddModelError(string.Empty, "Phòng đã có lịch được duyệt trùng với khung giờ này.");
            }

            if (!ModelState.IsValid)
            {
                LoadRooms(model.RoomID);
                return View(model);
            }

            _context.RoomBookings.Add(new RoomBooking
            {
                MemberID = member.MemberID,
                RoomID = model.RoomID,
                StartTime = model.StartTime,
                EndTime = model.EndTime,
                Note = model.Note?.Trim()
            });
            _context.SaveChanges();
            TempData["Success"] = "Yêu cầu đặt phòng đã được gửi và đang chờ duyệt.";
            return RedirectToAction(nameof(MyBookings));
        }

        public IActionResult MyBookings()
        {
            var member = GetCurrentMember();
            if (member == null) return RedirectToAction("Login", "Account");

            var bookings = _context.RoomBookings.Where(b => b.MemberID == member.MemberID)
                .OrderByDescending(b => b.StartTime).ToList();
            ViewBag.RoomNames = _context.Rooms.ToDictionary(r => r.RoomID, r => r.RoomName);
            return View(bookings);
        }

        [HttpPost]
        public IActionResult Cancel(int id)
        {
            var member = GetCurrentMember();
            if (member == null) return RedirectToAction("Login", "Account");

            var booking = _context.RoomBookings.FirstOrDefault(b => b.RoomBookingID == id && b.MemberID == member.MemberID);
            if (booking == null) return NotFound();

            if (booking.Status != "Pending" && booking.Status != "Approved")
            {
                TempData["Error"] = "Yêu cầu này không thể hủy.";
                return RedirectToAction(nameof(MyBookings));
            }
            if (booking.StartTime <= DateTime.Now)
            {
                TempData["Error"] = "Không thể hủy lịch đã bắt đầu.";
                return RedirectToAction(nameof(MyBookings));
            }

            booking.Status = "Canceled";
            booking.ProcessedAt = DateTime.Now;
            _context.SaveChanges();
            TempData["Success"] = "Đã hủy yêu cầu đặt phòng.";
            return RedirectToAction(nameof(MyBookings));
        }

        public IActionResult Index()
        {
            if (!IsAdminOrStaff()) return RedirectToAction("Login", "Account");

            var bookings = _context.RoomBookings.OrderBy(b => b.Status == "Pending" ? 0 : 1)
                .ThenBy(b => b.StartTime).ToList();
            ViewBag.MemberNames = _context.Members.ToDictionary(m => m.MemberID, m => m.FullName);
            ViewBag.RoomNames = _context.Rooms.ToDictionary(r => r.RoomID, r => r.RoomName);
            ViewBag.StaffNames = _context.Staffs.ToDictionary(s => s.StaffID, s => s.FullName);
            return View(bookings);
        }

        [HttpPost]
        public IActionResult Process(int id, string decision)
        {
            if (!IsAdminOrStaff()) return RedirectToAction("Login", "Account");

            var booking = _context.RoomBookings.Find(id);
            if (booking == null) return NotFound();
            if (booking.Status != "Pending")
            {
                TempData["Error"] = "Yêu cầu này đã được xử lý.";
                return RedirectToAction(nameof(Index));
            }

            if (decision == "Approved")
            {
                if (booking.StartTime <= DateTime.Now)
                {
                    TempData["Error"] = "Không thể duyệt lịch đã bắt đầu.";
                    return RedirectToAction(nameof(Index));
                }
                if (_bookingConflictService.HasApprovedRoomConflict(booking.RoomID, booking.StartTime, booking.EndTime, booking.RoomBookingID))
                {
                    TempData["Error"] = "Không thể duyệt vì phòng đã có lịch trùng giờ được duyệt.";
                    return RedirectToAction(nameof(Index));
                }
                booking.Status = "Approved";
            }
            else if (decision == "Rejected")
            {
                booking.Status = "Rejected";
            }
            else
            {
                return BadRequest();
            }

            booking.ProcessedByStaffID = GetCurrentStaffId();
            booking.ProcessedAt = DateTime.Now;
            _context.SaveChanges();
            TempData["Success"] = booking.Status == "Approved" ? "Đã duyệt yêu cầu đặt phòng." : "Đã từ chối yêu cầu đặt phòng.";
            return RedirectToAction(nameof(Index));
        }
    }
}
