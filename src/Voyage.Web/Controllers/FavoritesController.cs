using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Voyage.Common.Constants;
using Voyage.Common.Constants.Validation;
using Voyage.Common.Messages;
using Voyage.Services.Contracts;
using Voyage.Services.Results;
using Voyage.Web.ViewModels.Favorites;

namespace Voyage.Web.Controllers
{
    [Authorize]
    public class FavoritesController : BaseController
    {
        private readonly IFavoriteService favoriteService;

        public FavoritesController(IFavoriteService favoriteService)
        {
            this.favoriteService = favoriteService;
        }

        [HttpGet]
        public async Task<IActionResult> Index([FromQuery] FavoritesQueryModel query)
        {
            if (!ModelState.IsValid)
            {
                query = new FavoritesQueryModel();
            }

            var model = await favoriteService.GetMineAsync(GetUserId(), query);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [EnableRateLimiting("favorites")]
        public Task<IActionResult> Add(int destinationId, string? returnUrl)
            => Toggle(destinationId, returnUrl, add: true);

        [HttpPost]
        [ValidateAntiForgeryToken]
        [EnableRateLimiting("favorites")]
        public Task<IActionResult> Remove(int destinationId, string? returnUrl)
            => Toggle(destinationId, returnUrl, add: false);

        private async Task<IActionResult> Toggle(
            int destinationId,
            string? returnUrl,
            bool add)
        {
            string userId = GetUserId();

            var result = add
                ? await favoriteService.AddAsync(destinationId, userId)
                : await favoriteService.RemoveAsync(destinationId, userId);

            if (result == FavoriteResult.DestinationNotFound)
            {
                return NotFound();
            }

            var (message, isError) = GetResultMessage(result, add);

            if (IsAjaxRequest())
            {
                int count = await favoriteService.GetCountForDestinationAsync(destinationId);

                var response = new FavoriteToggleResponse
                {
                    Success = !isError,
                    IsFavorite = await favoriteService.IsFavoriteAsync(destinationId, userId),
                    Count = count,
                    CountText = FavoriteTexts.SavedBy(count),
                    Message = message
                };

                return result == FavoriteResult.LimitReached
                    ? Conflict(response)
                    : Ok(response);
            }

            TempData[isError
                ? TempDataKeys.ErrorMessage
                : TempDataKeys.SuccessMessage] = message;

            return RedirectBack(returnUrl, destinationId);
        }

        private static (string Message, bool IsError) GetResultMessage(
            FavoriteResult result,
            bool add)
            => result switch
            {
                FavoriteResult.Success =>
                    (add ? SuccessMessages.FavoriteAdded : SuccessMessages.FavoriteRemoved, false),

                FavoriteResult.AlreadyExists =>
                    (ErrorMessages.FavoriteAlreadyExists, true),

                FavoriteResult.NotInFavorites =>
                    (ErrorMessages.FavoriteNotFound, true),

                FavoriteResult.LimitReached =>
                    (string.Format(
                        ErrorMessages.FavoritesLimitReached,
                        FavoriteValidationConstants.MaxPerUser), true),

                _ => throw new ArgumentOutOfRangeException(nameof(result))
            };

        private IActionResult RedirectBack(string? returnUrl, int destinationId)
        {
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }

            return RedirectToAction(
                "Details",
                "Destinations",
                new { id = destinationId, area = "" });
        }
    }
}