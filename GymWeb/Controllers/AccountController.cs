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
                // Đăng nhập thành công -> Lưu vào Session
                HttpContext.Session.SetString("Username", user.Username);
                HttpContext.Session.SetString("Role", user.Role);
                return RedirectToAction("Index", "Home");
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