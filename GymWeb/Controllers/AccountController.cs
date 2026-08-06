using Microsoft.AspNetCore.Mvc;
using GymWeb.Models;
using GymWeb.ViewModels;
using GymWeb.Services;
using Microsoft.AspNetCore.Identity;
using System.Security.Cryptography;

namespace GymWeb.Controllers
{
    public class AccountController : Controller
    {
        private readonly DataContext _context;
        private readonly IEmailSender _emailSender;
        public AccountController(DataContext context, IEmailSender emailSender)
        {
            _context = context;
            _emailSender = emailSender;
        }
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

                if (user.Role == "Staff")
                {
                    return RedirectToAction("Index", "Member"); // Nhân viên vào thẳng khu vực quản lý hội viên
                }

                if (user.Role == "Trainer")
                {
                    return RedirectToAction("Index", "MySchedule");
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

            var account = new Account { Username = model.Username, Role = "Member", Email = model.Email };
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

        // Bước 1: Nhập tên đăng nhập để nhận mã OTP qua email
        [HttpGet]
        public IActionResult ForgotPassword() => View(new ForgotPasswordViewModel());

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var account = _context.Accounts.FirstOrDefault(a => a.Username == model.Username);
            if (account == null || string.IsNullOrWhiteSpace(account.Email))
            {
                ModelState.AddModelError("", "Không tìm thấy tài khoản này, hoặc tài khoản chưa có email khôi phục. Vui lòng liên hệ quản trị viên.");
                return View(model);
            }

            var otp = RandomNumberGenerator.GetInt32(0, 1000000).ToString("D6");
            account.ResetOtpCode = otp;
            account.ResetOtpExpiryUtc = DateTime.UtcNow.AddMinutes(5);
            _context.SaveChanges();

            var html = $@"
                <div style='font-family:Arial,sans-serif;max-width:480px;margin:auto;padding:24px;border:1px solid #eee;border-radius:12px;'>
                    <h2 style='color:#d90429;margin-bottom:4px;'>GYM FITNESS</h2>
                    <p>Xin chào <b>{account.Username}</b>,</p>
                    <p>Mã OTP để đặt lại mật khẩu của bạn là:</p>
                    <p style='font-size:32px;font-weight:bold;letter-spacing:6px;color:#111;background:#f5f5f5;padding:12px 16px;border-radius:8px;text-align:center;'>{otp}</p>
                    <p>Mã có hiệu lực trong <b>5 phút</b>. Nếu bạn không yêu cầu đặt lại mật khẩu, vui lòng bỏ qua email này.</p>
                    <hr style='border:none;border-top:1px solid #eee;margin:20px 0;' />
                    <p style='color:#888;font-size:12px;'>Email tự động, vui lòng không trả lời.</p>
                </div>";

            try
            {
                await _emailSender.SendEmailAsync(account.Email!, "Mã OTP khôi phục mật khẩu - GYM FITNESS", html);
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Không thể gửi email lúc này. Vui lòng thử lại sau ít phút.");
                return View(model);
            }

            TempData["Info"] = $"Mã OTP đã được gửi tới {MaskEmail(account.Email!)}. Mã có hiệu lực trong 5 phút.";
            return RedirectToAction("ResetPassword", new { username = account.Username });
        }

        // Bước 2: Nhập mã OTP + mật khẩu mới
        [HttpGet]
        public IActionResult ResetPassword(string username)
        {
            if (string.IsNullOrWhiteSpace(username)) return RedirectToAction("ForgotPassword");
            return View(new ResetPasswordOtpViewModel { Username = username });
        }

        [HttpPost]
        public IActionResult ResetPassword(ResetPasswordOtpViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var account = _context.Accounts.FirstOrDefault(a => a.Username == model.Username);
            if (account == null || account.ResetOtpCode == null || account.ResetOtpExpiryUtc == null)
            {
                ModelState.AddModelError("", "Phiên khôi phục không hợp lệ. Vui lòng yêu cầu mã OTP mới.");
                return View(model);
            }
            if (account.ResetOtpExpiryUtc < DateTime.UtcNow)
            {
                ModelState.AddModelError("", "Mã OTP đã hết hạn. Vui lòng yêu cầu mã mới.");
                return View(model);
            }
            if (account.ResetOtpCode != model.Otp.Trim())
            {
                ModelState.AddModelError("Otp", "Mã OTP không chính xác.");
                return View(model);
            }

            var hasher = new PasswordHasher<Account>();
            account.PasswordHash = hasher.HashPassword(account, model.NewPassword);
            account.ResetOtpCode = null;
            account.ResetOtpExpiryUtc = null;
            _context.SaveChanges();

            TempData["Success"] = "Đặt lại mật khẩu thành công! Vui lòng đăng nhập lại.";
            return RedirectToAction("Login");
        }

        // Che bớt email khi hiển thị cho người dùng, VD: ng*****inh@gmail.com
        private static string MaskEmail(string email)
        {
            var atIndex = email.IndexOf('@');
            if (atIndex <= 1) return email;

            var name = email[..atIndex];
            var domain = email[atIndex..];
            var visible = Math.Min(2, name.Length);
            return name[..visible] + new string('*', Math.Max(name.Length - visible, 3)) + domain;
        }
    }
}
