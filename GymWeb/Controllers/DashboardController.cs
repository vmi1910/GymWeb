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
            var subscriptions = _context.Subscriptions.ToList();
            var members = _context.Members.ToList();
            var packages = _context.MembershipPackages.ToDictionary(p => p.PackageID);
            var today = DateTime.Today;

            var revenueThisMonth = payments.Where(p => p.PaymentDate.Month == today.Month && p.PaymentDate.Year == today.Year).Sum(p => p.Amount);
            var lastMonth = today.AddMonths(-1);
            var revenueLastMonth = payments.Where(p => p.PaymentDate.Month == lastMonth.Month && p.PaymentDate.Year == lastMonth.Year).Sum(p => p.Amount);

            var model = new DashboardViewModel
            {
                TotalRevenue = payments.Sum(p => p.Amount),
                RevenueThisMonth = revenueThisMonth,
                RevenueLastMonth = revenueLastMonth,
                RevenueTrendPercent = revenueLastMonth > 0 ? (double)((revenueThisMonth - revenueLastMonth) / revenueLastMonth * 100) : null,
                TotalMembers = members.Count,
                NewMembersThisMonth = members.Count(m => m.RegisterDate.Month == today.Month && m.RegisterDate.Year == today.Year),
                ActivePackages = packages.Values.Count(p => p.IsActive),
                ActiveSubscriptions = subscriptions.Count(s => s.EndDate.Date >= today),
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

            // Độ phổ biến từng gói tập (dựa trên số lượt đăng ký)
            var popularity = subscriptions
                .Where(s => packages.ContainsKey(s.PackageID))
                .GroupBy(s => packages[s.PackageID].PackageName)
                .Select(g => new { Name = g.Key, Count = g.Count() })
                .OrderByDescending(g => g.Count)
                .Take(5)
                .ToList();
            model.PackagePopularityLabels = popularity.Select(p => p.Name).ToList();
            model.PackagePopularityCounts = popularity.Select(p => p.Count).ToList();

            // Hoạt động thanh toán gần đây
            var subsById = subscriptions.ToDictionary(s => s.SubscriptionID);
            var membersById = members.ToDictionary(m => m.MemberID);
            model.RecentPayments = payments
                .OrderByDescending(p => p.PaymentDate)
                .Take(6)
                .Select(p =>
                {
                    var sub = subsById.GetValueOrDefault(p.SubscriptionID);
                    var member = sub != null ? membersById.GetValueOrDefault(sub.MemberID) : null;
                    var package = sub != null ? packages.GetValueOrDefault(sub.PackageID) : null;
                    return new RecentPaymentItem
                    {
                        MemberName = member?.FullName ?? "—",
                        PackageName = package?.PackageName ?? "—",
                        Amount = p.Amount,
                        PaymentDate = p.PaymentDate,
                        Method = p.Method
                    };
                })
                .ToList();

            return View(model);
        }
    }
}
