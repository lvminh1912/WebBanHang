using DataAccessTool;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models.SearchView;
using SV22T1020239.Admin.Models;
using System.Diagnostics;

namespace SV22T1020239.Admin.Controllers
{
    [Authorize(Roles = "admin")]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // 1. Lấy số liệu thống kê
            var tongDoanhThu = await _context.DonHangs
                .Include(d => d.TrangThaiDon) // Cần include để check tên trạng thái
                .Where(d => d.TrangThaiDon != null && d.TrangThaiDon.MaTrangThai != 5)
                .SumAsync(d => d.TongTien);

            var viewModel = new DashboardViewModel
            {
                TongDonHang = await _context.DonHangs.CountAsync(),
                TongSanPham = await _context.MatHangs.CountAsync(),
                TongKhachHang = await _context.KhachHangs.CountAsync(),
                DoanhThu = tongDoanhThu ?? 0
            };

            // 2. Lấy 5 đơn hàng mới nhất
            viewModel.DonHangMoiNhat = await _context.DonHangs
                .Include(d => d.KhachHang)
                .Include(d => d.TrangThaiDon)
                .OrderByDescending(d => d.NgayDat)
                .Take(5)
                .ToListAsync();

            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}