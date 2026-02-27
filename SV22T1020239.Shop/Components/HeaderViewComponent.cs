using BusinessLayers;
using DataAccessTool;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models.SearchView;
using System.Security.Claims;

namespace SV22T1020239.Shop.ViewComponents
{
    public class HeaderViewComponent : ViewComponent
    {
        private readonly GioHangService _gioHangService;
        private readonly ApplicationDbContext _context;

        public HeaderViewComponent(GioHangService gioHangService, ApplicationDbContext context)
        {
            _gioHangService = gioHangService;
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var model = new HeaderViewModel();

            if (UserClaimsPrincipal != null && UserClaimsPrincipal.Identity!.IsAuthenticated)
            {
                var userIdStr = UserClaimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!string.IsNullOrEmpty(userIdStr) && int.TryParse(userIdStr, out int userId))
                {
                    // 1. Lấy thông tin User
                    var khachHang = await _context.KhachHangs
                        .AsNoTracking()
                        .FirstOrDefaultAsync(k => k.MaKhachHang == userId);

                    if (khachHang != null)
                    {
                        model.FullName = khachHang.HoTen ?? "Khách hàng";
                        if (!string.IsNullOrEmpty(khachHang.HinhAnh))
                        {
                            model.Avatar = khachHang.HinhAnh;
                        }
                    }

                    // 2. Lấy thông tin Giỏ hàng
                    var gioHang = await _gioHangService.GetGioHangByKhachHangID(userId);
                    if (gioHang != null && gioHang.GioHangChiTiets != null)
                    {
                        model.CartCount = gioHang.GioHangChiTiets.Sum(x => x.SoLuong);
                        model.CartTotal = gioHang.GioHangChiTiets.Sum(x => x.SoLuong * (x.MatHang?.GiaBan ?? 0));
                    }
                }
            }

            return View(model);
        }
    }
}