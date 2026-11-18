using Microsoft.AspNetCore.Mvc;
using Voyage.Web.Common;
using Voyage.Web.Services.Contracts;

namespace Voyage.Web.Controllers
{
    public class DestinationsController : BaseController
    {
        private readonly IDestinationService destinationService;
        private readonly IReviewService reviewService;

        public DestinationsController(
            IDestinationService destinationService,
            IReviewService reviewService)
        {
            this.destinationService = destinationService;
            this.reviewService = reviewService;
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

            return View(model);
        }

        public async Task<IActionResult> Details(int id)
        {
            var destination = await destinationService.GetDetailsAsync(id);

            if (destination == null)
            {
                return NotFound();
            }

            string? userId = User.Identity?.IsAuthenticated == true
                ? GetUserId()
                : null;

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