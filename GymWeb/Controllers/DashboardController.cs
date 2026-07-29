using Microsoft.AspNetCore.Mvc;
using GymWeb.Models;
using GymWeb.ViewModels;

namespace GymWeb.Controllers
{
    // Thống kê doanh thu / tổng quan hệ thống (Admin)
    public class DashboardController : Controller
    {
        private readonly DataContext _context;
        public DashboardController(DataContext context) { _context = context; }

        private bool IsAdmin() => HttpContext.Session.GetString("Role") == "Admin";

        public IActionResult Index()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var payments = _context.Payments.ToList();
            var today = DateTime.Today;

            var model = new DashboardViewModel
            {
                TotalRevenue = payments.Sum(p => p.Amount),
                RevenueThisMonth = payments.Where(p => p.PaymentDate.Month == today.Month && p.PaymentDate.Year == today.Year).Sum(p => p.Amount),
                TotalMembers = _context.Members.Count(),
                ActivePackages = _context.MembershipPackages.Count(p => p.IsActive),
                TotalStaff = _context.Staffs.Count(s => s.Role == "Staff"),
                TotalTrainers = _context.Staffs.Count(s => s.Role == "Trainer")
            };

            // Doanh thu 6 tháng gần nhất
            for (int i = 5; i >= 0; i--)
            {
                var month = today.AddMonths(-i);
                var revenue = payments.Where(p => p.PaymentDate.Month == month.Month && p.PaymentDate.Year == month.Year).Sum(p => p.Amount);
                model.MonthlyLabels.Add("Th" + month.Month + "/" + month.Year);
                model.MonthlyRevenue.Add(revenue);
            }

            return View(model);
        }
    }
}
