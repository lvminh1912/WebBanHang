using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BusinessLayers;
using DataAccessTool;
using Models;
using System.Security.Claims;
using Models.SearchView;

namespace SV22T1020239.Shop.Controllers
{
    [Authorize] // Bắt buộc đăng nhập
    public class OrderController : Controller
    {
        private readonly DonHangService _donHangService;
        private readonly ApplicationDbContext _context;

        public OrderController(DonHangService donHangService, ApplicationDbContext context)
        {
            _donHangService = donHangService;
            _context = context;
        }

        // --- HÀM PHỤ TRỢ: Lấy ID người dùng an toàn ---
        private int GetCurrentUserId()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userIdStr) && int.TryParse(userIdStr, out int userId))
            {
                return userId;
            }
            return 0;
        }

        // GET: /Order/Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            // 1. Lấy ID khách hàng an toàn
            int userId = GetCurrentUserId();
            if (userId <= 0) return RedirectToAction("Login", "User"); // Hoặc Account

            // 2. Lấy Giỏ hàng
            var gioHang = await _context.GioHangs
                                        .Include(g => g.GioHangChiTiets)
                                        .ThenInclude(ct => ct.MatHang)
                                        .FirstOrDefaultAsync(g => g.MaKhachHang == userId);

            // Kiểm tra null cho giỏ hàng và chi tiết giỏ hàng
            if (gioHang == null || gioHang.GioHangChiTiets == null || !gioHang.GioHangChiTiets.Any())
            {
                TempData["Message"] = "Giỏ hàng của bạn đang trống.";
                return RedirectToAction("Index", "Cart");
            }

            // 3. Lấy thông tin khách hàng (Xử lý trường hợp có thể null)
            var khachHang = await _context.KhachHangs.FindAsync(userId);
            if (khachHang == null)
            {
                // Trường hợp hy hữu: Có Cookie đăng nhập nhưng DB không còn User này
                return RedirectToAction("Logout", "User");
            }

            // 4. Chuẩn bị dữ liệu cho View
            var model = new OrderCreateViewModel
            {
                TenKhachHang = khachHang.HoTen ?? "", // Xử lý null
                SoDienThoai = khachHang.DienThoai ?? "",
                DiaChiGiaoHang = khachHang.DiaChi ?? "",
                MaTinh = khachHang.MaTinh ?? 0,

                // Chuyển đổi an toàn danh sách
                Items = gioHang.GioHangChiTiets.ToList(),
                TongTien = gioHang.GioHangChiTiets
                    .Where(x => x.MatHang != null)
                    .Sum(x => x.SoLuong * x.MatHang!.GiaBan)
            };

            // 5. Lấy danh sách Tỉnh/Thành
            ViewBag.ListTinhThanh = await _context.TinhThanhs.ToListAsync();

            return View(model);
        }

        // POST: /Order/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(OrderCreateViewModel model)
        {
            int userId = GetCurrentUserId();
            if (userId <= 0) return RedirectToAction("Login", "User");

            // Gọi Service xử lý đặt hàng
            // Lưu ý: Nếu DiaChiGiaoHang null, hãy gán chuỗi rỗng để tránh lỗi SQL
            string diaChi = model.DiaChiGiaoHang ?? "";

            int orderId = await _donHangService.CheckoutFromCartAsync(userId, diaChi, model.MaTinh);

            // -- THÀNH CÔNG --
            if (orderId > 0)
            {
                return RedirectToAction("Success", new { id = orderId });
            }

            // -- THẤT BẠI (Xử lý lỗi) --
            if (orderId == -1) ModelState.AddModelError("", "Có sản phẩm đã ngừng kinh doanh.");
            else if (orderId == -2) ModelState.AddModelError("", "Kho không đủ số lượng hàng.");
            else ModelState.AddModelError("", "Lỗi hệ thống không xác định.");

            // -- RELOAD DỮ LIỆU ĐỂ HIỆN LẠI FORM --
            ViewBag.ListTinhThanh = await _context.TinhThanhs.ToListAsync();

            var gioHang = await _context.GioHangs
                                       .Include(g => g.GioHangChiTiets)
                                       .ThenInclude(ct => ct.MatHang)
                                       .FirstOrDefaultAsync(g => g.MaKhachHang == userId);

            // Tái tạo lại model để không bị null view
            model.Items = gioHang?.GioHangChiTiets.ToList() ?? new List<GioHangChiTiet>();
            model.TongTien = model.Items
                .Where(x => x.MatHang != null)
                .Sum(x => x.SoLuong * x.MatHang!.GiaBan);

            // Lấy lại tên khách hàng để hiển thị (tránh bị trống tên khi reload trang lỗi)
            var khachHang = await _context.KhachHangs.FindAsync(userId);
            model.TenKhachHang = khachHang?.HoTen ?? "Khách hàng";
            model.SoDienThoai = khachHang?.DienThoai ?? "";

            return View(model);
        }

        // GET: /Order/Detail/5
        public async Task<IActionResult> Detail(int id)
        {
            // 1. Lấy chi tiết đơn hàng (đã có hàm này trong DonHangService)
            var donHang = await _donHangService.GetDetailAsync(id);

            // 2. Kiểm tra tính hợp lệ (Đơn hàng tồn tại và thuộc về đúng khách hàng đang đăng nhập)
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (donHang == null || userIdStr == null || donHang.MaKhachHang.ToString() != userIdStr)
            {
                return NotFound(); // Tránh việc khách hàng xem trộm đơn hàng của người khác
            }

            return View(donHang);
        }

        // POST: /Order/Cancel/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdStr, out int userId))
            {
                bool result = await _donHangService.CancelOrderAsync(id, userId);
                if (result)
                {
                    TempData["Success"] = "Đã hủy đơn hàng thành công.";
                }
                else
                {
                    TempData["Error"] = "Không thể hủy đơn hàng này.";
                }
            }

            return RedirectToAction("Detail", new { id = id });
        }

        // GET: /Order/Success
        public IActionResult Success(int id)
        {
            return View(id);
        }
    }
}
