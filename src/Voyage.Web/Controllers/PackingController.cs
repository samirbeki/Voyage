using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Voyage.Common.Constants;
using Voyage.Common.Constants.Validation;
using Voyage.Common.Messages;
using Voyage.Data.Models.Enums;
using Voyage.Services.Contracts;
using Voyage.Services.Results;
using Voyage.Web.ViewModels.Packing;

namespace Voyage.Web.Controllers
{
    [Authorize]
    public class PackingController : BaseController
    {
        private readonly IPackingService packingService;

        public PackingController(IPackingService packingService)
        {
            this.packingService = packingService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(
            int tripId,
            PackingCategory? category)
        {
            var model = await packingService.GetListAsync(
                tripId,
                GetUserId());

            if (model == null)
            {
                return NotFound();
            }

            if (category.HasValue &&
                Enum.IsDefined(typeof(PackingCategory), category.Value))
            {
                model.NewItem.Category = category.Value;
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(
            int tripId,
            [Bind(Prefix = "NewItem")] PackingItemFormModel form)
        {
            if (!ModelState.IsValid)
            {
                return await RedisplayAsync(tripId, form);
            }

            var result = await packingService.AddAsync(
                tripId,
                form,
                GetUserId());

            switch (result)
            {
                case PackingResult.Success:
                    TempData[TempDataKeys.SuccessMessage] = string.Format(
                        SuccessMessages.PackingItemAdded,
                        form.Name.Trim());

                    var url = Url.Action(
                        nameof(Index),
                        new
                        {
                            tripId,
                            category = form.Category
                        });

                    return Redirect($"{url}#add-item");

                case PackingResult.TripNotFound:
                    return NotFound();

                case PackingResult.Duplicate:
                    ModelState.AddModelError(
                        "NewItem.Name",
                        ErrorMessages.PackingItemDuplicate);
                    break;

                case PackingResult.InvalidCategory:
                    ModelState.AddModelError(
                        "NewItem.Category",
                        ErrorMessages.PackingItemCategoryInvalid);
                    break;

                case PackingResult.LimitReached:
                    ModelState.AddModelError(
                        string.Empty,
                        string.Format(
                            ErrorMessages.PackingItemsLimitReached,
                            PackingItemValidationConstants.MaxPerTrip));
                    break;
            }

            return await RedisplayAsync(tripId, form);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Toggle([FromRoute] int id)
        {
            var result = await packingService.TogglePackedAsync(
                id,
                GetUserId());

            if (!result.Succeeded)
            {
                return NotFound();
            }

            if (IsAjaxRequest())
            {
                var progress = await packingService.GetProgressAsync(
                    result.TripId);

                return Ok(new PackingToggleResponse
                {
                    ItemId = id,
                    IsPacked = result.IsPacked,
                    Packed = progress.Packed,
                    Total = progress.Total,
                    Percent = progress.Percent,
                    IsComplete = progress.IsComplete
                });
            }

            return RedirectToAction(
                nameof(Index),
                "Packing",
                new { tripId = result.TripId },
                $"item-{id}");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var model = await packingService.GetForEditAsync(
                id,
                GetUserId());

            if (model == null)
            {
                return NotFound();
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            [FromRoute] int id,
            PackingItemEditViewModel model)
        {
            string userId = GetUserId();

            var current = await packingService.GetForEditAsync(
                id,
                userId);

            if (current == null)
            {
                return NotFound();
            }

            model.Id = id;
            model.TripId = current.TripId;
            model.TripName = current.TripName;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await packingService.EditAsync(
                id,
                model,
                userId);

            switch (result.Result)
            {
                case PackingResult.Success:
                    TempData[TempDataKeys.SuccessMessage] =
                        string.Format(
                            SuccessMessages.PackingItemUpdated,
                            model.Name.Trim());

                    return RedirectToAction(
                        nameof(Index),
                        "Packing",
                        new { tripId = result.TripId },
                        $"item-{id}");

                case PackingResult.ItemNotFound:
                    return NotFound();

                case PackingResult.Duplicate:
                    ModelState.AddModelError(
                        nameof(model.Name),
                        ErrorMessages.PackingItemDuplicate);
                    break;

                case PackingResult.InvalidCategory:
                    ModelState.AddModelError(
                        nameof(model.Category),
                        ErrorMessages.PackingItemCategoryInvalid);
                    break;
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            var result = await packingService.DeleteAsync(
                id,
                GetUserId());

            if (!result.Succeeded)
            {
                return NotFound();
            }

            TempData[TempDataKeys.SuccessMessage] =
                SuccessMessages.PackingItemDeleted;

            return RedirectToAction(
                nameof(Index),
                "Packing",
                new { tripId = result.TripId },
                "add-item");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddEssentials(int tripId)
        {
            var result = await packingService.AddEssentialsAsync(
                tripId,
                GetUserId());

            switch (result.Result)
            {
                case PackingResult.TripNotFound:
                    return NotFound();

                case PackingResult.LimitReached:
                    TempData[TempDataKeys.ErrorMessage] =
                        string.Format(
                            ErrorMessages.PackingItemsLimitReached,
                            PackingItemValidationConstants.MaxPerTrip);
                    break;

                case PackingResult.Duplicate:
                    TempData[TempDataKeys.ErrorMessage] =
                        ErrorMessages.PackingConcurrentChange;
                    break;

                case PackingResult.Success when result.Affected == 0:
                    TempData[TempDataKeys.SuccessMessage] =
                        SuccessMessages.EssentialsAlreadyAdded;
                    break;

                case PackingResult.Success when result.NotAddedDueToLimit > 0:
                    TempData[TempDataKeys.ErrorMessage] =
                        string.Format(
                            ErrorMessages.EssentialsPartiallyAdded,
                            result.Affected,
                            result.NotAddedDueToLimit,
                            PackingItemValidationConstants.MaxPerTrip);
                    break;

                case PackingResult.Success:
                    TempData[TempDataKeys.SuccessMessage] =
                        string.Format(
                            SuccessMessages.EssentialsAdded,
                            result.Affected);
                    break;
            }

            return RedirectToAction(
                nameof(Index),
                "Packing",
                new { tripId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnpackAll(int tripId)
        {
            var result = await packingService.UnpackAllAsync(
                tripId,
                GetUserId());

            if (!result.Succeeded)
            {
                return NotFound();
            }

            TempData[TempDataKeys.SuccessMessage] = result.Affected == 0
                ? SuccessMessages.PackingNothingToUnpack
                : string.Format(
                    SuccessMessages.PackingUnpackedAll,
                    result.Affected);

            return RedirectToAction(
                nameof(Index),
                "Packing",
                new { tripId });
        }

        private async Task<IActionResult> RedisplayAsync(
            int tripId,
            PackingItemFormModel form)
        {
            var model = await packingService.GetListAsync(
                tripId,
                GetUserId());

            if (model == null)
            {
                return NotFound();
            }

            model.NewItem = form;

            return View("Index", model);
        }
    }
}