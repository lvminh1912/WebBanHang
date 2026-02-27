using BusinessLayers.Shared;
using DataAccessTool;
using Microsoft.EntityFrameworkCore;
using Models;
using Models.SearchView;

namespace BusinessLayers
{
    public class TinhThanhService
    {
        private readonly ApplicationDbContext _context;
        private const int PageSize = 20;

        public TinhThanhService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PaginatedList<TinhThanh>> GetPagedListAsync(TinhThanhSearchModel search)
        {
            var query = _context.TinhThanhs.AsQueryable();

            if (!string.IsNullOrEmpty(search.Keyword))
            {
                query = query.Where(x => x.TenTinh != null && x.TenTinh.Contains(search.Keyword));
            }

            int count = await query.CountAsync();

            var items = await query
                .Skip((search.Page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            return new PaginatedList<TinhThanh>(items, count, search.Page, PageSize);
        }
    }
}