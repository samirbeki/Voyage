using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Voyage.Web.Common;
using Voyage.Web.Services.Contracts;
using Voyage.Web.ViewModels.Reviews;

namespace Voyage.Web.Controllers
{
    [Authorize]
    public class ReviewsController : BaseController
    {
        private readonly IReviewService reviewService;

        public ReviewsController(IReviewService reviewService)
        {
            this.reviewService = reviewService;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            int destinationId,
            ReviewFormModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData[TempDataKeys.ErrorMessage] =
                    ErrorMessages.ReviewCouldNotBeSaved;

                return RedirectToDetails(destinationId);
            }

            var success = await reviewService.CreateAsync(
                destinationId,
                model,
                GetUserId());

            if (!success)
            {
                TempData[TempDataKeys.ErrorMessage] =
                    ErrorMessages.ReviewAlreadyExists;

                return RedirectToDetails(destinationId);
            }

            TempData[TempDataKeys.SuccessMessage] =
                SuccessMessages.ReviewCreated;

            return RedirectToDetails(destinationId);
        }

        private IActionResult RedirectToDetails(int destinationId)
        {
            return RedirectToAction(
                "Details",
                "Destinations",
                new { id = destinationId },
                "reviews");
        }
    }
}