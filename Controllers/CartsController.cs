using BaiTapLon.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BaiTapLon.Controllers
{
    public class CartsController : Controller
    {
        private readonly WatchStoreContext _context;

        public CartsController(WatchStoreContext context)
        {
            _context = context;
        }

        // GET: Carts
        public async Task<IActionResult> Index()
        {
            var username = HttpContext.Session.GetString("Username");
            if (username == null)
            {
                // Xử lý trường hợp không có username
                TempData["ErrorMessage"] = "Bạn đã hết phiên đăng nhập. Vui lòng đăng nhập lại để xem giỏ hàng của mình.";
                return RedirectToAction("Logout", "Users");
            }
            var user = _context.Users.FirstOrDefault(u => u.Username == username);
            var cart = await _context.Carts.Include(c => c.CartItems).ThenInclude(ci => ci.Watch).FirstOrDefaultAsync(c => c.UserID == user.UserID && c.PurchaseDate == null);
            return View(cart);
        }


        // POST: Carts/AddToCart/5
        [HttpPost]
        public async Task<IActionResult> AddToCart(int id)
        {
            var username = HttpContext.Session.GetString("Username");
            var user = _context.Users.FirstOrDefault(u => u.Username == username);
            if (username == null || user == null)
            {
                return Json(new { success = false, message = "Bạn đã hết phiên đăng nhập. Vui lòng đăng nhập lại để xem giỏ hàng" +
                    " của mình.", redirectToUrl = Url.Action("Logout", "Users") });
            }
            var cart = await _context.Carts.Include(c => c.CartItems).FirstOrDefaultAsync(c => c.UserID == user.UserID && c.PurchaseDate == null);

            // Nếu cart là null, tạo một Cart mới
            if (cart == null)
            {
                cart = new Cart { UserID = user.UserID, CartItems = new List<CartItem>() };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            // Kiểm tra xem sản phẩm đã có trong giỏ hàng chưa
            var existingCartItem = cart.CartItems.FirstOrDefault(ci => ci.WatchID == id);
            if (existingCartItem != null)
            {
                // Nếu sản phẩm đã có trong giỏ hàng, hiển thị thông báo và không thực hiện thêm mới
                return Json(new { success = false, message = "Sản phẩm này đã có trong giỏ hàng." });
            }

            var cartItem = new CartItem { CartID = cart.CartID, WatchID = id, Quantity = 1 };
            _context.CartItems.Add(cartItem);
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Sản phẩm đã được thêm vào giỏ hàng" });
        }

        // POST: Carts/RemoveFromCart/5
        [HttpPost]
        public async Task<IActionResult> RemoveFromCart(int id)
        {
            var username = HttpContext.Session.GetString("Username");
            var user = _context.Users.FirstOrDefault(u => u.Username == username);
            var cart = await _context.Carts.Include(c => c.CartItems).FirstOrDefaultAsync(c => c.UserID == user.UserID && c.PurchaseDate == null);

            // Tìm mục giỏ hàng cần xóa
            var cartItem = cart.CartItems.FirstOrDefault(ci => ci.CartItemID == id);
            if (cartItem != null)
            {
                _context.CartItems.Remove(cartItem);
                await _context.SaveChangesAsync();
            }

            return Json(new { success = true, message = "Sản phẩm đã được xóa khỏi giỏ hàng" });
        }

        // POST: Carts/Buy
        [HttpPost]
        public async Task<IActionResult> Buy(int id, int quantity)
        {
            var username = HttpContext.Session.GetString("Username");
            var user = _context.Users.FirstOrDefault(u => u.Username == username);
            var cart = await _context.Carts.Include(c => c.CartItems).ThenInclude(ci => ci.Watch).FirstOrDefaultAsync(c => c.UserID == user.UserID && c.PurchaseDate == null);

            // Tìm mục giỏ hàng cần mua
            var cartItem = cart.CartItems.FirstOrDefault(ci => ci.CartItemID == id);
            if (cartItem != null)
            {
                if (cartItem.Watch.StockQuantity >= quantity)
                {
                    // Cập nhật số lượng hàng tồn kho
                    cartItem.Watch.StockQuantity -= quantity;
                    await _context.SaveChangesAsync();
                    return Json(new { success = true, message = "Bạn đã mua sản phẩm thành công" });
                }
                else
                {
                    return Json(new { success = false, message = "Số lượng hàng tồn kho không đủ" });
                }
            }

            return Json(new { success = true, message = "Bạn đã mua sản phẩm thành công" });
        }

    }



}
