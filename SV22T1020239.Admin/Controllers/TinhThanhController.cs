using BusinessLayers;
using BusinessLayers.Shared;
using DataAccessTool;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models;
using Models.SearchView;

namespace SV22T1020239.Admin.Controllers
{
    [Authorize(Roles = "admin")]
    public class TinhThanhController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly TinhThanhService _service;
        private const string SessionKey = "TinhThanh_SearchState";

        public TinhThanhController(ApplicationDbContext context, TinhThanhService service)
        {
            _context = context;
            _service = service;
        }

        public async Task<IActionResult> Index(TinhThanhSearchModel search)
        {
            // Kiểm tra xem người dùng có đang thực hiện tìm kiếm hoặc chuyển trang không
            bool isSearchAction = Request.Query.ContainsKey("keyword") || Request.Query.ContainsKey("page");

            if (isSearchAction)
            {
                // Lưu trạng thái tìm kiếm vào Session
                HttpContext.Session.SetObject(SessionKey, search);
            }
            else
            {
                // Khôi phục trạng thái cũ từ Session khi quay lại trang
                var savedSearch = HttpContext.Session.GetObject<TinhThanhSearchModel>(SessionKey);
                if (savedSearch != null) search = savedSearch;
            }

            var model = await _service.GetPagedListAsync(search);
            ViewBag.SearchModel = search;

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return PartialView("List", model);

            return View(model);
        }
        // GET: TinhThanh/Create
        public IActionResult Create()
        {
            return View("Edit", new TinhThanh());
        }

        // POST: TinhThanh/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TinhThanh model)
        {
            if (ModelState.IsValid)
            {
                bool exists = await _context.TinhThanhs.AnyAsync(x => x.MaTinh == model.MaTinh);
                if (exists)
                {
                    ModelState.AddModelError("MaTinh", "Mã tỉnh này đã tồn tại, vui lòng nhập mã khác.");
                    return View("Edit", model);
                }

                _context.Add(model);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View("Edit", model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            // Nếu không có ID thì báo lỗi
            if (id == 0) return NotFound();

            var item = await _context.TinhThanhs.FindAsync(id);
            if (item == null) return NotFound();

            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(TinhThanh model)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra ID đã tồn tại chưa
                var exists = await _context.TinhThanhs.AsNoTracking().AnyAsync(x => x.MaTinh == model.MaTinh);

                if (exists)
                {
                    _context.Update(model);
                }
                else
                {
                    // Nếu ID chưa có -> Thêm mới
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
            var item = await _context.TinhThanhs.FindAsync(id);
            if (item == null) return Json(new { success = false, message = "Không tìm thấy!" });

            // 1. Kiểm tra bên Khách Hàng
            bool hasCustomer = await _context.KhachHangs.AnyAsync(k => k.MaTinh == id);
            if (hasCustomer)
                return Json(new { success = false, message = "Không thể xóa: Có khách hàng thuộc tỉnh/thành này!" });

            // 2. Kiểm tra bên Đơn Hàng (Địa chỉ giao hàng)
            bool hasOrder = await _context.DonHangs.AnyAsync(d => d.MaTinh == id);
            if (hasOrder)
                return Json(new { success = false, message = "Không thể xóa: Có đơn hàng giao tới tỉnh/thành này!" });

            _context.TinhThanhs.Remove(item);
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }
    }
}