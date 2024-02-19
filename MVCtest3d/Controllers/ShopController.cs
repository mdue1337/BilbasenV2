using Microsoft.AspNetCore.Mvc;
using MVCtest3d.Database;

namespace MVCtest3d.Controllers
{
    public class ShopController : HomeController
    {
        private readonly DatabaseConnection _db;

        public ShopController(DatabaseConnection db) : base(db)
        {
            _db = db;
        }

        public IActionResult CreateListing() 
        {
            return View();
        }
    }
}
