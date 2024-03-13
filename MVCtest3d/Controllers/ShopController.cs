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
            return View();
        }

        [HttpPost]
        public IActionResult CreateListing(int Price, int Year, int Horsepower, string Brand, string Model, string Timestamp, int userId, string Location)
        {
            try
            {
                ListingModel model = new()
                {
                    Price = Price,
                    Year = Year,
                    Horsepower = Horsepower,
                    Brand = Brand,
                    Model = Model,
                    Created = Timestamp,
                    UserId = userId,
                    Location = Location
                };

                if(userId == 0)
                {
                    throw new Exception();
                }

                _db.CreateListing(model, userId);

                return RedirectToAction("UpdateListing", "Shop", new {id = model.Id});
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
            listing = _db.GetSpecificListing(listing.Id);
            List<PictureModel> pictures = _db.GetListingPictures(listing.Id);

            UpdateListingModel info = new()
            {
                ListingModel = listing,
                PictureModel = pictures
            };

            return View(info);
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

                return RedirectToAction("UpdateListing", "Shop", new { id = ListingId });
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Failed uploading picture, try again";
                return RedirectToAction("UpdateListing", "Shop", new { id = ListingId });
            }
        }

        [HttpGet]
        public IActionResult ShowListing(ListingModel listing)
        {
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
            catch (Exception)
            {
                return View(new { id = ListingId });
            }
        }
    }
}
