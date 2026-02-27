using BusinessLayers;
using BusinessLayers.Shared;
using DataAccessTool;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Models;
using Models.SearchView;

namespace SV22T1020239.Admin.Controllers
{
    public class KhachHangController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly KhachHangService _khachHangService;
        private const string SessionKey = "KhachHang_SearchState";
        private async Task LoadTinhThanhList(int? selectedId = null)
        {
            var tinhThanhs = await _context.TinhThanhs.OrderBy(t => t.TenTinh).ToListAsync();
            ViewBag.MaTinh = new SelectList(tinhThanhs, "MaTinh", "TenTinh", selectedId);
        }

        public KhachHangController(ApplicationDbContext context, KhachHangService service)
        {
            _context = context;
            _khachHangService = service;
        }

        public async Task<IActionResult> Index(KhachHangSearchModel search)
        {
            bool isSearchAction = Request.Query.ContainsKey("keyword") || Request.Query.ContainsKey("diaChi");

            if (isSearchAction)
            {
                HttpContext.Session.SetObject(SessionKey, search);
            }
            else
            {
                var savedSearch = HttpContext.Session.GetObject<KhachHangSearchModel>(SessionKey);
                if (savedSearch != null) search = savedSearch;
            }

            var model = await _khachHangService.GetPagedListAsync(search);
            ViewBag.SearchModel = search;

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return PartialView("List", model);

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await LoadTinhThanhList();
            return View("Edit", new KhachHang());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(KhachHang model, IFormFile? uploadImage)
        {
            ModelState.Remove("HinhAnh");
            ModelState.Remove("uploadImage");

            if (ModelState.IsValid)
            {
                if (uploadImage != null && uploadImage.Length > 0)
                {
                    model.HinhAnh = await _khachHangService.SaveImageAsync(uploadImage);
                }
                _context.Add(model);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            await LoadTinhThanhList(model.MaTinh);
            return View("Edit", model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var khachHang = await _context.KhachHangs.FindAsync(id);
            if (khachHang == null) return NotFound();
            await LoadTinhThanhList(khachHang.MaTinh);
            return View(khachHang);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, KhachHang model, IFormFile? uploadImage)
        {
            if (id != model.MaKhachHang) return NotFound();

            ModelState.Remove("HinhAnh");
            ModelState.Remove("uploadImage");

            if (ModelState.IsValid)
            {
                // Xử lý lưu ảnh nếu người dùng chọn file mới
                if (uploadImage != null && uploadImage.Length > 0)
                {
                    model.HinhAnh = await _khachHangService.SaveImageAsync(uploadImage);
                }
                // Nếu không chọn ảnh mới, HinhAnh sẽ giữ nguyên giá trị từ input hidden gửi lên

                _context.Update(model);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        // --- Delete (AJAX) ---
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var kh = await _context.KhachHangs.FindAsync(id);
            if (kh == null) return Json(new { success = false, message = "Không tìm thấy!" });

            // Kiểm tra ràng buộc: Nếu khách đã có đơn hàng thì không cho xóa
            // if (_context.DonHangs.Any(d => d.MaKhachHang == id)) ... 

            _context.KhachHangs.Remove(kh);
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

    }
}