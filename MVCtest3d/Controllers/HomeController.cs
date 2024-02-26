using Microsoft.AspNetCore.Mvc;
using MVCtest3d.Database;
using MVCtest3d.Models;
using System.Diagnostics;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp;

namespace MVCtest3d.Controllers
{
    public class HomeController : Controller
    {
        private readonly DatabaseConnection _db;

        public HomeController(DatabaseConnection db)
        {
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

        [HttpGet]
        public IActionResult About()
        {
            return View();
        }

        [HttpPost]
        public IActionResult About(IFormFile myFile)
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

                return View(imageBytes);
            }
            catch (Exception)
            {
                return Redirect("Index");
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}