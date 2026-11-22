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
            if (!await reviewService.DestinationExistsAsync(destinationId))
            {
                return NotFound();
            }

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

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var model = await reviewService.GetForEditAsync(id, GetUserId());

            if (model == null)
            {
                return NotFound();
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            [FromRoute] int id, ReviewEditViewModel model)
        {
            var current = await reviewService.GetForEditAsync(id, GetUserId());

            if (current == null)
            {
                return NotFound();
            }

            model.Id = current.Id;
            model.DestinationId = current.DestinationId;
            model.DestinationName = current.DestinationName;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var destinationId = await reviewService.EditAsync(
                id,
                model,
                GetUserId());

            if (destinationId == null)
            {
                return NotFound();
            }

            TempData[TempDataKeys.SuccessMessage] =
                SuccessMessages.ReviewUpdated;

            return RedirectToDetails(destinationId.Value);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var model = await reviewService.GetForEditAsync(id, GetUserId());

            if (model == null)
            {
                return NotFound();
            }

            return View(model);
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed([FromRoute] int id)
        {
            var destinationId = await reviewService.DeleteAsync(
                id,
                GetUserId());

            if (destinationId == null)
            {
                return NotFound();
            }

            TempData[TempDataKeys.SuccessMessage] =
                SuccessMessages.ReviewDeleted;

            return RedirectToDetails(destinationId.Value);
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