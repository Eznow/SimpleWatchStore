using Microsoft.EntityFrameworkCore;
using BaiTapLon.Models;

namespace BaiTapLon
{
    public class WatchStoreContext : DbContext
    {

        public WatchStoreContext(DbContextOptions<WatchStoreContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Watch> Watches { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Rating> Ratings { get; set; }

    }


}



