using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Vidly.Models
{
    public class Coupon
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        [Display(Name = "Discount Percentage")]
        public double DiscountPercentage { get; set; }
        [Required]
        [Display(Name = "Minimum Purchase Required")]
        public double MinimumPurchaseRequired { get; set; }
        [Display(Name = "Active")]
        public bool IsActive { get; set; }
    }
}
