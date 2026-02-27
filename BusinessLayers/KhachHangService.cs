using BusinessLayers.Shared;
using DataAccessTool;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Models;
using Models.SearchView;

namespace BusinessLayers
{
    public class KhachHangService
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private const int PageSize = 10;

        public KhachHangService(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }
        public async Task<string> SaveImageAsync(IFormFile image)
        {
            string fileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(image.FileName);

            string rootPath = _webHostEnvironment.WebRootPath;

            if (_webHostEnvironment.ApplicationName.Contains(".Shop"))
            {
                rootPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "SV22T1020239.Admin", "wwwroot");
            }

            string uploadsFolder = Path.Combine(rootPath, "images", "KhachHang");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            string filePath = Path.Combine(uploadsFolder, fileName);
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await image.CopyToAsync(fileStream);
            }

            // LUÔN trả về định dạng chuỗi có dấu gạch chéo để đồng bộ DB
            return $"/images/KhachHang/{fileName}";
        }
        public async Task<PaginatedList<KhachHang>> GetPagedListAsync(KhachHangSearchModel search)
        {
            var query = _context.KhachHangs
                        .Include(k => k.TinhThanh)
                        .AsQueryable();

            if (!string.IsNullOrEmpty(search.Keyword))
            {
                query = query.Where(k => k.HoTen.Contains(search.Keyword)
                                      || (k.DienThoai != null && k.DienThoai.Contains(search.Keyword)));
            }

            if (!string.IsNullOrEmpty(search.DiaChi))
            {
                query = query.Where(k => k.DiaChi != null && k.DiaChi.Contains(search.DiaChi));
            }

            int count = await query.CountAsync();

            var items = await query
                .OrderByDescending(k => k.MaKhachHang)
                .Skip((search.Page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            return new PaginatedList<KhachHang>(items, count, search.Page, PageSize);
        }
        // 1. Hàm đăng ký tài khoản (Thêm khách hàng mới)
        public async Task<int> Register(KhachHang data)
        {
            // Kiểm tra Email đã tồn tại chưa
            bool exists = await _context.KhachHangs.AnyAsync(x => x.Email == data.Email);
            if (exists) return -1; // Mã lỗi: Email đã tồn tại

            // Thêm mới
            _context.KhachHangs.Add(data);
            await _context.SaveChangesAsync();
            return data.MaKhachHang;
        }

        // 2. Hàm kiểm tra đăng nhập
        public async Task<KhachHang?> Login(string email, string password)
        {
            var user = await _context.KhachHangs.FirstOrDefaultAsync(x => x.Email == email);

            // Lưu ý: Trong thực tế bạn nên mã hóa mật khẩu (MD5/SHA256/BCrypt)
            // Ở đây mình so sánh trực tiếp để demo (giả sử DB lưu plain text)
            if (user != null && user.MatKhau == password)
            {
                return user;
            }
            return null;
        }

        // 3. Hàm đổi mật khẩu
        public async Task<bool> ChangePassword(int userId, string oldPass, string newPass)
        {
            var user = await _context.KhachHangs.FindAsync(userId);
            if (user == null) return false;

            if (user.MatKhau == oldPass) // Lưu ý: Nên mã hóa MD5/SHA256 trong thực tế
            {
                user.MatKhau = newPass;
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }
    }
}