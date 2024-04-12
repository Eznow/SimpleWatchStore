using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace BaiTapLon.Models
{
    public class Watch
    {
        public int WatchID { get; set; }
        public string Name { get; set; }
        public string Brand { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        [ValidateNever]
        public decimal? AverageStarRating { get; set; }
        [Required]
        [Display(Name = "Image")]
        public string ImagePath { get; set; }
        [ValidateNever]
        public virtual ICollection<Review> Reviews { get; set; }
        [ValidateNever]
        public virtual ICollection<Rating> Ratings { get; set; }
        [ValidateNever]
        public virtual ICollection<CartItem> CartItems { get; set; }
        [ValidateNever]
        public WatchStatus WatchStatus { get; set; }
    }
}
