using Microsoft.AspNetCore.Mvc;
using GymWeb.Models;

namespace GymWeb.Controllers
{
    // Xem danh sách đăng ký gói tập của hội viên (Admin + Staff) - chỉ đọc, việc tạo mới thực hiện qua Payment/Create
    public class SubscriptionController : Controller
    {
        private readonly DataContext _context;
        public SubscriptionController(DataContext context) { _context = context; }

        private bool IsAdminOrStaff()
        {
            var role = HttpContext.Session.GetString("Role");
            return role == "Admin" || role == "Staff";
        }

        public IActionResult Index()
        {
            if (!IsAdminOrStaff()) return RedirectToAction("Login", "Account");

            var list = _context.Subscriptions.OrderByDescending(s => s.StartDate).ToList();
            var members = _context.Members.ToDictionary(m => m.MemberID);
            var packages = _context.MembershipPackages.ToDictionary(p => p.PackageID);

            ViewBag.Members = members;
            ViewBag.Packages = packages;

            return View(list);
        }
    }
}
