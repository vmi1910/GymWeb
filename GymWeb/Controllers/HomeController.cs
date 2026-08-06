using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using GymWeb.Models;
using GymWeb.ViewModels;

namespace GymWeb.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly DataContext _context;

    public HomeController(ILogger<HomeController> logger, DataContext context)
    {
        _logger = logger;
        _context = context;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    // Trang công khai "Bộ môn tập luyện" - hiển thị các bộ môn + đội ngũ HLV thật đang hoạt động
    public IActionResult Programs()
    {
        var trainers = (from s in _context.Staffs
                         where s.Role == "Trainer" && s.Status == "Active"
                         join tp in _context.TrainerProfiles on s.StaffID equals tp.StaffID into tpj
                         from tp in tpj.DefaultIfEmpty()
                         select new TrainerViewModel
                         {
                             StaffID = s.StaffID,
                             FullName = s.FullName,
                             Specialty = tp != null ? tp.Specialty : "",
                             Certification = tp != null ? tp.Certification : "",
                             CertificateImagePath = tp != null ? tp.CertificateImagePath : null
                         })
                        .OrderBy(t => t.FullName)
                        .ToList();

        return View(trainers);
    }

    // Trang công khai "Hệ thống phòng tập" - hiển thị phòng tập thật (dữ liệu Admin quản lý) + chi nhánh
    public IActionResult Facilities()
    {
        var rooms = _context.Rooms.OrderBy(r => r.Floor).ThenBy(r => r.RoomName).ToList();
        return View(rooms);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
