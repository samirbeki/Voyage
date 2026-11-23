using Microsoft.AspNetCore.Mvc;
using Voyage.Common.Constants;
using Voyage.Common.Messages;
using Voyage.Web.Services.Contracts;
using Voyage.Web.ViewModels.Destinations;
using Voyage.Web.ViewModels.Enums;

namespace Voyage.Web.Areas.Admin.Controllers
{
    public class DestinationsController : AdminBaseController
    {
        private readonly IDestinationService destinationService;

        public DestinationsController(IDestinationService destinationService)
        {
            this.destinationService = destinationService;
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

        [HttpGet]
        public async Task<IActionResult> Add()
        {
            var model = new DestinationFormViewModel
            {
                Categories = await destinationService.GetCategoriesAsync()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(DestinationFormViewModel model)
        {
            if (model.CategoryId > 0 &&
                !await destinationService.CategoryExistsAsync(model.CategoryId))
            {
                ModelState.AddModelError(
                    nameof(model.CategoryId),
                    ErrorMessages.CategoryNotFound);
            }

            if (!string.IsNullOrWhiteSpace(model.Name) &&
                !string.IsNullOrWhiteSpace(model.Country) &&
                await destinationService.DestinationExistsAsync(
                    model.Name,
                    model.Country))
            {
                ModelState.AddModelError(
                    nameof(model.Name),
                    ErrorMessages.DestinationExists);
            }

            if (!ModelState.IsValid)
            {
                model.Categories = await destinationService.GetCategoriesAsync();

                return View(model);
            }

            await destinationService.AddAsync(model);

            TempData[TempDataKeys.SuccessMessage] = SuccessMessages.DestinationAdded;

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var model = await destinationService.GetForEditAsync(id);

            if (model == null)
            {
                return NotFound();
            }

            model.Categories = await destinationService.GetCategoriesAsync();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DestinationFormViewModel model)
        {
            if (model.CategoryId > 0 &&
                !await destinationService.CategoryExistsAsync(model.CategoryId))
            {
                ModelState.AddModelError(
                    nameof(model.CategoryId),
                    ErrorMessages.CategoryNotFound);
            }

            if (!string.IsNullOrWhiteSpace(model.Name) &&
                !string.IsNullOrWhiteSpace(model.Country) &&
                await destinationService.DestinationExistsAsync(
                    model.Name,
                    model.Country,
                    id))
            {
                ModelState.AddModelError(
                    nameof(model.Name),
                    ErrorMessages.DestinationExists);
            }

            if (!ModelState.IsValid)
            {
                model.Categories = await destinationService.GetCategoriesAsync();

                return View(model);
            }

            var success = await destinationService.EditAsync(id, model);

            if (!success)
            {
                return NotFound();
            }

            TempData[TempDataKeys.SuccessMessage] = SuccessMessages.DestinationUpdated;

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var model = await destinationService.GetDetailsAsync(id);

            if (model == null)
            {
                return NotFound();
            }

            return View(model);
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var success = await destinationService.DeleteAsync(id);

            if (!success)
            {
                return NotFound();
            }

            TempData[TempDataKeys.SuccessMessage] = SuccessMessages.DestinationDeleted;

            return RedirectToAction(nameof(Index));
        }
    }
}