using Microsoft.AspNetCore.Mvc;
using GymWeb.Models;
using Microsoft.AspNetCore.Identity;
using GymWeb.ViewModels;
using GymWeb.Helpers;
using Microsoft.EntityFrameworkCore;

namespace GymWeb.Controllers
{
    public class AdminController : Controller
    {
        private readonly DataContext _context;
        private readonly IWebHostEnvironment _env;
        public AdminController(DataContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
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
            var accounts = _context.Accounts.ToList();
            var members = _context.Members.ToList();
            var staffs = _context.Staffs.ToList();

            var list = new List<AccountListItemViewModel>();
            foreach (var acc in accounts)
            {
                var item = new AccountListItemViewModel
                {
                    AccountID = acc.AccountID,
                    Username = acc.Username,
                    Role = acc.Role
                };

                if (acc.Role == "Member")
                {
                    var member = members.FirstOrDefault(m => m.AccountID == acc.AccountID);
                    if (member != null)
                    {
                        item.EditController = "Member";
                        item.EditId = member.MemberID;
                    }
                }
                else
                {
                    // Staff, Trainer, Admin đều lưu hồ sơ trong bảng Staff
                    var staff = staffs.FirstOrDefault(s => s.AccountID == acc.AccountID);
                    if (staff != null)
                    {
                        item.EditController = acc.Role == "Trainer" ? "Trainer" : "Staff";
                        item.EditId = staff.StaffID;
                    }
                }

                list.Add(item);
            }

            return View(list);
        }

        //Tạo tài khoản (Admin)
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
            }

            // Tạo mới bắt buộc phải có mật khẩu (Edit thì không, nên không đặt [Required] ở Model)
            if (string.IsNullOrWhiteSpace(model.RawPassword))
            {
                ModelState.AddModelError("RawPassword", "Mật khẩu không được để trống");
            }

            // Xử lý ảnh chứng chỉ TRƯỚC khi ghi bất kỳ dữ liệu nào xuống DB,
            // để nếu ảnh không hợp lệ thì không tạo tài khoản "dở dang".
            string? certificateImagePath = null;
            if (model.Role == "Trainer" && model.CertificateImage != null)
            {
                certificateImagePath = FileUploadHelper.SaveImage(_env, model.CertificateImage, "certificates", out var imageError);
                if (imageError != null)
                {
                    ModelState.AddModelError("CertificateImage", imageError);
                }
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
                    _context.SaveChanges();
                }
                else
                {
                    // Staff, Trainer, Admin đều lưu vào bảng STAFF, phân biệt qua cột Role
                    var staff = new Staff
                    {
                        FullName = model.FullName,
                        PhoneNumber = model.PhoneNumber,
                        Email = model.Email,
                        Gender = model.Gender,
                        Role = model.Role, // Staff / Trainer / Admin
                        AccountID = account.AccountID
                    };
                    _context.Staffs.Add(staff);
                    _context.SaveChanges(); // Lưu để lấy StaffID trước khi tạo hồ sơ PT (nếu có)

                    if (model.Role == "Trainer")
                    {
                        var trainerProfile = new TrainerProfile
                        {
                            StaffID = staff.StaffID,
                            Specialty = model.Specialty ?? string.Empty,
                            Certification = model.Certification ?? string.Empty,
                            CertificateImagePath = certificateImagePath
                        };
                        _context.TrainerProfiles.Add(trainerProfile);
                        _context.SaveChanges();
                    }
                }

                return RedirectToAction("Accounts");
            }

            return View(model);
        }
        // GET: Hiển thị form sửa tài khoản
        [HttpGet]
        public IActionResult Edit(int id)
        {
            if (HttpContext.Session.GetString("Role") != "Admin") return RedirectToAction("Login", "Account");

            var account = _context.Accounts.Find(id);
            if (account == null) return NotFound();

            var member = _context.Members.FirstOrDefault(m => m.AccountID == id);
            var staff = _context.Staffs.FirstOrDefault(s => s.AccountID == id);

            var model = new AccountCreateViewModel
            {
                Username = account.Username,
                Role = account.Role,
                FullName = member?.FullName ?? staff?.FullName ?? "",
                PhoneNumber = member?.PhoneNumber ?? staff?.PhoneNumber ?? "",
                Email = member?.Email ?? staff?.Email ?? "",
                Gender = member?.Gender ?? staff?.Gender ?? "Nam"
            };

            ViewBag.AccountID = id; // dùng để post lại đúng ID
            return View(model);
        }

        // POST: Lưu thông tin đã sửa
        [HttpPost]
        public IActionResult Edit(int id, AccountCreateViewModel model)
        {
            if (HttpContext.Session.GetString("Role") != "Admin") return RedirectToAction("Login", "Account");

            var account = _context.Accounts.Find(id);
            if (account == null) return NotFound();

            // Kiểm tra trùng username với tài khoản khác
            if (_context.Accounts.Any(a => a.Username == model.Username && a.AccountID != id))
            {
                ModelState.AddModelError("Username", "Tên đăng nhập này đã tồn tại!");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.AccountID = id;
                return View(model);
            }

            account.Username = model.Username;

            // Chỉ đổi mật khẩu nếu người dùng có nhập mới
            if (!string.IsNullOrWhiteSpace(model.RawPassword))
            {
                var hasher = new PasswordHasher<Account>();
                account.PasswordHash = hasher.HashPassword(account, model.RawPassword);
            }

            var member = _context.Members.FirstOrDefault(m => m.AccountID == id);
            var staff = _context.Staffs.FirstOrDefault(s => s.AccountID == id);

            if (member != null)
            {
                member.FullName = model.FullName;
                member.PhoneNumber = model.PhoneNumber;
                member.Email = model.Email;
                member.Gender = model.Gender;
            }
            else if (staff != null)
            {
                staff.FullName = model.FullName;
                staff.PhoneNumber = model.PhoneNumber;
                staff.Email = model.Email;
                staff.Gender = model.Gender;
            }

            _context.SaveChanges();
            return RedirectToAction("Accounts");
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            if (HttpContext.Session.GetString("Role") != "Admin") return RedirectToAction("Login", "Account");

            var account = _context.Accounts.Find(id);
            if (account != null)
            {
                try
                {
                    _context.Accounts.Remove(account);
                    _context.SaveChanges();
                }
                catch (DbUpdateException)
                {
                    // Xảy ra khi tài khoản này là Staff/Trainer đã từng thu ngân một hóa đơn (Payment.StaffID)
                    // -> ràng buộc Restrict trên Payment.Staff chặn việc xóa dây chuyền.
                    TempData["Error"] = "Không thể xóa tài khoản này vì đã có hóa đơn thanh toán gắn với tài khoản.";
                }
            }
            return RedirectToAction("Accounts");
        }
    }
}