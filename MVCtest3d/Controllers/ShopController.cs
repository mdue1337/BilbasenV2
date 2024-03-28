using Microsoft.AspNetCore.Mvc;
using MVCtest3d.Database;
using MVCtest3d.Database.DatabaseModels;
using MVCtest3d.Models;
using MVCtest3d.Other;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;

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
            if(HttpContext.Session.GetInt32("UserId") == null)
            {
                TempData["ErrorMessage"] = "You must be logged in to create a listing. Please sign in or up";
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        public IActionResult CreateListing(int Price, int Year, int Horsepower, string Brand, string Model, string Timestamp, string Location)
        {
            try
            {
                if(HttpContext.Session.GetInt32("UserId") == null)
                {
                    throw new Exception();
                }

                ListingModel model = new()
                {
                    Price = Price,
                    Year = Year,
                    Horsepower = Horsepower,
                    Brand = Brand,
                    Model = Model,
                    Created = Timestamp,
                    UserId = (int)HttpContext.Session.GetInt32("UserId"),
                    Location = Location
                };

                int listingId = _db.CreateListing(model);

                return RedirectToAction("UpdateListing", "Shop", new { Id = listingId });
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Failed creating the listing, try again";
                return View();
            }
        }

        [HttpGet]
        public IActionResult UpdateListing(ListingModel listing)
        {
            try
            {
                listing = _db.GetSpecificListing(listing.Id);

                if (listing.UserId != HttpContext.Session.GetInt32("UserId"))
                {
                    throw new Exception();
                }

                List<PictureModel> pictures = _db.GetListingPictures(listing.Id);

                UpdateListingModel info = new()
                {
                    ListingModel = listing,
                    PictureModel = pictures
                };

                return View(info);
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Cannot update a listing you do not own";
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        public IActionResult UpdateListing(IFormFile myFile, int ListingId)
        {
            try
            {
                byte[] imageBytes;

                using (var ms = new MemoryStream())
                {
                    myFile.CopyTo(ms);
                    ms.Seek(0, SeekOrigin.Begin);

                    using (var image = Image.Load(ms))
                    {
                        using (var outputStream = new MemoryStream())
                        {
                            image.Save(outputStream, new JpegEncoder());
                            imageBytes = outputStream.ToArray();
                        }
                    }
                }

                _db.InsertPicture(imageBytes, ListingId);

                return RedirectToAction("UpdateListing", "Shop", new { Id = ListingId });
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Failed uploading picture, try again";
                return RedirectToAction("UpdateListing", "Shop", new { Id = ListingId });
            }
        }

        [HttpGet]
        public IActionResult DeleteListing(int listingId)
        {
            ListingModel listing = _db.GetSpecificListing(listingId);

            if(listing.UserId != HttpContext.Session.GetInt32("UserId"))
            {
                TempData["ErrorMessage"] = "Cannot delete a listing you do not own";
                return RedirectToAction("Index", "Home");
            }
            else if (listing.Status == false)
            {
                TempData["ErrorMessage"] = "Cannot delete a listing that has been sold";
                return RedirectToAction("Index", "Home");
            }

            _db.DeleteListing(listing.Id);

            TempData["BuySucess"] = "Listing was succesfully deleted";
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult ShowListing(ListingModel listing)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                ViewBag.UserStatus = false;
            }
            else
            {
                ViewBag.UserStatus = true;
            }

            listing = _db.GetSpecificListing(listing.Id);
            List<PictureModel> pictures = _db.GetListingPictures(listing.Id);
            Uri maps = MapsCollector.GenerateMap(listing.Location);

            ShowListingModel info = new()
            {
                ListingModel = listing,
                PictureModel = pictures,
                GoogleMaps = maps
            };

            return View(info);
        }

        [HttpPost]
        public IActionResult ShowListing(int ListingId)
        {
            int? BuyerId = HttpContext.Session.GetInt32("UserId");

            if(BuyerId == null)
            {
                TempData["ErrorMessage"] = "You must be logged in to buy a product";
                return RedirectToAction("Index", "Home");
            }

            try
            {
                _db.PurchaseListing(ListingId, (int)BuyerId);
                TempData["BuySucess"] = "You succesfully bought this listing";
                return RedirectToAction("Index", "Home");
            }
            catch (InvalidOperationException)
            {
                TempData["ErrorMessage"] = "You cannot buy your own listing";
                return RedirectToAction("Index", "Home");
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Error buying listing";
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpGet]
        public IActionResult MessageSeller(int targetId)
        {
            int? connection = HttpContext.Session.GetInt32("UserId");

            if (connection == null)
            {
                TempData["ErrorMessage"] = "You must be logged in to start a chat";
                return RedirectToAction("Index", "Home");
            }

            MessageSellerModel model = new()
            {
                ConnectionId = (int)connection,
                TargetId = targetId
            };

            return View(model);
        }
    }
}
