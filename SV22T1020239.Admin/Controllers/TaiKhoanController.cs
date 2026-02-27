using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using DataAccessTool;
using Models;
using Microsoft.EntityFrameworkCore;

namespace SV22T1020239.Admin.Controllers
{
    public class TaiKhoanController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TaiKhoanController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: TaiKhoan/Login
        public IActionResult Login()
        {
            return View();
        }

        // POST: TaiKhoan/Login
        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            // 1. Kiểm tra thông tin đăng nhập từ CSDL
            var user = await _context.KhachHangs
                .FirstOrDefaultAsync(u => u.Email == email && u.MatKhau == password); // Lưu ý: Nên mã hóa mật khẩu trong thực tế

            if (user == null)
            {
                ViewBag.Error = "Email hoặc mật khẩu không đúng.";
                return View();
            }

            // 2. Kiểm tra vai trò (Chỉ Admin mới được vào trang này)
            if (user.VaiTro?.Trim().ToLower() != "admin")
            {
                ViewBag.Error = "Bạn không có quyền truy cập vào trang Admin.";
                return View();
            }

            // 3. Tạo Claims (Thông tin phiên đăng nhập)
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.HoTen),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, "admin"), // Quan trọng: Gán role admin
                new Claim("MaKhachHang", user.MaKhachHang.ToString())
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties { IsPersistent = true }; // Giữ đăng nhập khi đóng trình duyệt

            // 4. Ghi Cookie xác thực
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            return RedirectToAction("Index", "Home");
        }

        // GET: TaiKhoan/Logout
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        // GET: TaiKhoan/Register
        public IActionResult Register()
        {
            return View();
        }

        // POST: TaiKhoan/Register
        [HttpPost]
        public async Task<IActionResult> Register(KhachHang model)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra email trùng
                if (await _context.KhachHangs.AnyAsync(k => k.Email == model.Email))
                {
                    ModelState.AddModelError("Email", "Email này đã được sử dụng.");
                    return View(model);
                }

                // Mặc định đăng ký mới là "user". 
                model.VaiTro = "user";
                model.DangHoatDong = true;

                _context.Add(model);
                await _context.SaveChangesAsync();

                ViewBag.Success = "Đăng ký thành công! Vui lòng liên hệ Admin để cấp quyền truy cập.";
                return View();
            }
            return View(model);
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}