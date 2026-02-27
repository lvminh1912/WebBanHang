using BusinessLayers;
using BusinessLayers.Shared;
using DataAccessTool;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Models;
using Models.SearchView;

namespace SV22T1020239.Admin.Controllers
{
    [Authorize(Roles = "admin")]
    public class MatHangController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly MatHangService _matHangService;
        private const string SessionKey = "MatHang_SearchState";

        public MatHangController(ApplicationDbContext context, MatHangService matHangService)
        {
            _context = context;
            _matHangService = matHangService;
        }

        public async Task<IActionResult> Index(MatHangSearchModel search)
        {
            bool isSearchAction = Request.Query.ContainsKey("searchName");
            if (isSearchAction)
            {
                HttpContext.Session.SetObject(SessionKey, search);
            }
            else
            {
                var savedSearch = HttpContext.Session.GetObject<MatHangSearchModel>(SessionKey);
                if (savedSearch != null)
                {
                    search = savedSearch;
                }
            }

            // 2. Gọi Service
            var model = await _matHangService.GetPagedListAsync(search);

            // 3. Chuẩn bị dữ liệu hiển thị (Để điền lại vào các ô input)
            ViewBag.SearchModel = search;
            ViewBag.LoaiHangs = await _context.LoaiHangs.ToListAsync();

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("List", model);
            }

            return View(model);
        }

        // GET: MatHang/Create
        public async Task<IActionResult> Create()
        {
            var loaiHangs = await _context.LoaiHangs.ToListAsync();
            ViewBag.MaLoaiHang = new SelectList(loaiHangs, "MaLoaiHang", "TenLoaiHang");
            return View("Edit", new MatHang());
        }
        // POST: MatHang/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MatHang model, IFormFile? uploadImage)
        {
            if (ModelState.IsValid)
            {
                if (uploadImage != null && uploadImage.Length > 0)
                {
                    model.HinhAnh = await _matHangService.SaveImageAsync(uploadImage);
                }
                _context.Add(model);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.MaLoaiHang = new SelectList(await _context.LoaiHangs.ToListAsync(), "MaLoaiHang", "TenLoaiHang", model.MaLoaiHang);
            return View("Edit", model);
        }
        // GET: MatHang/Edit
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var matHang = await _context.MatHangs.FindAsync(id);
            if (matHang == null) return NotFound();
            ViewBag.MaLoaiHang = new SelectList(await _context.LoaiHangs.ToListAsync(), "MaLoaiHang", "TenLoaiHang", matHang.MaLoaiHang);
            return View(matHang);
        }
        // POST: MatHang/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, MatHang matHang, IFormFile? uploadImage)
        {
            if (id != matHang.MaMatHang) return NotFound();

            if (ModelState.IsValid)
            {
                if (uploadImage != null && uploadImage.Length > 0)
                {
                    matHang.HinhAnh = await _matHangService.SaveImageAsync(uploadImage);
                }
                _context.Update(matHang);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.MaLoaiHang = new SelectList(await _context.LoaiHangs.ToListAsync(), "MaLoaiHang", "TenLoaiHang", matHang.MaLoaiHang);
            return View(matHang);
        }
        // DELETE: MatHang/Delete
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var matHang = await _context.MatHangs.FindAsync(id);
            if (matHang == null)
            {
                return Json(new { success = false, message = "Không tìm thấy mặt hàng!" });
            }

            try
            {
                _context.MatHangs.Remove(matHang);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi khi xóa: " + ex.Message });
            }
        }
    }
}