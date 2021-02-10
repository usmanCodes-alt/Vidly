using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Vidly.Data;
using Vidly.Models;
using Vidly.Models.ViewModels;
using Vidly.Utility;

namespace Vidly.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _db;
        public CartController(ApplicationDbContext db)
        {
            _db = db;
        }
        [Authorize]
        public IActionResult Index(string selectedCoupon)
        {
            OrderHeader orderHeader = new OrderHeader();
            ClaimsIdentity claimsIdentity = (ClaimsIdentity)User.Identity;
            Claim claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            List<Cart> userOrders = _db.Carts.Include(s => s.Movie).Where(s => s.ApplicationUserId == claim.Value).ToList();
            IEnumerable<Coupon> coupons = _db.Coupons.Where(s => s.IsActive == true).ToList();
            IEnumerable<SelectListItem> couponSelectListItem = coupons.Select(s => new SelectListItem
            {
                Text = s.Name,
                Value = s.Id.ToString()
            });
            CartIndexViewModel model = new CartIndexViewModel
            {
                OrderHeader = orderHeader,
                UserOrders = userOrders,
                Coupons = couponSelectListItem
            };
            if (selectedCoupon != null)
            {
                Coupon selectedCouponFromDb = _db.Coupons.Where(s => s.Name == selectedCoupon).FirstOrDefault();
                model.CouponId = selectedCouponFromDb.Id;
            }
            //Now we have to calculate the total inside the order header
            foreach (Cart cart in userOrders)
            {
                double individualMoviePriceByQuantity = 0;
                individualMoviePriceByQuantity += (cart.Movie.Price * cart.Count);
                model.OrderHeader.Price = model.OrderHeader.Price + individualMoviePriceByQuantity;
                model.OrderHeader.TotalItemsOrdered++;
            }
            if (HttpContext.Session.GetString(SD.SessionCouponCode) != null && selectedCoupon != null)
            {
                Coupon coupon = _db.Coupons.Where(s => s.Name == HttpContext.Session.GetString(SD.SessionCouponCode)).FirstOrDefault();
                model.OrderHeader.Price = GetDiscount(coupon, model.OrderHeader.Price);
            }
            return View(model);
        }
        public ActionResult AddCoupon(CartIndexViewModel model)
        {
            Coupon coupon = _db.Coupons.Find(Convert.ToInt32(model.OrderHeader.CouponCode));
            HttpContext.Session.SetString(SD.SessionCouponCode, coupon.Name);
            return RedirectToAction(nameof(Index), new { selectedCoupon = coupon.Name });
        }
        public ActionResult RemoveCoupon(CartIndexViewModel model)
        {
            HttpContext.Session.SetString(SD.SessionCouponCode, string.Empty);
            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult IncreaseQuantity(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            Movie movie = _db.Movies.Find(id);
            if (movie == null)
            {
                return NotFound();
            }
            if (movie.TotalAvailable == 0)
            {
                return RedirectToAction(nameof(Index));
            }
            movie.TotalAvailable--;
            //Get cart that has this movie for this user
            ClaimsIdentity claimsIdentity = (ClaimsIdentity)User.Identity;
            Claim claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            Cart cart = _db.Carts.Where(s => s.ApplicationUserId == claim.Value && s.MovieId == id).FirstOrDefault();
            cart.Count++;
            _db.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DecreaseQuantity(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            Movie movie = _db.Movies.Find(id);
            if (movie == null)
            {
                return NotFound();
            }
            movie.TotalAvailable++;
            ClaimsIdentity claimsIdentity = (ClaimsIdentity)User.Identity;
            Claim claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            Cart cart = _db.Carts.Where(s => s.ApplicationUserId == claim.Value && s.MovieId == id).FirstOrDefault();
            cart.Count--;
            _db.SaveChanges();
            if (cart.Count == 0)
            {
                return RedirectToAction(nameof(RemoveItem), new { id });
            }
            return RedirectToAction(nameof(Index));
        }
        public ActionResult RemoveItem(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            ClaimsIdentity claimsIdentity = (ClaimsIdentity)User.Identity;
            Claim claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            Cart cart = _db.Carts.Where(s => s.ApplicationUserId == claim.Value && s.MovieId == id).FirstOrDefault();
            Movie movie = _db.Movies.Find(cart.MovieId);
            if (cart == null)
            {
                return NotFound();
            }
            _db.Carts.Remove(cart);
            movie.TotalAvailable = cart.Count;
            _db.SaveChanges();
            return RedirectToAction(nameof(Index));
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
