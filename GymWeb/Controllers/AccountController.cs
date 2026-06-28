using Microsoft.AspNetCore.Mvc;
using GymWeb.Models;
using Microsoft.AspNetCore.Identity;

namespace ProjectWeb.Controllers // 
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
                    return RedirectToAction("Accounts", "Admin"); // Sang trang quản lý tài khoản của Admin
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
        
        public IActionResult Register()
        {
            return View();
        }

        
    }
}