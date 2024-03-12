using Microsoft.AspNetCore.Mvc;
using MVCtest3d.Database;
using MVCtest3d.Database.DatabaseModels;
using MVCtest3d.Other;

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
                    return RedirectToAction("PasswordUpdate", "User", new { id = user.Id });
                }
                // Man kunne også videresende det hele, men så står det oppe i URL'et. Vi sender dermed kun userId og derefter hentes dataen til det id
                return RedirectToAction("Information", "User", new { id = user.Id });
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
            try
            {
                UserModel _user = _db.GetUser(user.Id);
                return View(_user);
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Cannot access with given params";
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        public IActionResult Information() // take params and then do database stuff
        {
            return View();
        }

        [HttpGet]
        public IActionResult PasswordUpdate(UserModel user) // some kind of check to see if the user exists before accessing
        {
            return View(user.Id);
        }

        [HttpPost]
        public IActionResult PasswordUpdate(int Id, string Password, string confirmPSW)
        {
            if (Password is null || confirmPSW is null)
            {
                TempData["ErrorMessage"] = "Please enter a password";
                return RedirectToAction("PasswordUpdate", "User", new { id = Id });
            }
            else if (Password != confirmPSW)
            {
                TempData["ErrorMessage"] = "Passwords do not match.";
                return RedirectToAction("PasswordUpdate", "User", new { id = Id });
            }

            try
            {
                string encrypted = EncryptionHelper.ComputeSha256Hash(Password);
                _db.UpdatePassword(encrypted, Id);
                _db.ActivateAccount(Id);
                return RedirectToAction("Information", "User", new { id = Id });
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Failed updating password, please try again";
                return RedirectToAction("PasswordUpdate", "User", new { id = Id });
            }
        }
    }
}
