using Microsoft.AspNetCore.Mvc;
using Voyage.Common.Constants;
using Voyage.Web.Services.Contracts;
using Voyage.Web.ViewModels.Enums;

namespace Voyage.Web.Controllers
{
    public class DestinationsController : BaseController
    {
        private readonly IDestinationService destinationService;
        private readonly IReviewService reviewService;
        private readonly IFavoriteService favoriteService;

        public DestinationsController(
            IDestinationService destinationService,
            IReviewService reviewService,
            IFavoriteService favoriteService)
        {
            this.destinationService = destinationService;
            this.reviewService = reviewService;
            this.favoriteService = favoriteService;
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

            return View(destination);
        }
    }
}