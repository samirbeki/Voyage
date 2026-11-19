using Microsoft.AspNetCore.Mvc;
using Voyage.Web.Services.Contracts;

namespace Voyage.Web.Controllers
{
    public class HomeController : BaseController
    {
        private readonly IHomeService homeService;
        private readonly IFavoriteService favoriteService;

        public HomeController(
            IHomeService homeService,
            IFavoriteService favoriteService)
        {
            this.homeService = homeService;
            this.favoriteService = favoriteService;
        }

        public async Task<IActionResult> Index()
        {
            var model = await homeService.GetIndexAsync();

            if (User.Identity?.IsAuthenticated == true)
            {
                string userId = GetUserId();

                var destinationIds = model.PopularDestinations.Select(d => d.Id);

                var favoriteIds = await favoriteService
                    .GetFavoriteDestinationIdsAsync(userId, destinationIds);

                foreach (var destination in model.PopularDestinations)
                {
                    destination.IsFavorite = favoriteIds.Contains(destination.Id);
                }
            }

            return View(model);
        }

        public IActionResult About()
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