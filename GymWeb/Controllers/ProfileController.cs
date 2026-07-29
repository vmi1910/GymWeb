using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using GymWeb.Models;
using GymWeb.ViewModels;

namespace GymWeb.Controllers
{
    // Khu vực tự phục vụ dành cho Hội viên - xem và chỉnh sửa hồ sơ cá nhân của chính mình.
    // Hiện chỉ áp dụng cho tài khoản Role = "Member" (Staff/Admin có hồ sơ quản lý riêng ở khu Admin).
    public class ProfileController : Controller
    {
        private readonly DataContext _context;
        public ProfileController(DataContext context) { _context = context; }

        // Lấy Account + Member ứng với tài khoản đang đăng nhập trong session
        private (Account? Account, Member? Member) GetCurrentMemberAccount()
        {
            var username = HttpContext.Session.GetString("Username");
            if (username == null) return (null, null);

            var account = _context.Accounts.FirstOrDefault(a => a.Username == username);
            if (account == null || account.Role != "Member") return (account, null);

            var member = _context.Members.FirstOrDefault(m => m.AccountID == account.AccountID);
            return (account, member);
        }

        [HttpGet]
        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("Username") == null) return RedirectToAction("Login", "Account");

            var (account, member) = GetCurrentMemberAccount();
            if (account == null || member == null)
            {
                TempData["Error"] = "Chức năng hồ sơ cá nhân hiện chỉ áp dụng cho tài khoản Hội viên.";
                return RedirectToAction("Index", "Home");
            }

            var model = new ProfileViewModel
            {
                Username = account.Username,
                RegisterDate = member.RegisterDate,
                Status = member.Status,
                FullName = member.FullName,
                DateOfBirth = member.DateOfBirth,
                Gender = member.Gender,
                PhoneNumber = member.PhoneNumber,
                Email = member.Email,
                Address = member.Address
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult Index(ProfileViewModel model)
        {
            if (HttpContext.Session.GetString("Username") == null) return RedirectToAction("Login", "Account");

            var (account, member) = GetCurrentMemberAccount();
            if (account == null || member == null)
            {
                TempData["Error"] = "Chức năng hồ sơ cá nhân hiện chỉ áp dụng cho tài khoản Hội viên.";
                return RedirectToAction("Index", "Home");
            }

            // Username/RegisterDate/Status chỉ đọc - lấy lại từ DB, không tin dữ liệu POST lên
            model.Username = account.Username;
            model.RegisterDate = member.RegisterDate;
            model.Status = member.Status;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            member.FullName = model.FullName;
            member.DateOfBirth = model.DateOfBirth;
            member.Gender = model.Gender;
            member.PhoneNumber = model.PhoneNumber;
            member.Email = model.Email;
            member.Address = model.Address;

            // Chỉ đổi mật khẩu nếu người dùng có nhập mật khẩu mới
            if (!string.IsNullOrWhiteSpace(model.NewPassword))
            {
                var hasher = new PasswordHasher<Account>();
                account.PasswordHash = hasher.HashPassword(account, model.NewPassword);
            }

            _context.SaveChanges();
            TempData["Success"] = "Cập nhật hồ sơ thành công!";
            return RedirectToAction("Index");
        }
    }
}