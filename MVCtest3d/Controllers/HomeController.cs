using Microsoft.AspNetCore.Mvc;
using MVCtest3d.Database;
using MVCtest3d.Database.DatabaseModels;
using MVCtest3d.Models;
using System.Diagnostics;

namespace MVCtest3d.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly DatabaseConnection _db;

        public HomeController(ILogger<HomeController> logger, DatabaseConnection db)
        {
            _logger = logger;
            _db = db;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult About()
        {
            if (TempData.ContainsKey("SuccessMessage"))
            {
                ViewBag.SuccessMessage = TempData["SuccessMessage"];
            }

            List<UserModel> result = _db.GetUsers().OrderBy(x => x.Name).ToList();

            return View(result);
        }

        [HttpGet]
        public IActionResult SignUp() 
        {
            return View();
        }

        [HttpPost]
        public IActionResult SignUp(UserModel model)
        {
            _db.CreateUser(model);
            TempData["SuccessMessage"] = "User successfully created.";
            return RedirectToAction("About");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}