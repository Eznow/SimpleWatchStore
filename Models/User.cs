using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace BaiTapLon.Models
{
    public class User
    {
        [Key]
        public int UserID { get; set; }
        [Required(ErrorMessage = "Username is required")]
        public string Username { get; set; }
        [Required(ErrorMessage = "Password is required")]
        [StringLength(50)]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [ValidateNever]
        public string Role { get; set; } // "Admin" or "Customer"
        [ValidateNever]
        public virtual ICollection<Rating> Ratings { get; set; }
        [ValidateNever]
        public virtual ICollection<Review> Reviews { get; set; }
        [ValidateNever]
        public virtual ICollection<Cart> Carts { get; set; }
    }
}
