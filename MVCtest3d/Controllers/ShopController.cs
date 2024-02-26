using Microsoft.AspNetCore.Mvc;
using MVCtest3d.Database;
using MVCtest3d.Database.DatabaseModels;

namespace MVCtest3d.Controllers
{
    public class ShopController : HomeController
    {
        private readonly DatabaseConnection _db;

        public ShopController(DatabaseConnection db) : base(db)
        {
            _db = db;
        }

        [HttpGet]
        public IActionResult CreateListing() 
        {
            return View();
        }

        [HttpPost]
        public IActionResult CreateListing(int Price, int Year, int Horsepower, string Brand, string Model, string Timestamp, int userId)
        {
            try
            {
                ListingModel model = new ListingModel
                {
                    Price = Price,
                    Year = Year,
                    Horsepower = Horsepower,
                    Brand = Brand,
                    Model = Model,
                    Created = Timestamp
                };

                _db.CreateListing(model, userId);

                return BadRequest();
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Failed creating the listing, try again";
                return View();
            }
        }
    }
}
