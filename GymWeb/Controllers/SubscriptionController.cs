using Microsoft.AspNetCore.Mvc;
using GymWeb.Models;

namespace GymWeb.Controllers
{
    // Xem danh sách đăng ký gói tập của hội viên (Admin) - chỉ đọc, việc tạo mới thực hiện qua Payment/Create
    public class SubscriptionController : Controller
    {
        private readonly DataContext _context;
        public SubscriptionController(DataContext context) { _context = context; }

        private bool IsAdmin() => HttpContext.Session.GetString("Role") == "Admin";

        public IActionResult Index()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var list = _context.Subscriptions.OrderByDescending(s => s.StartDate).ToList();
            var members = _context.Members.ToDictionary(m => m.MemberID);
            var packages = _context.MembershipPackages.ToDictionary(p => p.PackageID);

            ViewBag.Members = members;
            ViewBag.Packages = packages;

            return View(list);
        }
    }
}
