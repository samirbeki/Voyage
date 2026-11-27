using Microsoft.AspNetCore.Mvc;
using Voyage.Common.Constants;
using Voyage.Services.Contracts;
using Voyage.Web.ViewModels.Enums;

namespace Voyage.Web.Controllers
{
    public class DestinationsController : BaseController
    {
        private readonly IDestinationService destinationService;
        private readonly IReviewService reviewService;
        private readonly IFavoriteService favoriteService;
        private readonly IExpertTipService expertTipService;
        private readonly IWeatherService weatherService;

        public DestinationsController(
            IDestinationService destinationService,
            IReviewService reviewService,
            IFavoriteService favoriteService,
            IExpertTipService expertTipService,
            IWeatherService weatherService)
        {
            this.destinationService = destinationService;
            this.reviewService = reviewService;
            this.favoriteService = favoriteService;
            this.expertTipService = expertTipService;
            this.weatherService = weatherService;
        }

        public async Task<IActionResult> Index(
            string? searchTerm,
            int? categoryId,
            DestinationSorting sorting = DestinationSorting.NameAsc,
            int currentPage = 1)
        {
            var model = await destinationService.GetAllAsync(
                searchTerm,
                categoryId,
                currentPage,
                sorting);

            if (User.Identity?.IsAuthenticated == true)
            {
                string userId = GetUserId();

                var destinationIds = model.Destinations.Select(d => d.Id);

                var favoriteIds = await favoriteService
                    .GetFavoriteDestinationIdsAsync(userId, destinationIds);

                foreach (var destination in model.Destinations)
                {
                    destination.IsFavorite = favoriteIds.Contains(destination.Id);
                }
            }

            return View(model);
        }

        public async Task<IActionResult> Details(int id)
        {
            var destination = await destinationService.GetDetailsAsync(id);

            if (destination == null)
            {
                return NotFound();
            }

            destination.FavoritesCount =
                await favoriteService.GetCountForDestinationAsync(id);

            string? userId = User.Identity?.IsAuthenticated == true
                ? GetUserId()
                : null;

            if (userId != null)
            {
                var destinationIds = destination.SimilarDestinations
                    .Select(d => d.Id)
                    .Append(id);

                var favoriteIds = await favoriteService
                    .GetFavoriteDestinationIdsAsync(userId, destinationIds);

                destination.IsFavorite = favoriteIds.Contains(id);

                foreach (var similarDestination in destination.SimilarDestinations)
                {
                    similarDestination.IsFavorite =
                        favoriteIds.Contains(similarDestination.Id);
                }
            }

            bool canSeeHidden = User.IsInRole(RoleConstants.Moderator)
                             || User.IsInRole(RoleConstants.Administrator);

            destination.Reviews = await reviewService.GetForDestinationAsync(
                id, userId, canSeeHidden);

            destination.RatingSummary =
                await reviewService.GetRatingSummaryAsync(id);

            destination.CanReview = userId != null
                && !await reviewService.HasUserReviewedAsync(id, userId);

            destination.ExpertTips =
                await expertTipService.GetForDestinationAsync(id, userId);

            destination.CanAddExpertTip = userId != null
                && User.IsInRole(RoleConstants.TravelExpert)
                && await expertTipService.CanAddAsync(id, userId);

            return View(destination);
        }

        [HttpGet]
        public async Task<IActionResult> Weather(int id)
        {
            var destination = await destinationService.GetDetailsAsync(id);

            if (destination == null)
            {
                return NotFound();
            }

            var weather = await weatherService.GetForecastAsync(destination.Name, destination.Country);

            if (weather == null)
            {
                return NoContent();
            }

            return PartialView("_Weather", weather.FirstDays(WeatherConstants.DestinationForecastDays));
        }
    }
}