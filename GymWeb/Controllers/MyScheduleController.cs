using GymWeb.Models;
using GymWeb.Services;
using GymWeb.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GymWeb.Controllers
{
    public class MyScheduleController : Controller
    {
        private readonly DataContext _context;
        private readonly ScheduleConflictService _scheduleConflictService;

        public MyScheduleController(DataContext context, ScheduleConflictService scheduleConflictService)
        {
            _context = context;
            _scheduleConflictService = scheduleConflictService;
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

        private void LoadRooms(int? roomId = null)
        {
            var rooms = _context.Rooms.OrderBy(r => r.RoomName)
                .Select(r => new { r.RoomID, Display = r.RoomName + " - tầng " + r.Floor + " (" + r.Capacity + " người)" })
                .ToList();
            ViewBag.RoomList = new SelectList(rooms, "RoomID", "Display", roomId);
        }

        public IActionResult Index()
        {
            var staff = GetCurrentStaff();
            if (staff == null) return RedirectToAction("Login", "Account");

            var schedules = _context.StaffSchedules.Where(s => s.StaffID == staff.StaffID)
                .OrderBy(s => s.ShiftDate).ThenBy(s => s.StartTime).ToList();
            ViewBag.Staff = staff;
            ViewBag.RoomNames = _context.Rooms.ToDictionary(r => r.RoomID, r => r.RoomName);
            return View(schedules);
        }

        [HttpGet]
        public IActionResult Register()
        {
            if (GetCurrentStaff() == null) return RedirectToAction("Login", "Account");

            LoadRooms();
            return View(new ScheduleRegistrationViewModel());
        }

        [HttpPost]
        public IActionResult Register(ScheduleRegistrationViewModel model)
        {
            var staff = GetCurrentStaff();
            if (staff == null) return RedirectToAction("Login", "Account");

            if (model.EndTime <= model.StartTime)
            {
                ModelState.AddModelError(string.Empty, "Giờ kết thúc phải sau giờ bắt đầu.");
            }
            if (model.ShiftDate.Date < DateTime.Today ||
                (model.ShiftDate.Date == DateTime.Today && model.StartTime <= DateTime.Now.TimeOfDay))
            {
                ModelState.AddModelError("ShiftDate", "Chỉ có thể đăng ký ca làm việc trong tương lai.");
            }
            if (!_context.Rooms.Any(r => r.RoomID == model.RoomID))
            {
                ModelState.AddModelError("RoomID", "Phòng tập không hợp lệ.");
            }
            if (ModelState.IsValid && _scheduleConflictService.HasConflict(staff.StaffID, model.RoomID, model.ShiftDate, model.StartTime, model.EndTime))
            {
                ModelState.AddModelError(string.Empty, "Ca đăng ký trùng với lịch của nhân viên khác hoặc lịch sử dụng phòng.");
            }

            if (!ModelState.IsValid)
            {
                LoadRooms(model.RoomID);
                return View(model);
            }

            _context.StaffSchedules.Add(new StaffSchedule
            {
                StaffID = staff.StaffID,
                RoomID = model.RoomID,
                ShiftDate = model.ShiftDate.Date,
                StartTime = model.StartTime,
                EndTime = model.EndTime
            });
            _context.SaveChanges();
            TempData["Success"] = "Đã đăng ký ca làm việc thành công.";
            return RedirectToAction(nameof(Index));
        }
    }
}
