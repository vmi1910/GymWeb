using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GymWeb.Models;

namespace GymWeb.Controllers
{
    // Quản lý nhân viên (Admin) - chỉ hiển thị các tài khoản có Role = "Staff"
    public class StaffController : Controller
    {
        private readonly DataContext _context;
        public StaffController(DataContext context) { _context = context; }

        private bool IsAdmin() => HttpContext.Session.GetString("Role") == "Admin";

        public IActionResult Index(string? search)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var query = _context.Staffs.Where(s => s.Role == "Staff");

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(s => s.FullName.Contains(search) || s.PhoneNumber.Contains(search));
            }

            ViewData["Search"] = search;
            var list = query.OrderBy(s => s.FullName).ToList();
            return View(list);
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var staff = _context.Staffs.Find(id);
            if (staff == null) return NotFound();
            return View(staff);
        }

        [HttpPost]
        public IActionResult Edit(Staff model)
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
            staff.Salary = model.Salary;
            staff.Status = model.Status;

            _context.SaveChanges();
            TempData["Success"] = "Cập nhật thông tin nhân viên thành công!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var staff = _context.Staffs.Find(id);
            if (staff != null)
            {
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
                }
                catch (DbUpdateException)
                {
                    TempData["Error"] = "Không thể xóa nhân viên này vì đã có hóa đơn thanh toán gắn với tài khoản.";
                }
            }
            return RedirectToAction("Index");
        }
    }
}
