using Microsoft.AspNetCore.Mvc;
using GymWeb.Models;
using GymWeb.ViewModels;
using Microsoft.AspNetCore.Identity;

namespace GymWeb.Controllers
{
    public class AccountController : Controller
    {
        private readonly DataContext _context;
        public AccountController(DataContext context) { _context = context; }
        // Hàm này sẽ trả về giao diện trang đăng nhập (GET)
        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            var user = _context.Accounts.FirstOrDefault(a => a.Username == username);
            var hasher = new PasswordHasher<Account>();

            if (user != null && hasher.VerifyHashedPassword(user, user.PasswordHash, password) == PasswordVerificationResult.Success)
            {
                HttpContext.Session.SetString("Username", user.Username);
                HttpContext.Session.SetString("Role", user.Role);

                // KIỂM TRA ROLE ĐỂ ĐIỀU HƯỚNG
                if (user.Role == "Admin")
                {
                    return RedirectToAction("Index", "Dashboard"); // Sang trang thống kê tổng quan của Admin
                }
    
                return RedirectToAction("Index", "Home"); // Khách thường về trang chủ
            }
            ModelState.AddModelError("", "Sai tài khoản hoặc mật khẩu!");
            return View();
        }

    public IActionResult Logout()
    {
        HttpContext.Session.Clear(); // Xóa session khi đăng xuất
        return RedirectToAction("Index", "Home");
    }
        
        [HttpGet]
        public IActionResult Register() => View(new AccountCreateViewModel());

        [HttpPost]
        public IActionResult Register(AccountCreateViewModel model)
        {
            // Người dùng tự đăng ký luôn là Member, không cho chọn Role
            model.Role = "Member";

            if (_context.Accounts.Any(a => a.Username == model.Username))
            {
                ModelState.AddModelError("Username", "Tên đăng nhập này đã tồn tại!");
            }

            if (string.IsNullOrWhiteSpace(model.RawPassword))
            {
                ModelState.AddModelError("RawPassword", "Mật khẩu không được để trống");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var account = new Account { Username = model.Username, Role = "Member" };
            var hasher = new PasswordHasher<Account>();
            account.PasswordHash = hasher.HashPassword(account, model.RawPassword);

            _context.Accounts.Add(account);
            _context.SaveChanges(); // Lưu để lấy AccountID tự sinh

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

            // Tự động đăng nhập sau khi đăng ký thành công
            HttpContext.Session.SetString("Username", account.Username);
            HttpContext.Session.SetString("Role", account.Role);

            return RedirectToAction("Index", "Home");
        }
    }
}