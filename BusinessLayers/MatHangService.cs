using BusinessLayers.Shared;
using DataAccessTool;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Models;
using Models.SearchView;

namespace BusinessLayers
{
    public class MatHangService
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private const int PageSize = 10;

        public MatHangService(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<string> SaveImageAsync(IFormFile image)
        {
            string fileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(image.FileName);
            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "MatHang");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }
            string filePath = Path.Combine(uploadsFolder, fileName);
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await image.CopyToAsync(fileStream);
            }
            return fileName;
        }

        public async Task<PaginatedList<MatHang>> GetPagedListAsync(MatHangSearchModel search, bool onlyActive = false)
        {
            // 1. Khởi tạo Query và Include
            var query = _context.MatHangs.Include(m => m.LoaiHang).AsQueryable();

            // 2. Áp dụng các bộ lọc (Filter)
            if (onlyActive)
            {
                query = query.Where(m => m.DangBan == true);
            }

            if (!string.IsNullOrEmpty(search.SearchName))
                query = query.Where(m => m.TenMatHang.Contains(search.SearchName));

            if (search.MaLoai.HasValue && search.MaLoai.Value > 0)
                query = query.Where(m => m.MaLoaiHang == search.MaLoai);

            if (search.GiaMin.HasValue)
                query = query.Where(m => m.GiaBan >= search.GiaMin);

            if (search.GiaMax.HasValue)
                query = query.Where(m => m.GiaBan <= search.GiaMax);

            // 3. Xử lý Sắp xếp (Sorting)
            switch (search.SortOrder)
            {
                case "price_asc":
                    query = query.OrderBy(m => m.GiaBan); // Giá thấp -> cao
                    break;
                case "price_desc":
                    query = query.OrderByDescending(m => m.GiaBan); // Giá cao -> thấp
                    break;
                case "name_asc":
                    query = query.OrderBy(m => m.TenMatHang); // Tên A -> Z
                    break;
                case "name_desc":
                    query = query.OrderByDescending(m => m.TenMatHang); // Tên Z -> A
                    break;
                default:
                    query = query.OrderByDescending(m => m.MaMatHang); // Mặc định: Mới nhất
                    break;
            }

            // 4. Đếm tổng số dòng (dựa trên bộ lọc)
            int count = await query.CountAsync();

            // 5. Xử lý Phân trang (Pagination)
            if (search.PageSize <= 0) search.PageSize = 12;

            var items = await query
                .Skip((search.Page - 1) * search.PageSize)
                .Take(search.PageSize)
                .ToListAsync();

            return new PaginatedList<MatHang>(items, count, search.Page, search.PageSize);
        }

        // Thêm hàm lấy chi tiết cho Shop Detail
        public async Task<MatHang?> GetByIdAsync(int id)
        {
            return await _context.MatHangs
                                 .Include(m => m.LoaiHang)
                                 .FirstOrDefaultAsync(m => m.MaMatHang == id);
        }
    }
}