using Microsoft.AspNetCore.Mvc;
using MVCtest3d.Database;
using MVCtest3d.Database.DatabaseModels;

namespace MVCtest3d.Controllers
{
    public class UserController : HomeController
    {
        private readonly DatabaseConnection _db;

        public UserController(DatabaseConnection db) : base(db)
        {
            _db = db;
        }

        [HttpGet]
        public IActionResult SignUp()
        {
            return View();
        }

        [HttpPost]
        public IActionResult SignUp(string testMail, int Age, string Name)
        {
            UserModel user = new()
            {
                Age = Age,
                Name = Name
            };

            _db.CreateUser(testMail, user);
            return View();
        }

        [HttpGet]
        public IActionResult SignIn()
        {
            return View();
        }

        [HttpPost]
        public IActionResult SignIn(string Email, string Password)
        {
            try
            {
                List<UserModel> user = _db.LoginUser(Email, Password);
                if (user[0].Activated == false)
                {
                    return RedirectToAction("Index", "Home"); // update password logic
                }

                return View(); // login screen logic 
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Login failed, try again.";
                return View();
            }
        }
    }
}
