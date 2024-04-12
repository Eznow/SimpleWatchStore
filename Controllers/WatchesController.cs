using BaiTapLon.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileSystemGlobbing;

namespace BaiTapLon.Controllers
{
    public class WatchesController : Controller
    {
        private readonly WatchStoreContext _context;

        public WatchesController(WatchStoreContext context)
        {
            _context = context;
        }

        // GET: Watches
        public async Task<IActionResult> Index()
        {
            return View(await _context.Watches.Include(w => w.WatchStatus).ToListAsync());
        }

        // GET: Watches/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var watch = await _context.Watches
                .Include(w => w.WatchStatus)
                .Include(w => w.Reviews)
                .ThenInclude(r => r.User)
                .Include(w => w.Ratings)
                .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(m => m.WatchID == id);

            if (watch == null)
            {
                return NotFound();
            }

            // Kiểm tra xem người dùng hiện tại đã đánh giá chưa
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Username == User.Identity.Name);
            if (user != null)
            {
                var userRate = watch.Ratings.FirstOrDefault(r => r.UserID == user.UserID);
                ViewBag.HasRated = userRate != null;
            }

            return View(watch);
        }

        [HttpPost]
        public async Task<IActionResult> CreateReview(Review review)
        {
            if (ModelState.IsValid)
            {
                _context.Add(review);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Details), new { id = review.WatchID });
            }
            return View(review);
        }

        // GET: Watches/Create
        public IActionResult Create()
        {
            if (User.IsInRole("Admin"))
            {
                return View();
            }
            return RedirectToAction(nameof(Index), "Watches");
        }

        // POST: Watches/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Watch watch, IFormFile imageFile)
        {

            if (imageFile != null && imageFile.Length > 0)
            {
                var fileTime = DateTime.UtcNow.ToString("yyMMddHHmmss");
                var fileName = fileTime + Path.GetFileName(imageFile.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images", fileName);

                int index = filePath.LastIndexOf("\\wwwroot");
                if (index >= 0)
                {
                    watch.ImagePath = filePath.Substring(index + "\\wwwroot".Length);
                }

                _context.Watches.Add(watch);
                await _context.SaveChangesAsync();

                var watchStatus = new WatchStatus { WatchID = watch.WatchID, Enable = true };
                _context.WatchStatuses.Add(watchStatus);
                await _context.SaveChangesAsync();

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(fileStream);
                }

                return RedirectToAction(nameof(Index));

            }
            return View(watch);
        }

        // GET: Watches/DeleteConfirmation/5
        public async Task<IActionResult> DeleteConfirmation(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var watch = await _context.Watches.FindAsync(id);
            if (watch == null)
            {
                return NotFound();
            }

            return View(watch);
        }

        // DELETE: Watches/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var watch = await _context.Watches.FindAsync(id);

            // Tìm tất cả các mục giỏ hàng liên quan đến sản phẩm này
            var relatedCartItems = _context.CartItems.Where(ci => ci.WatchID == id);
            var relatedReviews = _context.Reviews.Where(rv => rv.WatchID == id);
            var relatedRatings = _context.Ratings.Where(rt => rt.WatchID == id);
            var relatedStatuses = _context.WatchStatuses.Where(ws => ws.WatchID == id);


            // Xóa tất cả các mục giỏ hàng liên quan
            _context.CartItems.RemoveRange(relatedCartItems);
            _context.Reviews.RemoveRange(relatedReviews);
            _context.Ratings.RemoveRange(relatedRatings);
            _context.WatchStatuses.RemoveRange(relatedStatuses);

            // Xóa sản phẩm
            _context.Watches.Remove(watch);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


        [HttpGet]
        public async Task<IActionResult> Search(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                // Trường tìm kiếm rỗng, hiển thị tất cả sản phẩm
                return RedirectToAction(nameof(Index));
            }
            else
            {
                // Tìm kiếm theo từ khóa nhập vào
                var watches = await _context.Watches
                    .Include(w => w.WatchStatus)
                    .Where(w => w.Name.ToLower().Contains(searchTerm.ToLower()) || w.Brand.ToLower().Contains(searchTerm.ToLower()))
                    .ToListAsync();

                return View("Index", watches);
            }
        }



        // GET: Watches/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var watch = await _context.Watches.FindAsync(id);
            if (watch == null)
            {
                return NotFound();
            }

            return View(watch);
        }

        // POST: Watches/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("WatchID,Name,Brand,Description,Price,StockQuantity,ImagePath")] Watch watch)
        {
            if (id != watch.WatchID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(watch);
                    await _context.SaveChangesAsync();

                    // Thêm thông báo thành công vào TempData
                    TempData["Message"] = "Sửa thông tin sản phẩm thành công";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!WatchExists(watch.WatchID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(watch);
        }


        private bool WatchExists(int id)
        {
            return _context.Watches.Any(e => e.WatchID == id);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleEnable(int id)
        {
            var watchStatus = await _context.WatchStatuses.FindAsync(id);
            if (watchStatus == null)
            {
                return NotFound();
            }

            watchStatus.Enable = !watchStatus.Enable;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id });
        }






    }


}
