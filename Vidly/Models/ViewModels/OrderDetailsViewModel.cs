using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Vidly.Models.ViewModels
{
    public class OrderDetailsViewModel
    {
        public OrderHeader OrderHeader { get; set; }    //General information about order
        public List<OrderDetails> OrderDetails { get; set; }    //Information about every item ordered
    }
}
