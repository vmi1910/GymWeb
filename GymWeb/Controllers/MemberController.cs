using Microsoft.AspNetCore.Mvc;
using GymWeb.Models;

namespace GymWeb.Controllers
{
    // Quản lý hội viên (Admin)
    public class MemberController : Controller
    {
        private readonly DataContext _context;
        public MemberController(DataContext context) { _context = context; }

        private bool IsAdmin() => HttpContext.Session.GetString("Role") == "Admin";

        public IActionResult Index(string? search)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var query = _context.Members.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(m => m.FullName.Contains(search) || m.PhoneNumber.Contains(search) || m.Email.Contains(search));
            }

            ViewData["Search"] = search;
            var list = query.OrderBy(m => m.FullName).ToList();
            return View(list);
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var member = _context.Members.Find(id);
            if (member == null) return NotFound();
            return View(member);
        }

        [HttpPost]
        public IActionResult Edit(Member model)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid) return View(model);

            var member = _context.Members.Find(model.MemberID);
            if (member == null) return NotFound();

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
