using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Vidly.Data;
using Vidly.Models;
using Vidly.Models.ViewModels;

namespace Vidly.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin, Manager")]
    public class MovieController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _webHost;
        [TempData]
        public string StatusMessage { get; set; }
        public MovieController(ApplicationDbContext db, IWebHostEnvironment webHost)
        {
            _db = db;
            _webHost = webHost;
        }
        public IActionResult Index()
        {
            List<Movie> movies = _db.Movies.Include(s => s.Category).ToList();
            return View(movies);
        }
        public ActionResult Create()
        {
            IEnumerable<Category> categories = _db.Categories.ToList();
            IEnumerable<SelectListItem> categorySelectList = categories.Select(s => new SelectListItem
            {
                Text = s.Name,
                Value = s.Id.ToString()
            });
            CreateNewMovieViewModel model = new CreateNewMovieViewModel
            {
                Movie = new Movie(),
                Categories = categorySelectList,
                StatusMessage = ""
            };
            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CreateNewMovieViewModel model)
        {
            if (ModelState.IsValid)
            {
                List<Movie> duplicateMovies = _db.Movies.Where(s => s.Name == model.Movie.Name).ToList();
                if (duplicateMovies.Count() > 0)
                {
                    StatusMessage = "A movie with same name already exists, please try again.";
                }
                else
                {
                    _db.Movies.Add(model.Movie);
                    _db.SaveChanges();

                    //now we have to save the image
                    string pathToWebRoot = _webHost.WebRootPath;
                    var files = HttpContext.Request.Form.Files;
                    int lastInsertedId = _db.Movies.Max(item => item.Id);
                    Movie movie = _db.Movies.Find(model.Movie.Id);
                    if (files.Count > 0)
                    {
                        string path = Path.Combine(pathToWebRoot, "images");
                        string fileExtention = Path.GetExtension(files[0].FileName);
                        using (FileStream fileStream = new FileStream(Path.Combine(path, lastInsertedId + fileExtention), FileMode.Create))
                        {
                            files[0].CopyTo(fileStream);
                        }
                        movie.Image = @"\images\" + lastInsertedId + fileExtention;
                    }
                    else
                    {
                        string path = Path.Combine(pathToWebRoot, @"images\" + "default-image.jpg");
                        System.IO.File.Copy(path, pathToWebRoot + @"\images\" + lastInsertedId + ".jpg");
                        movie.Image = @"\images\" + lastInsertedId + ".jpg";
                    }
                    _db.Update(movie);
                    _db.SaveChanges();
                    return RedirectToAction(nameof(Index));
                }
            }
            IEnumerable<Category> categories = _db.Categories.ToList();
            IEnumerable<SelectListItem> categorySelectListItem = categories.Select(s => new SelectListItem
            {
                Text = s.Name,
                Value = s.Id.ToString()
            });
            CreateNewMovieViewModel viewModel = new CreateNewMovieViewModel
            {
                Movie = model.Movie,
                Categories = categorySelectListItem
            };
            return View(viewModel);
        }
        public ActionResult Details(int? id)
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
            IEnumerable<Category> categories = _db.Categories.ToList();
            IEnumerable<SelectListItem> categorySelectListItem = categories.Select(s => new SelectListItem
            {
                Text = s.Name,
                Value = s.Id.ToString()
            });
            CreateNewMovieViewModel model = new CreateNewMovieViewModel
            {
                Movie = movie,
                Categories = categorySelectListItem
            };
            return View(model);
        }
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            Movie movie = _db.Movies.SingleOrDefault(s => s.Id == id);
            if (movie == null)
            {
                return NotFound();
            }
            IEnumerable<Category> categories = _db.Categories.ToList();
            IEnumerable<SelectListItem> categorySelectListItem = categories.Select(s => new SelectListItem
            {
                Text = s.Name,
                Value = s.Id.ToString()
            });
            CreateNewMovieViewModel model = new CreateNewMovieViewModel
            {
                Movie = movie,
                Categories = categorySelectListItem
            };
            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, CreateNewMovieViewModel model)
        {
            if (!ModelState.IsValid)
            {
                IEnumerable<Category> categories = _db.Categories.ToList();
                IEnumerable<SelectListItem> categorySelectList = categories.Select(s => new SelectListItem
                {
                    Text = s.Name,
                    Value = s.Id.ToString()
                });
                CreateNewMovieViewModel viewModel = new CreateNewMovieViewModel
                {
                    Movie = model.Movie,
                    Categories = categorySelectList,
                    StatusMessage = "Please provide all the information correctly.",
                };
                return View(viewModel);
            }
            string pathToWebRoot = _webHost.WebRootPath;
            var files = HttpContext.Request.Form.Files;
            Movie movie = _db.Movies.Find(id);
            if (files.Count > 0)
            {
                string pathToUploadAt = Path.Combine(pathToWebRoot, "images");
                string fileExtension = Path.GetExtension(files[0].FileName);
                string imagePath = Path.Combine(pathToWebRoot, movie.Image.TrimStart('\\'));

                //Delete previously uploaded file
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }

                using (FileStream fileStream = new FileStream(Path.Combine(pathToUploadAt, id + fileExtension), FileMode.Create))
                {
                    files[0].CopyTo(fileStream);
                }
                movie.Image = @"\images\" + id + fileExtension;
            }
            movie.Name = model.Movie.Name;
            movie.Description = model.Movie.Description;
            movie.TotalAvailable = model.Movie.TotalAvailable;
            movie.IsAvailable = model.Movie.IsAvailable;
            movie.CategoryId = model.Movie.CategoryId;
            _db.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
        public ActionResult Delete(int? id)
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
            Category category = _db.Categories.Find(movie.CategoryId);
            MovieDetailsViewModel model = new MovieDetailsViewModel
            {
                Movie = movie,
                CategoryName = category.Name
            };
            return View(model);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteMovie(int id)
        {
            Movie movie = _db.Movies.Find(id);
            if (movie == null)
            {
                return NotFound();
            }
            string webRootPath = _webHost.WebRootPath;
            string imagePath = Path.Combine(webRootPath, movie.Image.TrimStart('\\'));
            if (System.IO.File.Exists(imagePath))
            {
                System.IO.File.Delete(imagePath);
            }
            _db.Movies.Remove(movie);
            _db.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
    }
}
