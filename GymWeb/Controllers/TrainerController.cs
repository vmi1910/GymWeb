using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GymWeb.Models;
using GymWeb.ViewModels;
using GymWeb.Helpers;

namespace GymWeb.Controllers
{
    // Quản lý huấn luyện viên (Admin) - các tài khoản có Role = "Trainer"
    public class TrainerController : Controller
    {
        private readonly DataContext _context;
        private readonly IWebHostEnvironment _env;
        public TrainerController(DataContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        private bool IsAdmin() => HttpContext.Session.GetString("Role") == "Admin";

        public IActionResult Index(string? search)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var query = from s in _context.Staffs
                        where s.Role == "Trainer"
                        join tp in _context.TrainerProfiles on s.StaffID equals tp.StaffID into tpj
                        from tp in tpj.DefaultIfEmpty()
                        select new TrainerViewModel
                        {
                            StaffID = s.StaffID,
                            FullName = s.FullName,
                            DateOfBirth = s.DateOfBirth,
                            Gender = s.Gender,
                            PhoneNumber = s.PhoneNumber,
                            Email = s.Email,
                            Address = s.Address,
                            Status = s.Status,
                            Specialty = tp != null ? tp.Specialty : "",
                            Certification = tp != null ? tp.Certification : "",
                            CertificateImagePath = tp != null ? tp.CertificateImagePath : null
                        };

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(t => t.FullName.Contains(search) || t.PhoneNumber.Contains(search) || t.Specialty.Contains(search));
            }

            ViewData["Search"] = search;
            var list = query.OrderBy(t => t.FullName).ToList();
            return View(list);
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var staff = _context.Staffs.Find(id);
            if (staff == null || staff.Role != "Trainer") return NotFound();

            var profile = _context.TrainerProfiles.Find(id);

            var model = new TrainerViewModel
            {
                StaffID = staff.StaffID,
                FullName = staff.FullName,
                DateOfBirth = staff.DateOfBirth,
                Gender = staff.Gender,
                PhoneNumber = staff.PhoneNumber,
                Email = staff.Email,
                Address = staff.Address,
                Status = staff.Status,
                Specialty = profile?.Specialty ?? "",
                Certification = profile?.Certification ?? "",
                CertificateImagePath = profile?.CertificateImagePath
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult Edit(TrainerViewModel model)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid) return View(model);

            var staff = _context.Staffs.Find(model.StaffID);
            if (staff == null) return NotFound();

            staff.FullName = model.FullName;
            staff.DateOfBirth = model.DateOfBirth;
            staff.Gender = model.Gender;
            staff.PhoneNumber = model.PhoneNumber;
            staff.Email = model.Email;
            staff.Address = model.Address;
            staff.Status = model.Status;

            var profile = _context.TrainerProfiles.Find(model.StaffID);
            if (profile == null)
            {
                profile = new TrainerProfile { StaffID = model.StaffID };
                _context.TrainerProfiles.Add(profile);
            }
            profile.Specialty = model.Specialty;
            profile.Certification = model.Certification;

            // Nếu admin chọn ảnh mới thì thay thế, không thì giữ nguyên ảnh cũ
            if (model.CertificateImage != null)
            {
                var newPath = FileUploadHelper.SaveImage(_env, model.CertificateImage, "certificates", out var imageError);
                if (imageError != null)
                {
                    ModelState.AddModelError("CertificateImage", imageError);
                    model.CertificateImagePath = profile.CertificateImagePath;
                    return View(model);
                }

                FileUploadHelper.DeleteIfExists(_env, profile.CertificateImagePath);
                profile.CertificateImagePath = newPath;
            }

            _context.SaveChanges();
            TempData["Success"] = "Cập nhật hồ sơ huấn luyện viên thành công!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var staff = _context.Staffs.Find(id);
            if (staff != null)
            {
                var imagePath = _context.TrainerProfiles.Find(id)?.CertificateImagePath;
                try
                {
                    var account = _context.Accounts.Find(staff.AccountID);
                    if (account != null)
                    {
                        _context.Accounts.Remove(account);
                    }
                    else
                    {
                        _context.Staffs.Remove(staff);
                    }
                    _context.SaveChanges();
                    FileUploadHelper.DeleteIfExists(_env, imagePath);
                }
                catch (DbUpdateException)
                {
                    TempData["Error"] = "Không thể xóa huấn luyện viên này vì đã có hóa đơn thanh toán gắn với tài khoản.";
                }
            }
            return RedirectToAction("Index");
        }
    }
}
