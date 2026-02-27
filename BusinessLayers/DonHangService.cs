using BusinessLayers.Shared;
using DataAccessTool;
using Microsoft.EntityFrameworkCore;
using Models;
using Models.SearchView;

namespace BusinessLayers
{
    public class DonHangService
    {
        private readonly ApplicationDbContext _context;
        private const int PageSize = 10;

        public DonHangService(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================================================================
        // PHẦN 1: CÁC CHỨC NĂNG DÀNH CHO ADMIN (QUẢN LÝ ĐƠN HÀNG)
        // =========================================================================

        /// <summary>
        /// Lấy danh sách đơn hàng có phân trang và tìm kiếm
        /// </summary>
        public async Task<PaginatedList<DonHang>> GetPagedListAsync(DonHangSearchModel search)
        {
            var query = _context.DonHangs
                .Include(d => d.KhachHang)
                .Include(d => d.TrangThaiDon)
                .AsNoTracking() // Tối ưu: Không theo dõi thay đổi vì chỉ để hiển thị
                .AsQueryable();

            if (!string.IsNullOrEmpty(search.Keyword))
            {
                // Tìm theo Tên khách hoặc Mã đơn hàng
                // Lưu ý: Kiểm tra d.KhachHang != null trước khi truy cập HoTen
                query = query.Where(d => (d.KhachHang != null && d.KhachHang.HoTen.Contains(search.Keyword))
                                      || d.MaDonHang.ToString() == search.Keyword);
            }

            if (search.MaTrangThai.HasValue && search.MaTrangThai > 0)
            {
                query = query.Where(d => d.MaTrangThai == search.MaTrangThai);
            }

            if (search.TuNgay.HasValue)
            {
                query = query.Where(d => d.NgayDat >= search.TuNgay.Value);
            }

            if (search.DenNgay.HasValue)
            {
                // Thêm 1 ngày để lấy trọn vẹn ngày kết thúc (tính đến 23:59:59 của ngày đó)
                query = query.Where(d => d.NgayDat < search.DenNgay.Value.AddDays(1));
            }

            int count = await query.CountAsync();

            var items = await query
                .OrderByDescending(d => d.NgayDat) // Đơn mới nhất lên đầu
                .Skip((search.Page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            return new PaginatedList<DonHang>(items, count, search.Page, PageSize);
        }

        /// <summary>
        /// Lấy chi tiết một đơn hàng cụ thể
        /// </summary>
        public async Task<DonHang?> GetDetailAsync(int id)
        {
            return await _context.DonHangs
                .Include(d => d.KhachHang)
                .Include(d => d.TrangThaiDon)
                .Include(d => d.TinhThanh)
                .Include(d => d.DonHangChiTiets)
                    .ThenInclude(ct => ct.MatHang)
                .AsNoTracking() // Tối ưu đọc dữ liệu
                .FirstOrDefaultAsync(m => m.MaDonHang == id);
        }

        /// <summary>
        /// Cập nhật trạng thái đơn hàng (Duyệt, Giao hàng, Hủy...)
        /// </summary>
        public async Task<bool> UpdateStatusAsync(int maDonHang, int maTrangThaiMoi)
        {
            var donHang = await _context.DonHangs.FindAsync(maDonHang);
            if (donHang == null) return false;

            // 1. Cập nhật trạng thái mới
            donHang.MaTrangThai = maTrangThaiMoi;

            // 2. Cập nhật ngày tương ứng
            DateTime currentTime = DateTime.Now;

            switch (maTrangThaiMoi)
            {
                case 3: // Đang giao hàng
                    donHang.NgayDi = currentTime;
                    break;

                case 4: // Giao thành công
                    donHang.NgayDen = currentTime;
                    if (donHang.NgayDi == null) donHang.NgayDi = currentTime;
                    break;

                case 5: // Đã hủy
                    donHang.NgayHuy = currentTime;
                    break;

                    // Các trạng thái khác (1, 2) có thể reset ngày nếu cần thiết
            }

            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Tạo đơn hàng thủ công (Admin tạo thay khách)
        /// </summary>
        public async Task<int> CreateAsync(DonHang model)
        {
            // 1. Tính tổng tiền từ chi tiết đơn hàng
            model.TongTien = 0;
            if (model.DonHangChiTiets != null)
            {
                foreach (var item in model.DonHangChiTiets)
                {
                    var matHang = await _context.MatHangs.FindAsync(item.MaMatHang);
                    if (matHang != null)
                    {
                        item.DonGia = matHang.GiaBan;
                        model.TongTien += item.DonGia * item.SoLuong;
                    }
                }
            }

            // 2. Gán ngày tạo nếu chưa có
            if (model.NgayDat == null) model.NgayDat = DateTime.Now;

            // Đảm bảo không insert ID rác
            if (model.MaDonHang != 0) model.MaDonHang = 0;

            _context.DonHangs.Add(model);
            await _context.SaveChangesAsync();
            return model.MaDonHang;
        }

        /// <summary>
        /// Cập nhật thông tin đơn hàng (Admin sửa địa chỉ, thêm bớt sản phẩm...)
        /// </summary>
        public async Task<bool> UpdateAsync(DonHang model)
        {
            // Phải Include chi tiết để xóa và thêm lại
            var existingOrder = await _context.DonHangs
                                              .Include(d => d.DonHangChiTiets)
                                              .FirstOrDefaultAsync(d => d.MaDonHang == model.MaDonHang);

            if (existingOrder == null) return false;

            // 1. Cập nhật thông tin chung
            existingOrder.MaKhachHang = model.MaKhachHang;
            existingOrder.MaTrangThai = model.MaTrangThai;
            existingOrder.NgayDat = model.NgayDat;
            existingOrder.NgayDi = model.NgayDi;
            existingOrder.NgayDen = model.NgayDen;
            existingOrder.NgayHuy = model.NgayHuy;
            existingOrder.DiaChiGiaoHang = model.DiaChiGiaoHang;
            existingOrder.MaTinh = model.MaTinh;

            // 2. Xử lý Chi tiết đơn hàng
            // Xóa hết chi tiết cũ
            if (existingOrder.DonHangChiTiets != null)
            {
                _context.DonHangChiTiets.RemoveRange(existingOrder.DonHangChiTiets);
            }

            // Thêm chi tiết mới
            decimal tongTien = 0;
            if (model.DonHangChiTiets != null)
            {
                foreach (var item in model.DonHangChiTiets)
                {
                    var matHang = await _context.MatHangs.FindAsync(item.MaMatHang);
                    if (matHang != null)
                    {
                        var chiTietMoi = new DonHangChiTiet
                        {
                            MaDonHang = existingOrder.MaDonHang,
                            MaMatHang = item.MaMatHang,
                            SoLuong = item.SoLuong,
                            DonGia = matHang.GiaBan // Lấy giá mới nhất từ DB
                        };
                        tongTien += chiTietMoi.DonGia * chiTietMoi.SoLuong;
                        _context.DonHangChiTiets.Add(chiTietMoi);
                    }
                }
            }

            existingOrder.TongTien = tongTien;

            await _context.SaveChangesAsync();
            return true;
        }

        // =========================================================================
        // PHẦN 2: CÁC CHỨC NĂNG DÀNH CHO KHÁCH HÀNG (CLIENT)
        // =========================================================================

        /// <summary>
        /// Lấy lịch sử mua hàng của khách
        /// </summary>
        public async Task<List<DonHang>> GetListByKhachHangID(int maKhachHang, int trangThai = 0)
        {
            var query = _context.DonHangs
                                .Include(d => d.TrangThaiDon)
                                .AsNoTracking() // Tối ưu
                                .Where(d => d.MaKhachHang == maKhachHang);

            if (trangThai > 0)
            {
                query = query.Where(d => d.MaTrangThai == trangThai);
            }

            return await query.OrderByDescending(d => d.NgayDat).ToListAsync();
        }

        /// <summary>
        /// Xử lý đặt hàng (Checkout) từ Giỏ hàng
        /// </summary>
        public async Task<int> CheckoutFromCartAsync(int maKhachHang, string diaChiGiaoHang, int maTinh)
        {
            // BƯỚC 1: Lấy dữ liệu Giỏ hàng
            var gioHang = await _context.GioHangs
                .Include(g => g.GioHangChiTiets)
                .ThenInclude(ct => ct.MatHang)
                .FirstOrDefaultAsync(g => g.MaKhachHang == maKhachHang);

            // Kiểm tra null an toàn
            if (gioHang == null || gioHang.GioHangChiTiets == null || !gioHang.GioHangChiTiets.Any())
            {
                return 0; // Giỏ hàng rỗng
            }

            // BƯỚC 2: Kiểm tra tồn kho
            foreach (var item in gioHang.GioHangChiTiets)
            {
                // Kiểm tra null cho MatHang (dù đã include nhưng vẫn cần check để tránh warning)
                if (item.MatHang == null) continue;

                // Kiểm tra ngừng kinh doanh
                if (item.MatHang.DangBan == false) return -1;

                // Kiểm tra số lượng tồn
                if (item.MatHang.SoLuong < item.SoLuong) return -2;
            }

            // BƯỚC 3: Tạo Đơn Hàng & Transaction
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    // 3.1. Tạo Header đơn hàng
                    var donHangMoi = new DonHang
                    {
                        MaKhachHang = maKhachHang,
                        NgayDat = DateTime.Now,
                        MaTrangThai = 1, // Chờ xác nhận
                        DiaChiGiaoHang = diaChiGiaoHang ?? "", // Xử lý null
                        MaTinh = maTinh,
                        TongTien = 0
                    };

                    _context.DonHangs.Add(donHangMoi);
                    await _context.SaveChangesAsync(); // Lưu để lấy ID

                    // 3.2. Tạo Chi tiết đơn & Trừ kho
                    decimal tongTien = 0;

                    foreach (var item in gioHang.GioHangChiTiets)
                    {
                        if (item.MatHang == null) continue;

                        var chiTietDon = new DonHangChiTiet
                        {
                            MaDonHang = donHangMoi.MaDonHang,
                            MaMatHang = item.MaMatHang,
                            SoLuong = item.SoLuong,
                            DonGia = item.MatHang.GiaBan
                        };

                        _context.DonHangChiTiets.Add(chiTietDon);
                        tongTien += (chiTietDon.DonGia * chiTietDon.SoLuong);

                        // TRỪ TỒN KHO: Cần lấy đối tượng MatHang đang được Tracking bởi EF Context để update
                        // Lưu ý: item.MatHang ở trên lấy từ GioHang (có thể Tracking), 
                        // nhưng an toàn nhất là Find lại hoặc dùng trực tiếp item.MatHang nếu nó đang được track.
                        // Ở đây ta dùng item.MatHang vì nó được load từ context chung.
                        item.MatHang.SoLuong -= item.SoLuong;
                    }

                    donHangMoi.TongTien = tongTien;

                    // 3.3. Xóa Giỏ hàng
                    _context.GioHangChiTiets.RemoveRange(gioHang.GioHangChiTiets);

                    // Lưu tất cả thay đổi (Đơn chi tiết, Update tồn kho, Xóa giỏ)
                    await _context.SaveChangesAsync();

                    // Commit Transaction
                    await transaction.CommitAsync();

                    return donHangMoi.MaDonHang;
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    return 0; // Lỗi hệ thống
                }
            }
        }
        /// <summary>
        /// Xử lý hủy đơn hàng bởi khách hàng
        /// </summary>
        public async Task<bool> CancelOrderAsync(int maDonHang, int maKhachHang)
        {
            // Tìm đơn hàng của đúng khách hàng đó
            var donHang = await _context.DonHangs
                .Include(d => d.DonHangChiTiets)
                .FirstOrDefaultAsync(d => d.MaDonHang == maDonHang && d.MaKhachHang == maKhachHang);

            // Logic mới: Chỉ không cho hủy nếu đơn đã Thành công (4) hoặc đã Hủy (5) rồi
            if (donHang == null || donHang.MaTrangThai == 4 || donHang.MaTrangThai == 5)
                return false;

            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    // 1. Cập nhật trạng thái thành 5 (Đã hủy)
                    donHang.MaTrangThai = 5;
                    donHang.NgayHuy = DateTime.Now;

                    // 2. HOÀN TRẢ KHO: Cộng lại số lượng vào bảng MatHang
                    if (donHang.DonHangChiTiets != null)
                    {
                        foreach (var item in donHang.DonHangChiTiets)
                        {
                            var matHang = await _context.MatHangs.FindAsync(item.MaMatHang);
                            if (matHang != null)
                            {
                                matHang.SoLuong += item.SoLuong; // Cộng trả lại kho
                            }
                        }
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return true;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    return false;
                }
            }
        }
    }
}