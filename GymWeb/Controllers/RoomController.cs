using Microsoft.AspNetCore.Mvc;
using GymWeb.Models;

namespace GymWeb.Controllers
{
    // Quản lý phòng tập (Admin) - hỗ trợ cho module Lịch tập
    public class RoomController : Controller
    {
        private readonly DataContext _context;
        public RoomController(DataContext context) { _context = context; }

        private bool IsAdmin() => HttpContext.Session.GetString("Role") == "Admin";

        public IActionResult Index()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var list = _context.Rooms.OrderBy(r => r.Floor).ThenBy(r => r.RoomName).ToList();
            return View(list);
        }

        [HttpGet]
        public IActionResult Create()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            return View(new Room());
        }

        [HttpPost]
        public IActionResult Create(Room model)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid) return View(model);

            _context.Rooms.Add(model);
            _context.SaveChanges();
            TempData["Success"] = "Thêm phòng tập thành công!";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var room = _context.Rooms.Find(id);
            if (room == null) return NotFound();
            return View(room);
        }

        [HttpPost]
        public IActionResult Edit(Room model)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid) return View(model);

            var room = _context.Rooms.Find(model.RoomID);
            if (room == null) return NotFound();

            room.RoomName = model.RoomName;
            room.Capacity = model.Capacity;
            room.Floor = model.Floor;
            room.Status = model.Status;

            _context.SaveChanges();
            TempData["Success"] = "Cập nhật phòng tập thành công!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            bool inUse = _context.StaffSchedules.Any(s => s.RoomID == id);
            if (inUse)
            {
                TempData["Error"] = "Không thể xóa phòng này vì đang có lịch tập gắn với phòng.";
                return RedirectToAction("Index");
            }

            var room = _context.Rooms.Find(id);
            if (room != null)
            {
                _context.Rooms.Remove(room);
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }
    }
}
