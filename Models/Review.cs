using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BaiTapLon.Models
{
    public class Review
    {
        public int ReviewID { get; set; }
        [ValidateNever]
        public int UserID { get; set; }

        [Required(ErrorMessage = "WatchID is required.")]
        public int WatchID { get; set; }

        [MaxLength(500, ErrorMessage = "Comment cannot exceed 500 characters.")]
        public string Comment { get; set; } // Comment của người dùng

        [ValidateNever]
        [JsonIgnore]

        public virtual User User { get; set; }
        [ValidateNever]

        public virtual Watch Watch { get; set; }
    }
}