using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        public OrderController(ApplicationDbContext db)
        {
            _db = db;
        }
        public IActionResult Index()
        {
            List<OrderHeader> currentOrders = _db.OrderHeaders.Include(s => s.ApplicationUser).Where(s => s.Status == SD.OrderSubmitted).ToList();
            return View(currentOrders);
        }
        public ActionResult OrderReady(int? orderHeaderId)
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
            List<OrderDetails> userOrders = _db.OrderDetails.Where(s => s.OrderHeaderId == orderHeader.Id).ToList();
            foreach(OrderDetails userOrder in userOrders)
            {
                _db.OrderDetails.Remove(userOrder);
            }
            _db.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
    }
}
