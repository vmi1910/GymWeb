using GymWeb.Models;
using GymWeb.Services;
using GymWeb.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GymWeb.Controllers
{
    // Hội viên chọn PT + đăng ký tập cùng trong ca làm PT đó đã tự đăng ký (MyScheduleController)
    public class TrainerBookingController : Controller
    {
        private readonly DataContext _context;
        private readonly BookingConflictService _bookingConflictService;

        public TrainerBookingController(DataContext context, BookingConflictService bookingConflictService)
        {
            _context = context;
            _bookingConflictService = bookingConflictService;
        }

        private Member? GetCurrentMember()
        {
            var username = HttpContext.Session.GetString("Username");
            if (username == null || HttpContext.Session.GetString("Role") != "Member") return null;

            var account = _context.Accounts.FirstOrDefault(a => a.Username == username);
            return account == null ? null : _context.Members.FirstOrDefault(m => m.AccountID == account.AccountID);
        }

        private Staff? GetCurrentStaff()
        {
            var username = HttpContext.Session.GetString("Username");
            var role = HttpContext.Session.GetString("Role");
            if (username == null || (role != "Staff" && role != "Trainer")) return null;

            return (from account in _context.Accounts
                    join item in _context.Staffs on account.AccountID equals item.AccountID
                    where account.Username == username
                    select item).FirstOrDefault();
        }

        private bool IsAdminOrStaff() => HttpContext.Session.GetString("Role") is "Admin" or "Staff";

        private void LoadTrainers(int? staffId = null)
        {
            var trainers = (from s in _context.Staffs
                             where s.Role == "Trainer" && s.Status == "Active"
                             join tp in _context.TrainerProfiles on s.StaffID equals tp.StaffID into tpj
                             from tp in tpj.DefaultIfEmpty()
                             orderby s.FullName
                             select new
                             {
                                 s.StaffID,
                                 Display = s.FullName + (tp != null && !string.IsNullOrWhiteSpace(tp.Specialty) ? " - " + tp.Specialty : "")
                             }).ToList();
            ViewBag.TrainerList = new SelectList(trainers, "StaffID", "Display", staffId);
        }

        [HttpGet]
        private List<StaffSchedule> LoadUpcomingShifts(int staffId)
        {
            var shifts = _context.StaffSchedules
                .Where(s => s.StaffID == staffId && s.ShiftDate >= DateTime.Today)
                .OrderBy(s => s.ShiftDate).ThenBy(s => s.StartTime)
                .Take(15)
                .ToList();
            ViewBag.RoomNames = _context.Rooms.ToDictionary(r => r.RoomID, r => r.RoomName);
            return shifts;
        }

        public IActionResult Create(int? staffId)
        {
            if (GetCurrentMember() == null) return RedirectToAction("Login", "Account");

            LoadTrainers(staffId);
            if (staffId.HasValue)
            {
                ViewBag.TrainerShifts = LoadUpcomingShifts(staffId.Value);
            }

            return View(new TrainerBookingCreateViewModel { StaffID = staffId ?? 0 });
        }

        [HttpPost]
        public IActionResult Create(TrainerBookingCreateViewModel model)
        {
            var member = GetCurrentMember();
            if (member == null) return RedirectToAction("Login", "Account");

            var trainer = _context.Staffs.FirstOrDefault(s => s.StaffID == model.StaffID && s.Role == "Trainer");
            if (trainer == null)
            {
                ModelState.AddModelError("StaffID", "Huấn luyện viên không hợp lệ.");
            }

            var schedule = _context.StaffSchedules.FirstOrDefault(s => s.ScheduleID == model.ScheduleID && s.StaffID == model.StaffID);
            if (schedule == null)
            {
                ModelState.AddModelError(string.Empty, "Ca làm việc không hợp lệ, vui lòng chọn lại.");
            }

            DateTime startTime = default, endTime = default;
            if (schedule != null)
            {
                startTime = schedule.ShiftDate.Add(schedule.StartTime);
                endTime = schedule.ShiftDate.Add(schedule.EndTime);

                if (startTime < DateTime.Now)
                {
                    ModelState.AddModelError(string.Empty, "Ca làm việc này đã diễn ra, vui lòng chọn ca khác.");
                }
                else if (_bookingConflictService.HasApprovedTrainerConflict(model.StaffID, startTime, endTime))
                {
                    ModelState.AddModelError(string.Empty, "Huấn luyện viên đã có lịch tập được duyệt trùng ca này. Vui lòng chọn ca khác.");
                }
            }

            if (!ModelState.IsValid)
            {
                LoadTrainers(model.StaffID);
                if (model.StaffID > 0)
                {
                    ViewBag.TrainerShifts = LoadUpcomingShifts(model.StaffID);
                }
                return View(model);
            }

            _context.TrainerBookings.Add(new TrainerBooking
            {
                MemberID = member.MemberID,
                StaffID = model.StaffID,
                StartTime = startTime,
                EndTime = endTime,
                Note = model.Note?.Trim()
            });
            _context.SaveChanges();
            TempData["Success"] = "Yêu cầu tập cùng huấn luyện viên đã được gửi và đang chờ duyệt.";
            return RedirectToAction(nameof(MyBookings));
        }

        public IActionResult MyBookings()
        {
            var member = GetCurrentMember();
            if (member == null) return RedirectToAction("Login", "Account");

            var bookings = _context.TrainerBookings.Where(b => b.MemberID == member.MemberID)
                .OrderByDescending(b => b.StartTime).ToList();
            ViewBag.StaffNames = _context.Staffs.ToDictionary(s => s.StaffID, s => s.FullName);
            return View(bookings);
        }

        [HttpPost]
        public IActionResult Cancel(int id)
        {
            var member = GetCurrentMember();
            if (member == null) return RedirectToAction("Login", "Account");

            var booking = _context.TrainerBookings.FirstOrDefault(b => b.TrainerBookingID == id && b.MemberID == member.MemberID);
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
            TempData["Success"] = "Đã hủy yêu cầu tập cùng huấn luyện viên.";
            return RedirectToAction(nameof(MyBookings));
        }

        // Trainer chỉ thấy yêu cầu của chính mình; Admin/Staff thấy toàn bộ
        public IActionResult Index()
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Admin" && role != "Staff" && role != "Trainer") return RedirectToAction("Login", "Account");

            var query = _context.TrainerBookings.AsQueryable();
            if (role == "Trainer")
            {
                var trainer = GetCurrentStaff();
                if (trainer == null) return RedirectToAction("Login", "Account");
                query = query.Where(b => b.StaffID == trainer.StaffID);
            }

            var bookings = query.OrderBy(b => b.Status == "Pending" ? 0 : 1).ThenBy(b => b.StartTime).ToList();
            ViewBag.MemberNames = _context.Members.ToDictionary(m => m.MemberID, m => m.FullName);
            ViewBag.StaffNames = _context.Staffs.ToDictionary(s => s.StaffID, s => s.FullName);
            return View(bookings);
        }

        [HttpPost]
        public IActionResult Process(int id, string decision)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Admin" && role != "Staff" && role != "Trainer") return RedirectToAction("Login", "Account");

            var booking = _context.TrainerBookings.Find(id);
            if (booking == null) return NotFound();

            if (role == "Trainer")
            {
                var trainer = GetCurrentStaff();
                if (trainer == null || booking.StaffID != trainer.StaffID) return Forbid();
            }

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
                if (!_bookingConflictService.IsWithinTrainerShift(booking.StaffID, booking.StartTime, booking.EndTime))
                {
                    TempData["Error"] = "Không thể duyệt vì khung giờ này không còn nằm trong ca làm việc của huấn luyện viên.";
                    return RedirectToAction(nameof(Index));
                }
                if (_bookingConflictService.HasApprovedTrainerConflict(booking.StaffID, booking.StartTime, booking.EndTime, booking.TrainerBookingID))
                {
                    TempData["Error"] = "Không thể duyệt vì huấn luyện viên đã có lịch được duyệt trùng giờ.";
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

            booking.ProcessedByStaffID = GetCurrentStaff()?.StaffID;
            booking.ProcessedAt = DateTime.Now;
            _context.SaveChanges();
            TempData["Success"] = booking.Status == "Approved" ? "Đã duyệt yêu cầu tập cùng PT." : "Đã từ chối yêu cầu.";
            return RedirectToAction(nameof(Index));
        }
    }
}
