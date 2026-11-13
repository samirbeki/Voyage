using Microsoft.AspNetCore.Mvc;

namespace Voyage.Web.Controllers
{
    public class HomeController : BaseController
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("ServerError");
        }

        public IActionResult HttpError(int statusCode)
        {
            if (statusCode == StatusCodes.Status404NotFound)
            {
                return View("NotFound");
            }

            return View("ServerError");
        }
    }
}