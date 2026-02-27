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
    public class DonHangController : Controller
    {
        private readonly DonHangService _service;
        private readonly ApplicationDbContext _context;
        private const string SessionKey = "DonHang_SearchState";

        public DonHangController(DonHangService service, ApplicationDbContext context)
        {
            _service = service;
            _context = context;
        }

        public async Task<IActionResult> Index(DonHangSearchModel search)
        {
            // 1. LOGIC SESSION
            // Kiểm tra xem có tham số nào trên URL không (tức là người dùng vừa bấm Tìm kiếm/Phân trang)
            bool isSearchAction = Request.Query.ContainsKey("keyword")
                               || Request.Query.ContainsKey("maTrangThai")
                               || Request.Query.ContainsKey("tuNgay")
                               || Request.Query.ContainsKey("denNgay")
                               || Request.Query.ContainsKey("page");

            if (isSearchAction)
            {
                // Nếu là hành động tìm kiếm mới -> Lưu vào Session
                HttpContext.Session.SetObject(SessionKey, search);
            }
            else
            {
                // Nếu vào trang tự nhiên (Menu/Back) -> Khôi phục từ Session
                var savedSearch = HttpContext.Session.GetObject<DonHangSearchModel>(SessionKey);
                if (savedSearch != null)
                {
                    search = savedSearch;
                }
            }

            // 2. LOGIC LẤY DỮ LIỆU
            ViewBag.TrangThais = await _context.TrangThaiDons.ToListAsync();

            var model = await _service.GetPagedListAsync(search);

            // Truyền search model ra View để điền lại vào các ô input
            ViewBag.SearchModel = search;

            // Hỗ trợ AJAX (nếu bạn dùng hàm loadPage như các trang kia)
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("List", model); // Trả về file List.cshtml vừa tạo
            }
            return View(model); // Trả về file Index.cshtml (lần đầu truy cập)
        }

        // Xem chi tiết đơn hàng
        public async Task<IActionResult> Details(int id)
        {
            var donHang = await _service.GetDetailAsync(id);
            if (donHang == null) return NotFound();

            // Lấy danh sách trạng thái để Admin có thể đổi trạng thái ngay tại trang chi tiết
            ViewBag.TrangThais = new SelectList(await _context.TrangThaiDons.ToListAsync(), "MaTrangThai", "TenTrangThai", donHang.MaTrangThai);

            return View(donHang);
        }

        // Cập nhật trạng thái (Dùng cho nút bấm hoặc dropdown thay đổi nhanh)
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, int maTrangThai)
        {
            // Gọi Service để thực hiện logic cập nhật trạng thái và ngày giờ
            bool result = await _service.UpdateStatusAsync(id, maTrangThai);

            if (!result)
            {
                // Nếu lỗi, có thể thông báo (hoặc đơn giản là load lại trang)
                TempData["Error"] = "Cập nhật trạng thái thất bại!";
            }
            else
            {
                TempData["Success"] = "Cập nhật trạng thái thành công!";
            }

            // Quay lại trang Chi tiết để thấy ngày giờ vừa cập nhật
            return RedirectToAction(nameof(Details), new { id = id });
        }
        // Hàm hỗ trợ load dữ liệu cho Dropdown
        private async Task LoadViewBags()
        {
            ViewBag.KhachHangs = await _context.KhachHangs.Select(k => new { k.MaKhachHang, ThongTin = $"{k.HoTen} - {k.DienThoai}" }).ToListAsync();
            ViewBag.TrangThais = await _context.TrangThaiDons.ToListAsync();

            // Load danh sách sản phẩm kèm giá để JS sử dụng
            var matHangs = await _context.MatHangs.Select(m => new { m.MaMatHang, m.TenMatHang, m.GiaBan, m.HinhAnh }).ToListAsync();
            ViewBag.MatHangs = matHangs;
            ViewBag.TinhThanhs = await _context.TinhThanhs.ToListAsync(); // Nếu có bảng Tỉnh
        }

        // GET: Create
        public async Task<IActionResult> Create()
        {
            await LoadViewBags();
            var model = new DonHang { NgayDat = DateTime.Now, MaTrangThai = 1 }; // Mặc định trạng thái Mới
            return View("Edit", model); // Dùng chung View Edit
        }

        // POST: Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DonHang model)
        {
            // Bỏ qua lỗi validation của các bảng liên kết
            ModelState.Remove("KhachHang");
            ModelState.Remove("TrangThaiDon");
            ModelState.Remove("TinhThanh");

            if (ModelState.IsValid)
            {
                await _service.CreateAsync(model);
                return RedirectToAction(nameof(Index));
            }

            await LoadViewBags();
            return View("Edit", model);
        }

        // GET: Edit
        public async Task<IActionResult> Edit(int id)
        {
            // Lấy đơn hàng kèm chi tiết để hiển thị lại
            var model = await _service.GetDetailAsync(id);
            if (model == null) return NotFound();

            await LoadViewBags();
            return View(model);
        }

        // POST: Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DonHang model)
        {
            if (id != model.MaDonHang) return NotFound();

            ModelState.Remove("KhachHang");
            ModelState.Remove("TrangThaiDon");
            ModelState.Remove("TinhThanh");

            if (ModelState.IsValid)
            {
                await _service.UpdateAsync(model);
                return RedirectToAction(nameof(Index));
            }

            await LoadViewBags();
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> SearchKhachHang(string? term)
        {
            // Khởi tạo query
            var query = _context.KhachHangs.AsQueryable();

            if (!string.IsNullOrWhiteSpace(term))
            {
                // Nếu có từ khóa -> Lọc theo Tên hoặc SĐT
                term = term.Trim();
                query = query.Where(k => k.HoTen.Contains(term) || (k.DienThoai != null && k.DienThoai.Contains(term)));
            }

            // Nếu term rỗng -> Query sẽ lấy tất cả (nhưng bị giới hạn Take(20) ở dưới)

            var data = await query
                .OrderByDescending(k => k.MaKhachHang) // Sắp xếp: Khách mới nhất hiện lên trước
                .Select(k => new {
                    id = k.MaKhachHang,
                    text = $"{k.HoTen} - {k.DienThoai ?? "Không có SĐT"}"
                })
                .Take(20) // Luôn giới hạn 20 kết quả để tối ưu tốc độ
                .ToListAsync();

            return Json(new { results = data });
        }

        [HttpGet]
        public async Task<IActionResult> SearchMatHang(string? term)
        {
            var query = _context.MatHangs.AsQueryable();

            if (!string.IsNullOrWhiteSpace(term))
            {
                term = term.Trim();
                query = query.Where(m => m.TenMatHang.Contains(term));
            }

            var data = await query
                .OrderBy(m => m.TenMatHang) // Sắp xếp A-Z
                .Select(m => new {
                    id = m.MaMatHang,
                    text = m.TenMatHang,
                    price = m.GiaBan,
                    image = m.HinhAnh ?? ""
                })
                .Take(20)
                .ToListAsync();

            return Json(new { results = data });
        }
    }
}