using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Vidly.Models
{
    public class OrderHeader
    {
        public OrderHeader()
        {
            Price = 0;
        }
        [Key]
        public int Id { get; set; }
        [Required]
        [Display(Name = "User")]
        public string ApplicationUserId { get; set; }
        [ForeignKey("ApplicationUserId")]
        public virtual ApplicationUser ApplicationUser { get; set; }
        [Required]
        [Display(Name = "Order Time")]
        public DateTime OrderTime { get; set; }
        public double Price { get; set; }
        public int Status { get; set; }
        [Display(Name = "Pickup Time")]
        public DateTime PickUpTime { get; set; }
        public string CouponCode { get; set; }
        public int TotalItemsOrdered { get; set; }
        public string TransactionId { get; set; }
    }
}
