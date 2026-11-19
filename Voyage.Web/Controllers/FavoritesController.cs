using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Voyage.Web.Common;
using Voyage.Web.Services.Contracts;
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
        public async Task<IActionResult> Add(int destinationId, string? returnUrl)
        {
            var result = await favoriteService.AddAsync(destinationId, GetUserId());

            if (result == FavoriteResult.DestinationNotFound)
            {
                return NotFound();
            }

            SetResultMessage(result, SuccessMessages.FavoriteAdded);

            return RedirectBack(returnUrl, destinationId);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(int destinationId, string? returnUrl)
        {
            var result = await favoriteService.RemoveAsync(destinationId, GetUserId());

            SetResultMessage(result, SuccessMessages.FavoriteRemoved);

            return RedirectBack(returnUrl, destinationId);
        }

        private void SetResultMessage(FavoriteResult result, string successMessage)
        {
            switch (result)
            {
                case FavoriteResult.Success:
                    TempData[TempDataKeys.SuccessMessage] = successMessage;
                    break;

                case FavoriteResult.AlreadyExists:
                    TempData[TempDataKeys.ErrorMessage] =
                        ErrorMessages.FavoriteAlreadyExists;
                    break;

                case FavoriteResult.NotInFavorites:
                    TempData[TempDataKeys.ErrorMessage] =
                        ErrorMessages.FavoriteNotFound;
                    break;

                case FavoriteResult.LimitReached:
                    TempData[TempDataKeys.ErrorMessage] =
                        string.Format(
                            ErrorMessages.FavoritesLimitReached,
                            ValidationConstants.FavoritesMaxPerUser);
                    break;
            }
        }

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