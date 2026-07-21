using Microsoft.AspNetCore.Mvc;
using GymWeb.Models;

namespace GymWeb.Controllers
{
    // Quản lý gói tập (Admin)
    public class PackageController : Controller
    {
        private readonly DataContext _context;
        public PackageController(DataContext context) { _context = context; }

        private bool IsAdmin() => HttpContext.Session.GetString("Role") == "Admin";

        public IActionResult Index()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var list = _context.MembershipPackages.OrderBy(p => p.Price).ToList();
            return View(list);
        }

        [HttpGet]
        public IActionResult Create()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            return View(new MembershipPackage());
        }

        [HttpPost]
        public IActionResult Create(MembershipPackage model)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid) return View(model);

            _context.MembershipPackages.Add(model);
            _context.SaveChanges();
            TempData["Success"] = "Thêm gói tập mới thành công!";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var package = _context.MembershipPackages.Find(id);
            if (package == null) return NotFound();
            return View(package);
        }

        [HttpPost]
        public IActionResult Edit(MembershipPackage model)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid) return View(model);

            var package = _context.MembershipPackages.Find(model.PackageID);
            if (package == null) return NotFound();

            package.PackageName = model.PackageName;
            package.DurationDays = model.DurationDays;
            package.Price = model.Price;
            package.Description = model.Description;
            package.IsActive = model.IsActive;

            _context.SaveChanges();
            TempData["Success"] = "Cập nhật gói tập thành công!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult ToggleStatus(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var package = _context.MembershipPackages.Find(id);
            if (package != null)
            {
                package.IsActive = !package.IsActive;
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            bool inUse = _context.Subscriptions.Any(s => s.PackageID == id);
            if (inUse)
            {
                TempData["Error"] = "Không thể xóa gói tập này vì đã có hội viên đăng ký. Bạn có thể ngừng bán thay vì xóa.";
                return RedirectToAction("Index");
            }

            var package = _context.MembershipPackages.Find(id);
            if (package != null)
            {
                _context.MembershipPackages.Remove(package);
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }
    }
}
