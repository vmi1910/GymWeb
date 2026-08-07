using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using GymWeb.Models;
using GymWeb.ViewModels;

namespace GymWeb.Controllers
{
    // Quản lý thanh toán - dùng chung cho Admin và Nhân viên (Staff): lập hóa đơn, gia hạn gói tập
    public class PaymentController : Controller
    {
        private readonly DataContext _context;
        public PaymentController(DataContext context) { _context = context; }

        private bool IsAdminOrStaff()
        {
            var role = HttpContext.Session.GetString("Role");
            return role == "Admin" || role == "Staff";
        }

        private int? GetCurrentStaffId()
        {
            var username = HttpContext.Session.GetString("Username");
            if (username == null) return null;

            var account = _context.Accounts.FirstOrDefault(a => a.Username == username);
            if (account == null) return null;

            var staff = _context.Staffs.FirstOrDefault(s => s.AccountID == account.AccountID);
            return staff?.StaffID;
        }

        public IActionResult Index()
        {
            if (!IsAdminOrStaff()) return RedirectToAction("Login", "Account");

            var payments = _context.Payments.OrderByDescending(p => p.PaymentDate).ToList();
            var subscriptions = _context.Subscriptions.ToDictionary(s => s.SubscriptionID);
            var members = _context.Members.ToDictionary(m => m.MemberID);
            var packages = _context.MembershipPackages.ToDictionary(p => p.PackageID);
            var staffs = _context.Staffs.ToDictionary(s => s.StaffID);

            ViewBag.Subscriptions = subscriptions;
            ViewBag.Members = members;
            ViewBag.Packages = packages;
            ViewBag.Staffs = staffs;
            ViewBag.TotalRevenue = payments.Where(p => p.Status == "Đã thanh toán").Sum(p => p.Amount);

            return View(payments);
        }

        // Nhân viên xác nhận đã nhận tiền mặt tại quầy - hoàn tất thanh toán do hội viên tự đăng ký online
        [HttpPost]
        public IActionResult ConfirmCash(int id)
        {
            if (!IsAdminOrStaff()) return RedirectToAction("Login", "Account");

            var payment = _context.Payments.Find(id);
            if (payment == null || payment.Method != "Tiền mặt" || payment.Status != "Chờ xác nhận")
            {
                TempData["Error"] = "Hóa đơn không hợp lệ hoặc đã được xử lý.";
                return RedirectToAction(nameof(Index));
            }

            payment.Status = "Đã thanh toán";
            payment.StaffID = GetCurrentStaffId();
            _context.SaveChanges();
            TempData["Success"] = "Đã xác nhận nhận tiền mặt. Gói tập của hội viên đã kích hoạt.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Create(int? memberId)
        {
            if (!IsAdminOrStaff()) return RedirectToAction("Login", "Account");
            LoadDropdowns(memberId);
            return View(new PaymentCreateViewModel { MemberID = memberId ?? 0 });
        }

        private void LoadDropdowns(int? memberId = null, int? packageId = null)
        {
            var members = _context.Members.OrderBy(m => m.FullName)
                .Select(m => new { m.MemberID, Display = m.FullName + " - " + m.PhoneNumber })
                .ToList();
            ViewBag.MemberList = new SelectList(members, "MemberID", "Display", memberId);

            var packages = _context.MembershipPackages.Where(p => p.IsActive).OrderBy(p => p.Price)
                .Select(p => new { p.PackageID, Display = p.PackageName + " - " + p.Price.ToString("N0") + "đ / " + p.DurationDays + " ngày" })
                .ToList();
            ViewBag.PackageList = new SelectList(packages, "PackageID", "Display", packageId);
        }

        [HttpPost]
        public IActionResult Create(PaymentCreateViewModel model)
        {
            if (!IsAdminOrStaff()) return RedirectToAction("Login", "Account");

            var member = _context.Members.Find(model.MemberID);
            var package = _context.MembershipPackages.Find(model.PackageID);

            if (member == null) ModelState.AddModelError("MemberID", "Hội viên không hợp lệ");
            if (package == null) ModelState.AddModelError("PackageID", "Gói tập không hợp lệ");

            if (!ModelState.IsValid)
            {
                LoadDropdowns(model.MemberID, model.PackageID);
                return View(model);
            }

            // Nếu hội viên đang có gói còn hạn thì cộng dồn thời gian (gia hạn) thay vì tính lại từ hôm nay
            var latest = _context.Subscriptions
                .Where(s => s.MemberID == model.MemberID)
                .OrderByDescending(s => s.EndDate)
                .FirstOrDefault();

            var startDate = (latest != null && latest.EndDate.Date > DateTime.Today) ? latest.EndDate.Date : DateTime.Today;

            var subscription = new Subscription
            {
                MemberID = model.MemberID,
                PackageID = model.PackageID,
                StartDate = startDate,
                EndDate = startDate.AddDays(package!.DurationDays),
                TotalAmount = package.Price
            };
            _context.Subscriptions.Add(subscription);
            _context.SaveChanges();

            var payment = new Payment
            {
                SubscriptionID = subscription.SubscriptionID,
                PaymentDate = DateTime.Now,
                Amount = model.Amount > 0 ? model.Amount : package.Price,
                Method = model.Method,
                StaffID = GetCurrentStaffId()
            };
            _context.Payments.Add(payment);
            _context.SaveChanges();

            TempData["Success"] = "Lập hóa đơn thành công cho hội viên " + member!.FullName + "!";
            return RedirectToAction("Index");
        }
    }
}
