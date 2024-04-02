namespace BaiTapLon.Models
{
    public class Cart
    {
        public int CartID { get; set; }
        public int UserID { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public virtual User User { get; set; }
        public virtual ICollection<CartItem> CartItems { get; set; }
    }


}
