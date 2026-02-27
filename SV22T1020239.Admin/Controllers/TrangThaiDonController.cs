using BusinessLayers;
using DataAccessTool;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models;
using Models.SearchView;
using BusinessLayers.Shared;

namespace SV22T1020239.Admin.Controllers
{
    [Authorize(Roles = "admin")]
    public class TrangThaiDonController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly TrangThaiDonService _service;
        private const string SessionKey = "TrangThaiDon_SearchState";

        public TrangThaiDonController(ApplicationDbContext context, TrangThaiDonService service)
        {
            _context = context;
            _service = service;
        }

        public async Task<IActionResult> Index(TrangThaiDonSearchModel search)
        {
            bool isSearchAction = Request.Query.ContainsKey("keyword") || Request.Query.ContainsKey("page");

            if (isSearchAction)
            {
                HttpContext.Session.SetObject(SessionKey, search);
            }
            else
            {
                var savedSearch = HttpContext.Session.GetObject<TrangThaiDonSearchModel>(SessionKey);
                if (savedSearch != null) search = savedSearch;
            }

            var model = await _service.GetPagedListAsync(search);
            ViewBag.SearchModel = search;

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return PartialView("List", model);

            return View(model);
        }

        public async Task<IActionResult> Edit(int id = 0)
        {
            if (id == 0) return View(new TrangThaiDon()); // Tạo mới

            var item = await _context.TrangThaiDons.FindAsync(id);
            if (item == null) return NotFound();

            return View(item); // Sửa
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(TrangThaiDon model)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra xem đây là Thêm mới hay Sửa dựa vào việc ID đã tồn tại chưa
                var exists = await _context.TrangThaiDons.AsNoTracking().AnyAsync(x => x.MaTrangThai == model.MaTrangThai);

                if (exists)
                {
                    // Logic Sửa (Update)
                    _context.Update(model);
                }
                else
                {
                    // Logic Thêm mới (Insert)
                    // Vì ID không tự tăng nên ta Add bình thường với ID người dùng nhập
                    _context.Add(model);
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            // Không cho xóa các trạng thái hệ thống quan trọng (1-5) để tránh lỗi logic
            if (id <= 5)
            {
                return Json(new { success = false, message = "Không thể xóa các trạng thái mặc định của hệ thống!" });
            }

            var item = await _context.TrangThaiDons.FindAsync(id);
            if (item == null) return Json(new { success = false, message = "Không tìm thấy!" });

            // Kiểm tra ràng buộc: Nếu đã có đơn hàng dùng trạng thái này thì không cho xóa
            bool isUsed = _context.DonHangs.Any(d => d.MaTrangThai == id);
            if (isUsed)
                return Json(new { success = false, message = "Trạng thái này đang được sử dụng trong đơn hàng, không thể xóa!" });

            _context.TrangThaiDons.Remove(item);
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }
    }
}