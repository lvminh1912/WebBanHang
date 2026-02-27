using BusinessLayers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims; 

namespace SV22T1020239.Shop.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly GioHangService _gioHangService;

        public CartController(GioHangService gioHangService)
        {
            _gioHangService = gioHangService;
        }

        private int GetCurrentUserId()
        {
            var claimId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(claimId, out int id) ? id : 0;
        }

        public async Task<IActionResult> Index()
        {
            int userId = GetCurrentUserId();
            var gioHang = await _gioHangService.GetGioHangByKhachHangID(userId);
            return View(gioHang);
        }

        public async Task<IActionResult> Add(int id, int quantity = 1)
        {
            int userId = GetCurrentUserId();
            if (userId == 0) return RedirectToAction("Login", "User");
            await _gioHangService.AddToCart(userId, id, quantity);
            return Redirect(Request.Headers["Referer"].ToString());
        }

        public async Task<IActionResult> Remove(int id)
        {
            int userId = GetCurrentUserId();
            await _gioHangService.RemoveItem(userId, id);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Update(int id, int quantity)
        {
            int userId = GetCurrentUserId();
            if (quantity <= 0)
                await _gioHangService.RemoveItem(userId, id);
            else
                await _gioHangService.UpdateQuantity(userId, id, quantity);

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Clear()
        {
            int userId = GetCurrentUserId();
            await _gioHangService.ClearCart(userId);
            return RedirectToAction("Index");
        }

    }
}