using Microsoft.AspNetCore.Mvc;
using MVCtest3d.Database;
using MVCtest3d.Models;
using System.Diagnostics;
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
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                ViewBag.UserStatus = false;
            }
            else
            {
                ViewBag.UserStatus = true;
            }

            List<ListingModel> listings = _db.GetAllListing();
            listings = listings.Where(x => x.Status == true).ToList();
            return View(listings);
        }

        [HttpGet]
        public IActionResult Privacy()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Cars()
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                ViewBag.UserStatus = false;
            }
            else
            {
                ViewBag.UserStatus = true;
            }

            List<ListingModel> listings = _db.GetAllListing();
            listings = listings.Where(x => x.Status == true).ToList();
            return View(listings);
        }
        [HttpPost]
        public IActionResult Cars(int sortingMethod, string searchParam)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                ViewBag.UserStatus = false;
            }
            else
            {
                ViewBag.UserStatus = true;
            }

            List<ListingModel> listings = _db.GetAllListing();
            listings = listings.Where(x => x.Status == true).ToList();

            switch (sortingMethod)
            {
                case 1:
                    listings = listings.OrderBy(x => x.Price).ToList();
                    break;
                case 2:
                    listings = listings.OrderByDescending(x => x.Price).ToList();
                    break;
                case 3:
                    listings = listings.OrderBy(x => x.Year).ToList();
                    break;
                case 4:
                    listings = listings.OrderByDescending(x => x.Year).ToList();
                    break;
                case 5:
                    listings = listings.OrderBy(x => x.Brand).ToList();
                    break;
                case 6:
                    listings = listings.OrderBy(x => x.Model).ToList();
                    break;
                case 7:
                    if(searchParam == null)
                    {
                        break;
                    }
                    // ligeglad om det er med lowercase / uppercase
                    listings = listings.Where(x => x.Brand.Contains(searchParam, StringComparison.InvariantCultureIgnoreCase) || x.Brand.Contains(searchParam, StringComparison.InvariantCultureIgnoreCase)).ToList();
                    break;
                case 8:
                    listings = listings.Where(x => x.Price <= int.Parse(searchParam)).ToList();
                    break;
                case 9:
                    listings = listings.Where(x => x.Price >= int.Parse(searchParam)).ToList();
                    break;
                case 10:
                    listings = listings.Where(x => x.Year <= int.Parse(searchParam)).ToList();
                    break;
                case 11:
                    listings = listings.Where(x => x.Year >= int.Parse(searchParam)).ToList();
                    break;
                case 12:
                    string[] param = searchParam.Split(' ');
                    listings = listings.Where(x => x.Price <= int.Parse(param[0]) && x.Price >= int.Parse(param[1]) && x.Year <= int.Parse(param[2]) && x.Year >= int.Parse(param[3])).ToList();
                    break;
            }
            
            return View(listings);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}