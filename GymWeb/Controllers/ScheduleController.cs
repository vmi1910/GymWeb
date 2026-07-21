using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using GymWeb.Models;

namespace GymWeb.Controllers
{
    // Quản lý lịch tập / lịch làm việc của nhân viên - huấn luyện viên (Admin)
    public class ScheduleController : Controller
    {
        private readonly DataContext _context;
        public ScheduleController(DataContext context) { _context = context; }

        private bool IsAdmin() => HttpContext.Session.GetString("Role") == "Admin";

        private void LoadDropdowns(int? staffId = null, int? roomId = null)
        {
            var staffList = _context.Staffs
                .Where(s => s.Role == "Staff" || s.Role == "Trainer")
                .OrderBy(s => s.FullName)
                .Select(s => new { s.StaffID, Display = s.FullName + " (" + s.Role + ")" })
                .ToList();

            ViewBag.StaffList = new SelectList(staffList, "StaffID", "Display", staffId);
            ViewBag.RoomList = new SelectList(_context.Rooms.OrderBy(r => r.RoomName).ToList(), "RoomID", "RoomName", roomId);
        }

        public IActionResult Index()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var list = _context.StaffSchedules
                .OrderByDescending(s => s.ShiftDate)
                .ThenBy(s => s.StartTime)
                .ToList();

            // Nạp kèm tên nhân viên / phòng để hiển thị (không dùng Include để tránh phụ thuộc navigation chưa cấu hình)
            var staffNames = _context.Staffs.ToDictionary(s => s.StaffID, s => s.FullName);
            var roomNames = _context.Rooms.ToDictionary(r => r.RoomID, r => r.RoomName);
            ViewBag.StaffNames = staffNames;
            ViewBag.RoomNames = roomNames;

            return View(list);
        }

        [HttpGet]
        public IActionResult Create()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            LoadDropdowns();
            return View(new StaffSchedule { StartTime = new TimeSpan(8, 0, 0), EndTime = new TimeSpan(17, 0, 0) });
        }

        [HttpPost]
        public IActionResult Create(StaffSchedule model)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            ModelState.Remove("Staff");
            ModelState.Remove("Room");

            if (model.EndTime <= model.StartTime)
            {
                ModelState.AddModelError("", "Giờ kết thúc phải sau giờ bắt đầu!");
            }

            if (!ModelState.IsValid)
            {
                LoadDropdowns(model.StaffID, model.RoomID);
                return View(model);
            }

            _context.StaffSchedules.Add(model);
            _context.SaveChanges();
            TempData["Success"] = "Thêm lịch tập thành công!";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var schedule = _context.StaffSchedules.Find(id);
            if (schedule == null) return NotFound();

            LoadDropdowns(schedule.StaffID, schedule.RoomID);
            return View(schedule);
        }

        [HttpPost]
        public IActionResult Edit(StaffSchedule model)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            ModelState.Remove("Staff");
            ModelState.Remove("Room");

            if (model.EndTime <= model.StartTime)
            {
                ModelState.AddModelError("", "Giờ kết thúc phải sau giờ bắt đầu!");
            }

            if (!ModelState.IsValid)
            {
                LoadDropdowns(model.StaffID, model.RoomID);
                return View(model);
            }

            var schedule = _context.StaffSchedules.Find(model.ScheduleID);
            if (schedule == null) return NotFound();

            schedule.StaffID = model.StaffID;
            schedule.RoomID = model.RoomID;
            schedule.ShiftDate = model.ShiftDate;
            schedule.StartTime = model.StartTime;
            schedule.EndTime = model.EndTime;

            _context.SaveChanges();
            TempData["Success"] = "Cập nhật lịch tập thành công!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var schedule = _context.StaffSchedules.Find(id);
            if (schedule != null)
            {
                _context.StaffSchedules.Remove(schedule);
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }
    }
}
