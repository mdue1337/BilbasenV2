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

            TempData["LoginConfirmation"] = "Please check your email for your login code";

            return RedirectToAction("Index", "Home");
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
                UserModel user = _db.LoginUser(Email, Password);
                if (user.Activated == false)
                {
                    return View("Information", user); 
                }

                return RedirectToAction("Information", user); // login screen logic 
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Login failed, try again.";
                return View();
            }
        }

        [HttpGet]
        public IActionResult Information(UserModel user)
        {
            return View(user);
        }

        [HttpGet]
        public IActionResult PasswordUpdate()
        {
            return View();
        }
    }
}
