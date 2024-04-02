using BaiTapLon.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;


namespace BaiTapLon.Controllers
{
    public class ReviewsController : Controller 
    {
        private readonly WatchStoreContext _context;

        public ReviewsController(WatchStoreContext context) 
        {
            _context = context;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("WatchID,Comment,StarRating")] Review review, int? starRating)
        {
            if (ModelState.IsValid)
            {
                var user = await _context.Users.SingleOrDefaultAsync(u => u.Username == User.Identity.Name);

                if (user != null)
                {
                    var existingRating = await _context.Ratings.SingleOrDefaultAsync(r => r.UserID == user.UserID && r.WatchID == review.WatchID);

                    if (existingRating == null && starRating.HasValue)
                    {
                        var rating = new Rating { UserID = user.UserID, WatchID = review.WatchID, StarRating = starRating.Value };
                        _context.Add(rating);
                    }

                    review.UserID = user.UserID;
                    _context.Add(review);

                    var newRating = await _context.Ratings.FirstOrDefaultAsync(r => r.UserID == review.UserID && r.WatchID == review.WatchID);

                    await _context.SaveChangesAsync();

                    var watchIdParam = new SqlParameter("@WatchID", review.WatchID);
                    var userIdParam = new SqlParameter("@UserID", user.UserID);
                    var starRatingParam = new SqlParameter("@StarRating", starRating.HasValue ? starRating.Value : (object)DBNull.Value);

                    _context.Database.ExecuteSqlRaw("EXEC UpdateAverageStarRating @WatchID, @UserID, @StarRating", watchIdParam, userIdParam, starRatingParam);

                    var watch = await _context.Watches.SingleOrDefaultAsync(w => w.WatchID == review.WatchID);

                    var commentViewModel = new
                    {
                        Username = user.Username,
                        Comment = review.Comment,
                    };

                    return Json(new { username = user.Username, comment = review.Comment, starRating = newRating?.StarRating, averageStarRating = watch.AverageStarRating }); 
                }
                else
                {
                    return View("Error");
                }
            }

            return View(review);
        }









    }
}
