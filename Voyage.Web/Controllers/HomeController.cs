using Microsoft.AspNetCore.Mvc;
using Voyage.Web.Services.Contracts;

namespace Voyage.Web.Controllers
{
    public class HomeController : BaseController
    {
        private readonly IHomeService homeService;

        public HomeController(IHomeService homeService)
        {
            this.homeService = homeService;
        }

        public async Task<IActionResult> Index()
        {
            return View(await homeService.GetIndexAsync());
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