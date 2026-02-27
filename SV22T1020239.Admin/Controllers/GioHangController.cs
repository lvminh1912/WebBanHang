using DataAccessTool;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models.SearchView;

namespace SV22T1020239.Admin.Controllers
{
    [Authorize(Roles = "admin")]
    public class GioHangController : Controller
    {
        private readonly ApplicationDbContext _context;

        public GioHangController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: GioHang/Index
        public async Task<IActionResult> Index(int maKhachHang)
        {
            // 1. Lấy thông tin khách hàng
            var khachHang = await _context.KhachHangs.FindAsync(maKhachHang);
            if (khachHang == null) return NotFound("Khách hàng không tồn tại");

            // 2. Lấy dữ liệu giỏ hàng từ CSDL [PHẦN CẦN SỬA]
            // Logic: Tìm chi tiết giỏ hàng -> Join với bảng Mặt Hàng -> Lọc theo Mã Khách Hàng
            var cartItems = await _context.GioHangChiTiets
                .Include(ghct => ghct.MatHang)    // Kèm thông tin Mặt hàng để lấy Tên, Giá, Ảnh
                .Include(ghct => ghct.GioHang)    // Kèm thông tin Giỏ hàng để lọc theo Khách
                .Where(ghct => ghct.GioHang != null && ghct.GioHang.MaKhachHang == maKhachHang && ghct.MatHang != null)
                .Select(ghct => new GioHangItemViewModel
                {
                    MaMatHang = ghct.MaMatHang,
                    TenMatHang = ghct.MatHang != null ? ghct.MatHang.TenMatHang : string.Empty,
                    HinhAnh = ghct.MatHang != null ? ghct.MatHang.HinhAnh ?? string.Empty : string.Empty,
                    DonGia = ghct.MatHang != null ? ghct.MatHang.GiaBan : 0,
                    SoLuong = ghct.SoLuong,
                    // ThanhTien được tính tự động trong ViewModel dựa trên DonGia * SoLuong
                })
                .ToListAsync();

            // 3. Đưa dữ liệu vào ViewModel
            var viewModel = new GioHangViewModel
            {
                KhachHang = khachHang,
                Items = cartItems
            };

            return View(viewModel);
        }
    }
}