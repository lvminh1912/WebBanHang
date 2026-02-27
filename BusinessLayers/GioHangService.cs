using DataAccessTool;
using Microsoft.EntityFrameworkCore;
using Models;

namespace BusinessLayers
{
    public class GioHangService
    {
        private readonly ApplicationDbContext _context;

        public GioHangService(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. Lấy giỏ hàng của khách (Kèm chi tiết sản phẩm)
        public async Task<GioHang> GetGioHangByKhachHangID(int maKhachHang)
        {
            var gioHang = await _context.GioHangs
                .Include(g => g.GioHangChiTiets)
                .ThenInclude(ct => ct.MatHang) // Join để lấy tên, ảnh, giá
                .FirstOrDefaultAsync(g => g.MaKhachHang == maKhachHang);

            if (gioHang == null)
            {
                // Nếu chưa có giỏ thì tạo mới (ảo) để trả về list rỗng, chưa lưu DB vội
                gioHang = new GioHang { MaKhachHang = maKhachHang, GioHangChiTiets = new List<GioHangChiTiet>() };
            }
            return gioHang;
        }

        // 2. Thêm vào giỏ (Hoặc cập nhật số lượng nếu đã có)
        public async Task AddToCart(int maKhachHang, int maMatHang, int soLuong)
        {
            // Tìm giỏ hàng của khách
            var gioHang = await _context.GioHangs.FirstOrDefaultAsync(g => g.MaKhachHang == maKhachHang);

            if (gioHang == null)
            {
                gioHang = new GioHang
                {
                    MaKhachHang = maKhachHang,
                    // Các trường khác nếu bắt buộc thì gán default
                };
                _context.GioHangs.Add(gioHang);
                await _context.SaveChangesAsync(); // Lưu để lấy MaGioHang
            }

            // Tìm chi tiết sản phẩm trong giỏ
            var chiTiet = await _context.GioHangChiTiets
                .FirstOrDefaultAsync(c => c.MaGioHang == gioHang.MaGioHang && c.MaMatHang == maMatHang);

            if (chiTiet != null)
            {
                // Đã có -> Cộng dồn số lượng
                chiTiet.SoLuong += soLuong;
            }
            else
            {
                // Chưa có -> Thêm dòng mới
                chiTiet = new GioHangChiTiet
                {
                    MaGioHang = gioHang.MaGioHang,
                    MaMatHang = maMatHang,
                    SoLuong = soLuong
                };
                _context.GioHangChiTiets.Add(chiTiet);
            }

            await _context.SaveChangesAsync();
        }

        // 3. Xóa 1 sản phẩm khỏi giỏ
        public async Task RemoveItem(int maKhachHang, int maMatHang)
        {
            var gioHang = await _context.GioHangs.FirstOrDefaultAsync(g => g.MaKhachHang == maKhachHang);
            if (gioHang != null)
            {
                var chiTiet = await _context.GioHangChiTiets
                    .FirstOrDefaultAsync(c => c.MaGioHang == gioHang.MaGioHang && c.MaMatHang == maMatHang);

                if (chiTiet != null)
                {
                    _context.GioHangChiTiets.Remove(chiTiet);
                    await _context.SaveChangesAsync();
                }
            }
        }

        // 4. Cập nhật số lượng
        public async Task UpdateQuantity(int maKhachHang, int maMatHang, int soLuongMoi)
        {
            var gioHang = await _context.GioHangs.FirstOrDefaultAsync(g => g.MaKhachHang == maKhachHang);
            if (gioHang != null && soLuongMoi > 0)
            {
                var chiTiet = await _context.GioHangChiTiets
                    .FirstOrDefaultAsync(c => c.MaGioHang == gioHang.MaGioHang && c.MaMatHang == maMatHang);

                if (chiTiet != null)
                {
                    chiTiet.SoLuong = soLuongMoi;
                    await _context.SaveChangesAsync();
                }
            }
        }

        // 5. Xóa sạch giỏ hàng (Khi đặt hàng xong hoặc bấm nút xóa hết)
        public async Task ClearCart(int maKhachHang)
        {
            var gioHang = await _context.GioHangs.Include(g => g.GioHangChiTiets)
                                         .FirstOrDefaultAsync(g => g.MaKhachHang == maKhachHang);
            if (gioHang != null)
            {
                _context.GioHangChiTiets.RemoveRange(gioHang.GioHangChiTiets);
                await _context.SaveChangesAsync();
            }
        }
    }
}