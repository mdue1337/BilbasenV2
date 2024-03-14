using Microsoft.AspNetCore.Mvc;
using MVCtest3d.Database;
using MVCtest3d.Models;
using System.Diagnostics;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp;
using MVCtest3d.Database.DatabaseModels;

namespace MVCtest3d.Controllers
{
    public class HomeController : Controller
    {
        private readonly DatabaseConnection _db;

        public HomeController(DatabaseConnection db)
        {
            _db = db;
        }

        [HttpGet]
        public IActionResult Index()
        {
            List<ListingModel> listings = _db.GetAllListing();
            listings = listings.Where(x => x.Status == true).ToList();
            return View(listings);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Cars()
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