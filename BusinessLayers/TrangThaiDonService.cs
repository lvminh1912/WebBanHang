using BusinessLayers.Shared;
using DataAccessTool;
using Microsoft.EntityFrameworkCore;
using Models;
using Models.SearchView;

namespace BusinessLayers
{
    public class TrangThaiDonService
    {
        private readonly ApplicationDbContext _context;
        private const int PageSize = 20;

        public TrangThaiDonService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PaginatedList<TrangThaiDon>> GetPagedListAsync(TrangThaiDonSearchModel search)
        {
            var query = _context.TrangThaiDons.AsQueryable();

            if (!string.IsNullOrEmpty(search.Keyword))
            {
                query = query.Where(x => x.TenTrangThai.Contains(search.Keyword));
            }

            int count = await query.CountAsync();

            var items = await query
                .OrderBy(x => x.MaTrangThai) // Sắp xếp theo mã số để dễ nhìn quy trình
                .Skip((search.Page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            return new PaginatedList<TrangThaiDon>(items, count, search.Page, PageSize);
        }
    }
}