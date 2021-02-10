using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Vidly.Models.ViewModels
{
    public class CartIndexViewModel
    {
        public List<Cart> UserOrders { get; set; }
        public OrderHeader OrderHeader { get; set; }
        public IEnumerable<SelectListItem> Coupons { get; set; }
        public int? CouponId { get; set; }
    }
}
