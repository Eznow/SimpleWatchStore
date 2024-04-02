using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace BaiTapLon.Models
{
    public class Rating
    {
        public int RatingID { get; set; }
        public int UserID { get; set; }
        public int WatchID { get; set; }
        [Range(1, 5, ErrorMessage = "Star rating must be between 1 and 5.")]
        public int StarRating { get; set; }

        [ValidateNever]
        public virtual User User { get; set; }
        [ValidateNever]
        public virtual Watch Watch { get; set; }
    }

}
