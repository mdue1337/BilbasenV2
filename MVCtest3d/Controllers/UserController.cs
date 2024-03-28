using Microsoft.AspNetCore.Mvc;
using MVCtest3d.Database;
using MVCtest3d.Database.DatabaseModels;
using MVCtest3d.Hubs.Model;
using MVCtest3d.Models;

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
                HttpContext.Session.SetInt32("UserId", user.Id);
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
                if (HttpContext.Session.GetInt32("UserId") != user.Id)
                {
                    throw new();
                }

                // Get user
                UserModel _user = _db.GetUser(user.Id);

                // Get users listings
                List<ListingModel> userListings = _db.GetAllListing();
                userListings = userListings.Where(x => x.UserId == user.Id).ToList();

                // Get all user bought listings
                List<BuyHistory> _buy = _db.getUserHistory(user.Id);
                List<ListingModel> Buylistings = new();

                for (int i = 0; i < _buy.Count; i++)
                {
                    ListingModel data = _db.GetSpecificListing(_buy[i].ListingId);
                    Buylistings.Add(data);
                }

                // Get chats for user
                List<ChatRoomModel> chats = _db.GetChats(user.Id);

                InformationModel _info = new()
                {
                    User = _user,
                    BuyHistoryListings = Buylistings,
                    UserListings = userListings,
                    Chats = chats
                };
                return View(_info);
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Cannot access with given params";
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        public IActionResult Information(string pw) // take params and then do database stuff
        {
            int id = (int)HttpContext.Session.GetInt32("UserId");
            _db.UpdatePassword(pw, id);
            TempData["PasswordUpdate"] = "Password was updated";
            return RedirectToAction("Information", "User", new { Id = id });
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
                _db.UpdatePassword(Password, Id);
                _db.ActivateAccount(Id);

                HttpContext.Session.SetInt32("UserId", Id);

                return RedirectToAction("Information", "User", new { id = Id });
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Failed updating password, please try again";
                return RedirectToAction("PasswordUpdate", "User", new { id = Id });
            }
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ForgotPassword(string email)
        {
            _db.ResetPassword(email);
            TempData["email"] = $"Mail sent to: {email}";
            return RedirectToAction("SignIn", "User");
        }
    }
}
