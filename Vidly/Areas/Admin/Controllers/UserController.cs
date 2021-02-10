using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vidly.Data;
using Vidly.Models;

namespace Vidly.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _db;
        public UserController(ApplicationDbContext db)
        {
            _db = db;
        }
        [Authorize(Roles = "Manager")]
        public IActionResult Index()
        {
            ClaimsIdentity claimsIdentity = (ClaimsIdentity)User.Identity;
            var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            IEnumerable<ApplicationUser> applicationUsers = _db.ApplicationUsers.Where(s => s.Id != claims.Value).ToList();
            return View(applicationUsers);
        }
        [Authorize(Roles = "Manager")]
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }
            ApplicationUser user = _db.ApplicationUsers.Find(id);
            if (user == null)
            {
                return NotFound();
            }
            _db.ApplicationUsers.Remove(user);
            _db.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
    }
}
