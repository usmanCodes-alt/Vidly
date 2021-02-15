using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vidly.Data;
using Vidly.Models;
using Vidly.Models.ViewModels;
using Vidly.Utility;

namespace Vidly.Areas.Admin.Controllers
{
    [Area("Admin")]
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
            List<OrderHeader> currentOrders = _db.OrderHeaders.Include(s => s.ApplicationUser).Where(s => s.Status == SD.OrderSubmitted).ToList();
            return View(currentOrders);
        }
        public async Task<IActionResult> OrderReady(int? orderHeaderId)
        {
            if (orderHeaderId == null)
            {
                return NotFound();
            }
            OrderHeader orderHeader = _db.OrderHeaders.Find(orderHeaderId);
            if (orderHeader == null)
            {
                return NotFound();
            }
            orderHeader.Status = SD.OrderReady;     //Now we have to notify the user
            string userEmail = _db.ApplicationUsers.Where(s => s.Id == orderHeader.ApplicationUserId).FirstOrDefault().Email;
            await _emailSender.SendEmailAsync(userEmail, "Vidly - Order Ready " + orderHeader.Id, "Your order is ready for pick-up");
            List<OrderDetails> userOrders = _db.OrderDetails.Where(s => s.OrderHeaderId == orderHeader.Id).ToList();
            foreach(OrderDetails userOrder in userOrders)
            {
                _db.OrderDetails.Remove(userOrder);
            }
            _db.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
        public ActionResult ConfirmPickup()
        {
            List<OrderHeader> orders = _db.OrderHeaders.Include(s => s.ApplicationUser).Where(s => s.Status == SD.OrderReady).ToList();
            return View(orders);
        }
        public ActionResult Confirm(int? orderHeaderId)
        {
            if (orderHeaderId == null)
            {
                return NotFound();
            }
            OrderHeader userOrder = _db.OrderHeaders.Find(orderHeaderId);
            if (userOrder == null)
            {
                return NotFound();
            }
            userOrder.Status = SD.OrderRecieved;
            _db.SaveChanges();
            return RedirectToAction(nameof(ConfirmPickup));
        }
    }
}
