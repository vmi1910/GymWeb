using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using GymWeb.Models;

namespace GymWeb.Controllers
{
    // Quản lý trang thiết bị tại các phòng tập (Admin + Nhân viên)
    public class EquipmentController : Controller
    {
        private readonly DataContext _context;
        public EquipmentController(DataContext context) { _context = context; }

        private bool IsAdminOrStaff() => HttpContext.Session.GetString("Role") is "Admin" or "Staff";

        private void LoadRooms(int? roomId = null)
        {
            var rooms = _context.Rooms.OrderBy(r => r.RoomName)
                .Select(r => new { r.RoomID, Display = r.RoomName + " - tầng " + r.Floor })
                .ToList();
            ViewBag.RoomList = new SelectList(rooms, "RoomID", "Display", roomId);
        }

        public IActionResult Index(string? search)
        {
            if (!IsAdminOrStaff()) return RedirectToAction("Login", "Account");

            var query = from e in _context.Equipments select e;
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(e => e.EquipmentName.Contains(search) || e.Category.Contains(search));
            }

            var list = query.ToList();
            ViewData["Search"] = search;
            ViewBag.RoomNames = _context.Rooms.ToDictionary(r => r.RoomID, r => r.RoomName);
            return View(list.OrderBy(e => e.RoomID).ThenBy(e => e.EquipmentName).ToList());
        }

        [HttpGet]
        public IActionResult Create()
        {
            if (!IsAdminOrStaff()) return RedirectToAction("Login", "Account");
            LoadRooms();
            return View(new Equipment());
        }

        [HttpPost]
        public IActionResult Create(Equipment model)
        {
            if (!IsAdminOrStaff()) return RedirectToAction("Login", "Account");

            if (!_context.Rooms.Any(r => r.RoomID == model.RoomID))
            {
                ModelState.AddModelError("RoomID", "Phòng tập không hợp lệ.");
            }
            if (model.MaintenanceQuantity + model.BrokenQuantity > model.TotalQuantity)
            {
                ModelState.AddModelError("MaintenanceQuantity", "Số máy bảo trì + hỏng không được vượt quá tổng số lượng.");
            }
            if (!ModelState.IsValid)
            {
                LoadRooms(model.RoomID);
                return View(model);
            }

            _context.Equipments.Add(model);
            _context.SaveChanges();
            TempData["Success"] = "Thêm thiết bị thành công!";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            if (!IsAdminOrStaff()) return RedirectToAction("Login", "Account");

            var equipment = _context.Equipments.Find(id);
            if (equipment == null) return NotFound();

            LoadRooms(equipment.RoomID);
            return View(equipment);
        }

        [HttpPost]
        public IActionResult Edit(Equipment model)
        {
            if (!IsAdminOrStaff()) return RedirectToAction("Login", "Account");

            if (!_context.Rooms.Any(r => r.RoomID == model.RoomID))
            {
                ModelState.AddModelError("RoomID", "Phòng tập không hợp lệ.");
            }
            if (model.MaintenanceQuantity + model.BrokenQuantity > model.TotalQuantity)
            {
                ModelState.AddModelError("MaintenanceQuantity", "Số máy bảo trì + hỏng không được vượt quá tổng số lượng.");
            }
            if (!ModelState.IsValid)
            {
                LoadRooms(model.RoomID);
                return View(model);
            }

            var equipment = _context.Equipments.Find(model.EquipmentID);
            if (equipment == null) return NotFound();

            equipment.EquipmentName = model.EquipmentName;
            equipment.Category = model.Category;
            equipment.PurchaseDate = model.PurchaseDate;
            equipment.RoomID = model.RoomID;
            equipment.TotalQuantity = model.TotalQuantity;
            equipment.MaintenanceQuantity = model.MaintenanceQuantity;
            equipment.BrokenQuantity = model.BrokenQuantity;

            _context.SaveChanges();
            TempData["Success"] = "Cập nhật thiết bị thành công!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            if (!IsAdminOrStaff()) return RedirectToAction("Login", "Account");

            var equipment = _context.Equipments.Find(id);
            if (equipment != null)
            {
                _context.Equipments.Remove(equipment);
                _context.SaveChanges();
                TempData["Success"] = "Đã xóa thiết bị.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
