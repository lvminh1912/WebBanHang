using BusinessLayers;
using DataAccessTool;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using Models.SearchView;
using BusinessLayers.Shared;

namespace SV22T1020239.Admin.Controllers
{
    [Authorize(Roles = "admin")]
    public class LoaiHangController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly LoaiHangService _service;
        private const string SessionKey = "LoaiHang_SearchState";

        public LoaiHangController(ApplicationDbContext context, LoaiHangService service)
        {
            _context = context;
            _service = service;
        }

        public async Task<IActionResult> Index(LoaiHangSearchModel search)
        {
            bool isSearchAction = Request.Query.ContainsKey("keyword") || Request.Query.ContainsKey("page");

            if (isSearchAction)
            {
                HttpContext.Session.SetObject(SessionKey, search);
            }
            else
            {
                var savedSearch = HttpContext.Session.GetObject<LoaiHangSearchModel>(SessionKey);
                if (savedSearch != null) search = savedSearch;
            }

            var model = await _service.GetPagedListAsync(search);
            ViewBag.SearchModel = search;

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return PartialView("List", model);

            return View(model);
        }
        // GET: LoaiHang/Create
        public IActionResult Create()
        {
            return View("Edit", new LoaiHang());
        }

        // POST: LoaiHang/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LoaiHang model)
        {
            if (ModelState.IsValid)
            {
                _context.Add(model);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View("Edit", model);
        }
        // GET: LoaiHang/Edit
        public async Task<IActionResult> Edit(int id)
        {
            // Nếu id = 0 thì trả về lỗi (vì đã có trang Create riêng)
            if (id == 0) return NotFound();

            var item = await _context.LoaiHangs.FindAsync(id);
            if (item == null) return NotFound();

            return View(item);
        }

        // POST: LoaiHang/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(LoaiHang model)
        {
            if (ModelState.IsValid)
            {
                _context.Update(model);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.LoaiHangs.FindAsync(id);
            if (item == null) return Json(new { success = false, message = "Không tìm thấy!" });

            // Kiểm tra ràng buộc: Nếu loại hàng đang có sản phẩm thì không cho xóa
            bool hasProducts = _context.MatHangs.Any(m => m.MaLoaiHang == id);
            if (hasProducts)
                return Json(new { success = false, message = "Không thể xóa loại hàng này vì đang có sản phẩm!" });

            _context.LoaiHangs.Remove(item);
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }
    }
}