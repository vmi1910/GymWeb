using Microsoft.AspNetCore.Mvc;
using GymWeb.Models;
using Microsoft.AspNetCore.Identity;
using GymWeb.ViewModels;

namespace GymWeb.Controllers
{
    public class AdminController : Controller
    {
        private readonly DataContext _context;
        public AdminController(DataContext context)
        {
            _context = context;
        }

        // Trang quản lý danh sách tài khoản
        public IActionResult Accounts()
        {
            // BẢO MẬT: Nếu không phải Admin thì không cho vào, đá về trang Login
            if (HttpContext.Session.GetString("Role") != "Admin")
            {
                return RedirectToAction("Login", "Account");
            }

            // Lấy toàn bộ danh sách tài khoản từ database
            var listAccounts = _context.Accounts.ToList();
            return View(listAccounts);
        }

        //Tạo tài khoản (Admin)
        [HttpGet]
        public IActionResult Create()
        {
            if (HttpContext.Session.GetString("Role") != "Admin") return RedirectToAction("Login", "Account");
            return View(new AccountCreateViewModel());
        }

        // 2. Xử lý khi bấm nút Lưu (POST)
        [HttpPost]
        public IActionResult Create(AccountCreateViewModel model)
        {
            if (HttpContext.Session.GetString("Role") != "Admin") return RedirectToAction("Login", "Account");

            // Kiểm tra trùng Username
            if (_context.Accounts.Any(a => a.Username == model.Username))
            {
                ModelState.AddModelError("Username", "Tên đăng nhập này đã tồn tại!");
                return View(model);
            }

            if (ModelState.IsValid)
            {
                // 1. Lưu vào bảng ACCOUNT trước
                var account = new Account { Username = model.Username, Role = model.Role };
                var hasher = new PasswordHasher<Account>();
                account.PasswordHash = hasher.HashPassword(account, model.RawPassword);
        
                _context.Accounts.Add(account);
                _context.SaveChanges(); // Lưu để lấy được AccountID sinh ra tự động

                // 2. Dựa vào Role để lưu vào bảng MEMBER hoặc STAFF
                if (model.Role == "Member")
                {
                    var member = new Member 
                    { 
                        FullName = model.FullName, 
                        PhoneNumber = model.PhoneNumber, 
                        Email = model.Email, 
                        Gender = model.Gender,
                        AccountID = account.AccountID // Móc nối với tài khoản vừa tạo
                    };
                _context.Members.Add(member);
                }
                else if (model.Role == "Staff") // Thêm logic cho Staff
                {
                    var staff = new Staff 
                    { 
                        FullName = model.FullName, 
                        PhoneNumber = model.PhoneNumber, 
                        Email = model.Email, 
                        Gender = model.Gender,
                        Role = "Staff", // Gán role Staff
                        AccountID = account.AccountID
                    };
                    _context.Staffs.Add(staff);
                }
                else // Nếu là Admin
                {
                    var adminStaff = new Staff 
                    { 
                        FullName = model.FullName, 
                        PhoneNumber = model.PhoneNumber, 
                        Email = model.Email, 
                        Gender = model.Gender,
                        Role = "Admin", // Admin cũng được lưu vào bảng Staff hoặc tạo một bảng Admin riêng tùy bạn
                        AccountID = account.AccountID
                    };
                _context.Staffs.Add(adminStaff);
                }
                _context.SaveChanges(); // Lưu thông tin cá nhân
                return RedirectToAction("Accounts"); 
            }

            return View(model);
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            if (HttpContext.Session.GetString("Role") != "Admin") return RedirectToAction("Login", "Account");
    
            var account = _context.Accounts.Find(id);
            if (account != null)
            {
                _context.Accounts.Remove(account);
                _context.SaveChanges();
            }
            return RedirectToAction("Accounts");
        }
    }
}