using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vidly.Data;
using Vidly.Models;
using Vidly.Models.ViewModels;
using Vidly.Utility;

namespace Vidly.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IEmailSender _emailSender;
        public OrderController(ApplicationDbContext db, IEmailSender emailSender)
        {
            _db = db;
            _emailSender = emailSender;
        }
        public IActionResult Index()
        {
            return View();
        }
        //This action shows user their order
        [Authorize]
        public ActionResult ConfirmOrder()
        {
            ClaimsIdentity claimsIdentity = (ClaimsIdentity)User.Identity;
            Claim claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            OrderHeader orderHeader = _db.OrderHeaders.Include(s => s.ApplicationUser).Where(s => s.ApplicationUserId == claim.Value).FirstOrDefault();
            List<OrderDetails> orderDetails = _db.OrderDetails.Include(s => s.Movie).Where(s => s.OrderHeaderId == orderHeader.Id).ToList();
            OrderDetailsViewModel model = new OrderDetailsViewModel
            {
                OrderHeader = orderHeader,
                OrderDetails = orderDetails,
            };
            return View(model);
        }
        public async Task<IActionResult> PlaceOrder()
        {
            double totalAmount = 0;
            ClaimsIdentity claimsIdentity = (ClaimsIdentity)User.Identity;
            Claim claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            ApplicationUser applicationUser = _db.ApplicationUsers.Find(claim.Value);
            List<Cart> userOrders = _db.Carts.Include(s => s.Movie).Where(s => s.ApplicationUserId == claim.Value).ToList();
            foreach (Cart cart in userOrders)
            {
                totalAmount += (cart.Movie.Price * Convert.ToDouble(cart.Count));
            }
            OrderHeader orderHeader = new OrderHeader
            {
                ApplicationUserId = claim.Value,
                ApplicationUser = applicationUser,
                OrderTime = DateTime.Now.ToLocalTime(),
                Status = SD.OrderSubmitted,
                TotalItemsOrdered = userOrders.Count(),
            };
            string couponCheck = HttpContext.Session.GetString(SD.SessionCouponCode);
            //check for any coupon used, if used.. apply discount.
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString(SD.SessionCouponCode)))
            {
                Coupon coupon = _db.Coupons.Where(s => s.Name == HttpContext.Session.GetString(SD.SessionCouponCode)).FirstOrDefault();
                totalAmount = GetDiscount(coupon, totalAmount);
                orderHeader.CouponCode = coupon.Name;
            }
            orderHeader.Price = totalAmount;
            _db.OrderHeaders.Add(orderHeader);
            _db.SaveChanges();
            foreach (Cart cart in userOrders)
            {
                OrderDetails orderDetails = new OrderDetails
                {
                    OrderHeaderId = orderHeader.Id,
                    MovieId = cart.MovieId,
                    Movie = cart.Movie,
                    MovieCount = cart.Count,
                    MoviePrice = cart.Movie.Price,
                    MovieName = cart.Movie.Name,
                    MovieDescription = cart.Movie.Description,
                };
                OrderDetails duplicateOrderDetails = _db.OrderDetails.Where(s => s.MovieId == cart.MovieId).FirstOrDefault();
                if (duplicateOrderDetails == null)
                {
                    orderDetails.Movie = _db.Movies.Find(cart.MovieId);
                    orderDetails.OrderHeader = _db.OrderHeaders.Where(s => s.ApplicationUserId == claim.Value).FirstOrDefault();
                    _db.OrderDetails.Add(orderDetails);
                }
                else
                {
                    duplicateOrderDetails.MovieCount += orderDetails.MovieCount;
                }
            }
            _db.SaveChanges();
            OrderDetailsViewModel model = new OrderDetailsViewModel
            {
                OrderHeader = orderHeader,
                OrderDetails = _db.OrderDetails.Where(s => s.OrderHeaderId == orderHeader.Id).ToList(),
            };
            List<Cart> carts = _db.Carts.Where(s => s.ApplicationUserId == claim.Value).ToList();
            foreach(Cart cart in carts)
            {
                _db.Carts.Remove(cart);
            }
            _db.SaveChanges();
            string userEmail = _db.ApplicationUsers.Where(s => s.Id == claim.Value).FirstOrDefault().Email;
            await _emailSender.SendEmailAsync(userEmail, "Vidly - Order Confirmed " + model.OrderHeader.Id, "Order has been submitted successfully");
            HttpContext.Session.SetString(SD.SessionName, string.Empty);
            return RedirectToAction(nameof(ConfirmOrder));
        }
        private double GetDiscount(Coupon coupon, double currentAmount)
        {
            if (coupon == null || currentAmount < coupon.MinimumPurchaseRequired)
            {
                return currentAmount;
            }
            double discount = currentAmount * coupon.DiscountPercentage / 100;
            currentAmount -= discount;
            return currentAmount;
        }
    }
}
