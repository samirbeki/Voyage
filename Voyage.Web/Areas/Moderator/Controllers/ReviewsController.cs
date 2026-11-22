using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Voyage.Web.Common;
using Voyage.Web.Services.Contracts;

namespace Voyage.Web.Areas.Moderator.Controllers
{
    [Area("Moderator")]
    [Authorize(Roles = RoleConstants.Moderator + "," + RoleConstants.Administrator)]
    public class ReviewsController : Controller
    {
        private readonly IReviewService reviewService;

        public ReviewsController(IReviewService reviewService)
        {
            this.reviewService = reviewService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string status = "all", int page = 1)
        {
            var model = await reviewService.GetForModerationAsync(status, page);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Hide(int id, string status = "all", int page = 1)
        {
            return await ToggleVisibilityAsync(
                id,
                true,
                status,
                page,
                SuccessMessages.ReviewHidden);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Show(int id, string status = "all", int page = 1)
        {
            return await ToggleVisibilityAsync(
                id,
                false,
                status,
                page,
                SuccessMessages.ReviewShown);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, string status = "all", int page = 1)
        {
            bool deleted = await reviewService.DeleteByModeratorAsync(id);

            if (deleted)
            {
                TempData[TempDataKeys.SuccessMessage] =
                    SuccessMessages.ReviewDeletedByModerator;
            }
            else
            {
                TempData[TempDataKeys.ErrorMessage] =
                    ErrorMessages.ReviewNoLongerExists;
            }

            return RedirectToAction(nameof(Index), new { status, page });
        }

        private async Task<IActionResult> ToggleVisibilityAsync(
            int id,
            bool isHidden,
            string status,
            int page,
            string successMessage)
        {
            bool updated = await reviewService.SetHiddenAsync(id, isHidden);

            if (updated)
            {
                TempData[TempDataKeys.SuccessMessage] = successMessage;
            }
            else
            {
                TempData[TempDataKeys.ErrorMessage] =
                    ErrorMessages.ReviewNoLongerExists;
            }

            return RedirectToAction(nameof(Index), new { status, page });
        }
    }
}