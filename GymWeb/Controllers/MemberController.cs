using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using GymWeb.Models;
using GymWeb.ViewModels;

namespace GymWeb.Controllers
{
    // Quản lý hội viên - dùng chung cho Admin và Nhân viên (Staff)
    public class MemberController : Controller
    {
        private readonly DataContext _context;
        public MemberController(DataContext context) { _context = context; }

        private bool IsAdmin() => HttpContext.Session.GetString("Role") == "Admin";
        private bool IsAdminOrStaff()
        {
            var role = HttpContext.Session.GetString("Role");
            return role == "Admin" || role == "Staff";
        }

        public IActionResult Index(string? search)
        {
            if (!IsAdminOrStaff()) return RedirectToAction("Login", "Account");

            var query = _context.Members.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(m => m.FullName.Contains(search) || m.PhoneNumber.Contains(search) || m.Email.Contains(search));
            }

            ViewData["Search"] = search;
            var list = query.OrderBy(m => m.FullName).ToList();
            return View(list);
        }

        // Tra cứu thông tin khách hàng: hồ sơ + gói tập + lịch sử thanh toán (chỉ xem)
        public IActionResult Details(int id)
        {
            if (!IsAdminOrStaff()) return RedirectToAction("Login", "Account");

            var member = _context.Members.Find(id);
            if (member == null) return NotFound();

            var subscriptions = _context.Subscriptions
                .Where(s => s.MemberID == id)
                .OrderByDescending(s => s.StartDate)
                .ToList();

            var subIds = subscriptions.Select(s => s.SubscriptionID).ToList();
            var payments = _context.Payments
                .Where(p => subIds.Contains(p.SubscriptionID))
                .OrderByDescending(p => p.PaymentDate)
                .ToList();

            ViewBag.Subscriptions = subscriptions;
            ViewBag.Payments = payments;
            ViewBag.Packages = _context.MembershipPackages.ToDictionary(p => p.PackageID);

            return View(member);
        }

        // Tiếp nhận đăng ký hội viên mới (Staff/Admin) - chỉ tạo tài khoản Role = Member
        [HttpGet]
        public IActionResult Create()
        {
            if (!IsAdminOrStaff()) return RedirectToAction("Login", "Account");
            return View(new AccountCreateViewModel { Role = "Member" });
        }

        [HttpPost]
        public IActionResult Create(AccountCreateViewModel model)
        {
            if (!IsAdminOrStaff()) return RedirectToAction("Login", "Account");

            model.Role = "Member"; // Staff/Admin chỉ được tạo hội viên qua màn này

            if (_context.Accounts.Any(a => a.Username == model.Username))
            {
                ModelState.AddModelError("Username", "Tên đăng nhập này đã tồn tại!");
            }

            if (string.IsNullOrWhiteSpace(model.RawPassword))
            {
                ModelState.AddModelError("RawPassword", "Mật khẩu không được để trống");
            }

            if (!ModelState.IsValid) return View(model);

            var account = new Account { Username = model.Username, Role = "Member" };
            var hasher = new PasswordHasher<Account>();
            account.PasswordHash = hasher.HashPassword(account, model.RawPassword);

            _context.Accounts.Add(account);
            _context.SaveChanges();

            var member = new Member
            {
                FullName = model.FullName,
                PhoneNumber = model.PhoneNumber,
                Email = model.Email,
                Gender = model.Gender,
                AccountID = account.AccountID
            };
            _context.Members.Add(member);
            _context.SaveChanges();

            TempData["Success"] = "Thêm hội viên mới thành công!";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            if (!IsAdminOrStaff()) return RedirectToAction("Login", "Account");

            var member = _context.Members.Find(id);
            if (member == null) return NotFound();
            return View(member);
        }

        [HttpPost]
        public IActionResult Edit(Member model, string? role)
        {
            if (!IsAdminOrStaff()) return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid) return View(model);

            var member = _context.Members.Find(model.MemberID);
            if (member == null) return NotFound();

            var allowedRoles = new[] { "Staff", "Trainer" }; // Không cho thăng "Admin" qua màn này, dùng màn Admin/Create riêng
            var newRole = (IsAdmin() && allowedRoles.Contains(role)) ? role! : "Member";

            if (newRole != "Member")
            {
                // Chặn nếu hội viên đã có lịch sử gói tập (tránh mất dữ liệu Subscription/Payment)
                if (_context.Subscriptions.Any(s => s.MemberID == member.MemberID))
                {
                    ModelState.AddModelError("", "Không thể đổi vai trò: hội viên này đã có gói tập/hóa đơn thanh toán.");
                    return View(model);
                }

                var account = _context.Accounts.Find(member.AccountID);
                if (account == null) return NotFound();

                account.Role = newRole;

                var newStaff = new Staff
                {
                    FullName = model.FullName,
                    DateOfBirth = model.DateOfBirth,
                    Gender = model.Gender,
                    PhoneNumber = model.PhoneNumber,
                    Email = model.Email,
                    Address = model.Address,
                    Role = newRole,
                    AccountID = account.AccountID
                };
                _context.Members.Remove(member);
                _context.Staffs.Add(newStaff);
                _context.SaveChanges(); // lấy StaffID trước khi tạo hồ sơ Trainer

                if (newRole == "Trainer")
                {
                    _context.TrainerProfiles.Add(new TrainerProfile { StaffID = newStaff.StaffID });
                    _context.SaveChanges();
                }

                TempData["Success"] = "Đã chuyển tài khoản sang vai trò " + newRole + ". Vào mục tương ứng để bổ sung thêm hồ sơ (VD: ảnh chứng chỉ HLV).";
                return RedirectToAction("Index");
            }

            member.FullName = model.FullName;
            member.DateOfBirth = model.DateOfBirth;
            member.Gender = model.Gender;
            member.PhoneNumber = model.PhoneNumber;
            member.Email = model.Email;
            member.Address = model.Address;
            member.Status = model.Status;

            _context.SaveChanges();
            TempData["Success"] = "Cập nhật thông tin hội viên thành công!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            // Chỉ Admin được xóa hội viên, Staff chỉ được xem/cập nhật
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var member = _context.Members.Find(id);
            if (member != null)
            {
                // Xóa Account gốc sẽ cascade xóa luôn hồ sơ Member
                var account = _context.Accounts.Find(member.AccountID);
                if (account != null)
                {
                    _context.Accounts.Remove(account);
                }
                else
                {
                    _context.Members.Remove(member);
                }
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }
    }
}
