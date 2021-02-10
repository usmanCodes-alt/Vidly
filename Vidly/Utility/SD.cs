using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Vidly.Utility
{
    public static class SD
    {
        //Roles
        public const string AdminUser = "Admin";
        public const string CustomerUser = "Customer";
        public const string ManagerUser = "Manager";
        
        //Sessions
        public const string SessionName = "ssCartCount";
        public const string SessionCouponCode = "couponCode";

        //Order Status
        public const int OrderSubmitted = 1;
        public const int OrderRecieved = 0;
    }
}
