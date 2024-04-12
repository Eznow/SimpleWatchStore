using System.ComponentModel.DataAnnotations;

namespace BaiTapLon.Models
{
    public class WatchStatus
    {
        [Key]
        public int WatchID { get; set; }
        public bool Enable { get; set; }

        public Watch Watch { get; set; }
    }
}
