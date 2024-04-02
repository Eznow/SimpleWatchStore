using BaiTapLon.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace BaiTapLon.Controllers
{
    public class UsersController : Controller
    {
        private readonly WatchStoreContext _context;

        public UsersController(WatchStoreContext context)
        {
            _context = context;
        }

        // GET: Users/Register
        public IActionResult Register()
        {
            // Kiểm tra nếu người dùng đã đăng nhập thì chuyển hướng đến trang Watches/Index
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Watches");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("/Users/Register")]
        public async Task<IActionResult> Register([Bind("Username, Password")] User user)
        {
            if (ModelState.IsValid)
            {
                // Check if the data already exists in the database
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == user.Username);
                if (existingUser != null)
                {
                    Console.WriteLine("Đăng ký thất bại: Tên tài khoản đã tồn tại.");
                    return BadRequest("Tên tài khoản đã tồn tại. Vui lòng đặt lại tên khác"); // Trả về BadRequest nếu tên tài khoản đã tồn tại
                }

                user.Role = "Customer";

                _context.Add(user);
                try
                {
                    await _context.SaveChangesAsync();
                    Console.WriteLine("Đăng ký thành công");
                    return Ok("Đăng ký thành công."); // Trả về Ok nếu đăng ký thành công
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return StatusCode(500, "Đã xảy ra lỗi khi thực hiện đăng ký."); // Trả về StatusCode 500 nếu có lỗi xảy ra
                }
            }
            else
            {
                Console.WriteLine("Thêm thất bại");
                return BadRequest("Thêm thất bại."); // Trả về BadRequest nếu dữ liệu không hợp lệ
            }

    }

    // GET: Users/Login
    public IActionResult Login()
        {
            // Kiểm tra nếu người dùng đã đăng nhập thì chuyển hướng đến trang Watches/Index
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Watches");
            }
            return View();
        }

        // POST: Users/Login
        [HttpPost]
        public async Task<IActionResult> Login(User user)
        {
            var dbUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == user.Username && u.Password == user.Password);
            if (dbUser != null)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, dbUser.Username),
                    new Claim(ClaimTypes.Role, dbUser.Role)
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                // Lưu thông tin người dùng vào Session
                HttpContext.Session.SetString("Username", dbUser.Username);

                return RedirectToAction("Index", "Watches"); // Chuyển hướng đến trang Watches/Index
            }
            return View(user);
        }


        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // GET: Users/Logout
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Index), "Home");
        }
    }
}
