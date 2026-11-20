using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Voyage.Web.Common;
using Voyage.Web.Services.Contracts;
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
                    TempData["SuccessMessage"] = string.Format(
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
                            ValidationConstants.PackingItemsMaxPerTrip));
                    break;
            }

            return await RedisplayAsync(tripId, form);
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