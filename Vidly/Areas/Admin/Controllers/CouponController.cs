using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Vidly.Data;
using Vidly.Models;

namespace Vidly.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CouponController : Controller
    {
        private readonly ApplicationDbContext _db;
        public CouponController(ApplicationDbContext db)
        {
            _db = db;
        }
        public IActionResult Index()
        {
            List<Coupon> coupons = _db.Coupons.ToList();
            return View(coupons);
        }
        public ActionResult AddCoupon()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddCoupon(Coupon model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            Coupon duplicateCoupon = _db.Coupons.Where(s => s.Name == model.Name).FirstOrDefault();
            if (duplicateCoupon != null)
            {
                ViewBag.ErrorMessage = "A coupon already exists with this name";
                return View(model);
            }
            _db.Coupons.Add(model);
            _db.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            Coupon coupon = _db.Coupons.Find(id);
            if (coupon == null)
            {
                return NotFound();
            }
            return View(coupon);
        }
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            Coupon coupon = _db.Coupons.Find(id);
            if (coupon == null)
            {
                return NotFound();
            }
            return View(coupon);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Coupon coupon)
        {
            if (!ModelState.IsValid)
            {
                return NotFound();
            }
            _db.Update(coupon);
            _db.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            Coupon coupon = _db.Coupons.Find(id);
            if (coupon == null)
            {
                return NotFound();
            }
            return View(coupon);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            Coupon coupon = _db.Coupons.Find(id);
            if (coupon == null)
            {
                return NotFound();
            }
            _db.Coupons.Remove(coupon);
            _db.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
    }
}
