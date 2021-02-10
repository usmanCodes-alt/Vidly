using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Vidly.Data;
using Vidly.Models;
using Vidly.Models.ViewModels;
using Vidly.Utility;

namespace Vidly.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _db;
        public HomeController(ApplicationDbContext db)
        {
            _db = db;
        }
        public IActionResult Index()
        {
            ClaimsIdentity claimsIdentity = (ClaimsIdentity)User.Identity;
            Claim claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if (claim != null)
            {
                //User is logged in
                List<Cart> userOrders = _db.Carts.Where(s => s.ApplicationUserId == claim.Value).ToList();
                HttpContext.Session.SetInt32(SD.SessionName, userOrders.Count);
            }
            List<Movie> movies = _db.Movies.Include(s => s.Category).ToList();
            List<Movie> tempList = new List<Movie>();
            foreach(Movie movie in movies)
            {
                if (movie.TotalAvailable == 0)
                {
                    tempList.Add(movie);
                }
            }
            movies.RemoveAll(movie => tempList.Contains(movie));
            return View(movies);
        }
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            Movie movie = _db.Movies.Include(s => s.Category).Where(s => s.Id == id).FirstOrDefault();
            Cart cart = new Cart
            {
                MovieId = (int)id,
                Movie = movie
            };
            return View(cart);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult Details(Cart cart)
        {
            cart.Id = 0;
            if (ModelState.IsValid)
            {
                Movie movieBought = _db.Movies.Find(cart.MovieId);
                if (movieBought.TotalAvailable < cart.Count)
                {
                    Movie currentMovie = _db.Movies.Include(s => s.Category).Where(s => s.Id == cart.MovieId).FirstOrDefault();
                    Cart currentCart = new Cart
                    {
                        MovieId = currentMovie.Id,
                        Movie = currentMovie,
                    };
                    return View(currentCart);
                }
                movieBought.TotalAvailable -= cart.Count;
                ClaimsIdentity claimsIdentity = (ClaimsIdentity)User.Identity;
                Claim claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                cart.ApplicationUserId = claim.Value;
                Cart alreadyExistingCart = _db.Carts.Where(s => s.ApplicationUserId == cart.ApplicationUserId && s.MovieId == cart.MovieId).FirstOrDefault();
                if (alreadyExistingCart != null)
                {
                    //This movie is already once bought by the user
                    alreadyExistingCart.Count += cart.Count;
                }
                else
                {
                    cart.Movie = _db.Movies.Find(cart.MovieId);
                    cart.Movie.Category = _db.Categories.Find(cart.Movie.CategoryId);
                    _db.Carts.Add(cart);
                }
                _db.SaveChanges();
                int totalItemsOrderedByUser = _db.Carts.Where(s => s.ApplicationUserId == cart.ApplicationUserId).ToList().Count;
                HttpContext.Session.SetInt32(SD.SessionName, totalItemsOrderedByUser);
                return RedirectToAction(nameof(Index));
            }
            Movie currentMovieModel = _db.Movies.Include(s => s.Category).Where(s => s.Id == cart.MovieId).FirstOrDefault();
            Cart currentCartModel = new Cart
            {
                MovieId = currentMovieModel.Id,
                Movie = currentMovieModel,
            };
            return View(currentCartModel);
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
