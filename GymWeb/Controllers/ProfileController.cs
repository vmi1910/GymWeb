using Microsoft.AspNetCore.Mvc;
using GymWeb.Models;

namespace GymWeb.Controllers
{
    // Khu vực tự quản lý thông tin của Hội viên (Member) đang đăng nhập
    public class ProfileController : Controller
    {
        private readonly DataContext _context;
        public ProfileController(DataContext context) { _context = context; }

        private Member? GetCurrentMember()
        {
            var username = HttpContext.Session.GetString("Username");
            if (username == null) return null;

            var account = _context.Accounts.FirstOrDefault(a => a.Username == username);
            if (account == null || account.Role != "Member") return null;

            return _context.Members.FirstOrDefault(m => m.AccountID == account.AccountID);
        }

        // Thông tin cá nhân + gói tập đang sử dụng
        public IActionResult Index()
        {
            var member = GetCurrentMember();
            if (member == null) return RedirectToAction("Login", "Account");

            var current = _context.Subscriptions
                .Where(s => s.MemberID == member.MemberID && _context.Payments.Any(p => p.SubscriptionID == s.SubscriptionID && p.Status == "Đã thanh toán"))
                .OrderByDescending(s => s.EndDate)
                .FirstOrDefault();

            MembershipPackage? currentPackage = null;
            if (current != null)
            {
                currentPackage = _context.MembershipPackages.Find(current.PackageID);
            }

            ViewBag.CurrentSubscription = current;
            ViewBag.CurrentPackage = currentPackage;
            ViewBag.IsActive = current != null && current.EndDate.Date >= DateTime.Today;

            return View(member);
        }

        [HttpGet]
        public IActionResult Edit()
        {
            var member = GetCurrentMember();
            if (member == null) return RedirectToAction("Login", "Account");
            return View(member);
        }

        [HttpPost]
        public IActionResult Edit(Member model)
        {
            var member = GetCurrentMember();
            if (member == null) return RedirectToAction("Login", "Account");

            if (string.IsNullOrWhiteSpace(model.FullName))
            {
                ModelState.AddModelError("FullName", "Họ và tên không được để trống");
            }

            if (!ModelState.IsValid) return View(model);

            // Chỉ cập nhật các trường hồ sơ cá nhân, không cho tự đổi Status/Role/Account
            member.FullName = model.FullName;
            member.DateOfBirth = model.DateOfBirth;
            member.Gender = model.Gender;
            member.PhoneNumber = model.PhoneNumber;
            member.Email = model.Email;
            member.Address = model.Address;

            var account = _context.Accounts.Find(member.AccountID);
            if (account != null) account.Email = model.Email;

            _context.SaveChanges();
            TempData["Success"] = "Cập nhật thông tin cá nhân thành công!";
            return RedirectToAction("Index");
        }

        // Danh sách gói tập để đăng ký / gia hạn
        public IActionResult Packages()
        {
            var member = GetCurrentMember();
            if (member == null) return RedirectToAction("Login", "Account");

            var packages = _context.MembershipPackages.Where(p => p.IsActive).OrderBy(p => p.Price).ToList();

            var current = _context.Subscriptions
                .Where(s => s.MemberID == member.MemberID && _context.Payments.Any(p => p.SubscriptionID == s.SubscriptionID && p.Status == "Đã thanh toán"))
                .OrderByDescending(s => s.EndDate)
                .FirstOrDefault();

            ViewBag.CurrentPackageId = (current != null && current.EndDate.Date >= DateTime.Today) ? current.PackageID : (int?)null;
            ViewBag.CurrentEndDate = current?.EndDate;

            return View(packages);
        }

        [HttpPost]
        public IActionResult Register(int packageId, string method)
        {
            var member = GetCurrentMember();
            if (member == null) return RedirectToAction("Login", "Account");

            var package = _context.MembershipPackages.Find(packageId);
            if (package == null || !package.IsActive)
            {
                TempData["Error"] = "Gói tập không hợp lệ hoặc đã ngừng bán.";
                return RedirectToAction("Packages");
            }
            if (method != "Tiền mặt" && method != "Chuyển khoản")
            {
                TempData["Error"] = "Vui lòng chọn phương thức thanh toán hợp lệ.";
                return RedirectToAction("Packages");
            }

            // Nếu đang có gói còn hạn (và đã thanh toán) thì cộng dồn thời gian (gia hạn), ngược lại tính từ hôm nay
            var latest = _context.Subscriptions
                .Where(s => s.MemberID == member.MemberID)
                .OrderByDescending(s => s.EndDate)
                .FirstOrDefault();
            var latestPaid = latest != null && _context.Payments.Any(p => p.SubscriptionID == latest.SubscriptionID && p.Status == "Đã thanh toán");

            var startDate = (latest != null && latestPaid && latest.EndDate.Date > DateTime.Today) ? latest.EndDate.Date : DateTime.Today;

            var subscription = new Subscription
            {
                MemberID = member.MemberID,
                PackageID = package.PackageID,
                StartDate = startDate,
                EndDate = startDate.AddDays(package.DurationDays),
                TotalAmount = package.Price
            };
            _context.Subscriptions.Add(subscription);
            _context.SaveChanges();

            // Thanh toán thật sự chỉ được xác nhận sau: tiền mặt cần NV xác nhận đã nhận,
            // chuyển khoản mô phỏng bằng QR + hội viên tự bấm xác nhận đã chuyển.
            var payment = new Payment
            {
                SubscriptionID = subscription.SubscriptionID,
                PaymentDate = DateTime.Now,
                Amount = package.Price,
                Method = method,
                Status = "Chờ xác nhận",
                StaffID = null
            };
            _context.Payments.Add(payment);
            _context.SaveChanges();

            return method == "Tiền mặt"
                ? RedirectToAction(nameof(PaymentPendingCash), new { id = payment.PaymentID })
                : RedirectToAction(nameof(PaymentQr), new { id = payment.PaymentID });
        }

        // Hội viên chọn "Tiền mặt": chỉ hoàn tất khi nhân viên xác nhận đã nhận tiền tại quầy
        public IActionResult PaymentPendingCash(int id)
        {
            var member = GetCurrentMember();
            if (member == null) return RedirectToAction("Login", "Account");

            var payment = _context.Payments.Find(id);
            var subscription = payment != null ? _context.Subscriptions.Find(payment.SubscriptionID) : null;
            if (payment == null || subscription == null || subscription.MemberID != member.MemberID || payment.Method != "Tiền mặt")
            {
                return RedirectToAction("Packages");
            }

            ViewBag.Package = _context.MembershipPackages.Find(subscription.PackageID);
            return View(payment);
        }

        // Hội viên chọn "Chuyển khoản": hiện QR mô phỏng + nút tự xác nhận đã chuyển khoản
        public IActionResult PaymentQr(int id)
        {
            var member = GetCurrentMember();
            if (member == null) return RedirectToAction("Login", "Account");

            var payment = _context.Payments.Find(id);
            var subscription = payment != null ? _context.Subscriptions.Find(payment.SubscriptionID) : null;
            if (payment == null || subscription == null || subscription.MemberID != member.MemberID || payment.Method != "Chuyển khoản")
            {
                return RedirectToAction("Packages");
            }

            ViewBag.Package = _context.MembershipPackages.Find(subscription.PackageID);
            return View(payment);
        }

        [HttpPost]
        public IActionResult ConfirmTransferPaid(int id)
        {
            var member = GetCurrentMember();
            if (member == null) return RedirectToAction("Login", "Account");

            var payment = _context.Payments.Find(id);
            var subscription = payment != null ? _context.Subscriptions.Find(payment.SubscriptionID) : null;
            if (payment == null || subscription == null || subscription.MemberID != member.MemberID || payment.Method != "Chuyển khoản")
            {
                return RedirectToAction("Packages");
            }

            if (payment.Status == "Chờ xác nhận")
            {
                payment.Status = "Đã thanh toán";
                _context.SaveChanges();
            }

            TempData["Success"] = "Xác nhận chuyển khoản thành công! Gói tập của bạn đã kích hoạt.";
            return RedirectToAction("History");
        }

        // Lịch sử thanh toán / các gói đã đăng ký
        public IActionResult History()
        {
            var member = GetCurrentMember();
            if (member == null) return RedirectToAction("Login", "Account");

            var subscriptions = _context.Subscriptions
                .Where(s => s.MemberID == member.MemberID)
                .OrderByDescending(s => s.StartDate)
                .ToList();

            var subIds = subscriptions.Select(s => s.SubscriptionID).ToList();
            var payments = _context.Payments
                .Where(p => subIds.Contains(p.SubscriptionID))
                .OrderByDescending(p => p.PaymentDate)
                .ToList();

            var packages = _context.MembershipPackages.ToDictionary(p => p.PackageID);
            var subscriptionsById = subscriptions.ToDictionary(s => s.SubscriptionID);

            ViewBag.Subscriptions = subscriptionsById;
            ViewBag.Packages = packages;

            return View(payments);
        }

        // Xem lịch hoạt động của phòng tập / huấn luyện viên
        public IActionResult Schedule()
        {
            var member = GetCurrentMember();
            if (member == null) return RedirectToAction("Login", "Account");

            var schedules = _context.StaffSchedules
                .Where(s => s.ShiftDate >= DateTime.Today)
                .OrderBy(s => s.ShiftDate)
                .ThenBy(s => s.StartTime)
                .ToList();

            ViewBag.StaffNames = _context.Staffs.ToDictionary(s => s.StaffID, s => s.FullName);
            ViewBag.RoomNames = _context.Rooms.ToDictionary(r => r.RoomID, r => r.RoomName);

            return View(schedules);
        }
    }
}
