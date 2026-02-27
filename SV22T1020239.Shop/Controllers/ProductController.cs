using BusinessLayers;
using BusinessLayers.Shared;
using DataAccessTool;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models.SearchView;

namespace SV22T1020239.Shop.Controllers
{
    public class ProductController : Controller
    {
        private readonly MatHangService _matHangService;
        private readonly ApplicationDbContext _context;

        private const string PRODUCT_SEARCH_SESSION = "Shop_Product_Search_Condition";

        public ProductController(MatHangService matHangService, ApplicationDbContext context)
        {
            _matHangService = matHangService;
            _context = context;
        }

        // GET: Dữ liệu ban đầu cho trang danh sách sản phẩm
        public IActionResult Index()
        {
            // 1. Lấy lại trạng thái tìm kiếm cũ từ Session (nếu có)
            var search = HttpContext.Session.GetObject<MatHangSearchModel>(PRODUCT_SEARCH_SESSION);

            // Nếu chưa có session (lần đầu vào), tạo mới với giá trị mặc định
            if (search == null)
            {
                search = new MatHangSearchModel
                {
                    Page = 1,
                    PageSize = 12,
                    SearchName = "",
                    MaLoai = 0,      
                    GiaMin = null,   
                    GiaMax = null,   
                    SortOrder = ""
                };
            }

            // 2. Lấy danh sách loại hàng để đổ vào Dropdown
            ViewBag.LoaiHangs = _context.LoaiHangs.ToList();

            // 3. Truyền SearchModel sang View để điền dữ liệu vào form (giữ trạng thái input)
            return View(search);
        }

        // GET: Lấy danh sách sản phẩm theo điều kiện tìm kiếm (dùng cho Ajax)
        public async Task<IActionResult> List(MatHangSearchModel search)
        {
            // 1. Kiểm tra logic phân trang
            if (search.PageSize <= 0) search.PageSize = 12;

            // 2. LƯU TOÀN BỘ ĐIỀU KIỆN TÌM KIẾM VÀO SESSION
            HttpContext.Session.SetObject(PRODUCT_SEARCH_SESSION, search);

            // 3. Gọi Service xử lý truy vấn (Đảm bảo Service của bạn đã viết logic lọc theo GiaMin/GiaMax)
            var model = await _matHangService.GetPagedListAsync(search, onlyActive: true);

            // 4. Trả về Partial View
            return PartialView("List", model);
        }

        // GET: Chi tiết sản phẩm
        public async Task<IActionResult> Detail(int id)
        {
            var matHang = await _matHangService.GetByIdAsync(id);
            if (matHang == null || matHang.DangBan == false)
            {
                return NotFound();
            }
            return View(matHang);
        }
    }
}