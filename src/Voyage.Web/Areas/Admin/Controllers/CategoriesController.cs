using Microsoft.AspNetCore.Mvc;
using Voyage.Common.Constants;
using Voyage.Common.Messages;
using Voyage.Services.Contracts;
using Voyage.Web.ViewModels.Categories;

namespace Voyage.Web.Areas.Admin.Controllers
{
    public class CategoriesController : AdminBaseController
    {
        private readonly ICategoryService categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            this.categoryService = categoryService;
        }

        public async Task<IActionResult> Index()
        {
            var categories = await categoryService.GetAllAsync();

            return View(categories);
        }

        [HttpGet]
        public IActionResult Add()
        {
            return View(new CategoryFormViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(CategoryFormViewModel model)
        {
            if (!string.IsNullOrWhiteSpace(model.Name)
                && await categoryService.ExistsByNameAsync(model.Name))
            {
                ModelState.AddModelError(
                    nameof(model.Name),
                    ErrorMessages.CategoryExists);
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            await categoryService.AddAsync(model);

            TempData[TempDataKeys.SuccessMessage] = SuccessMessages.CategoryAdded;

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var model = await categoryService.GetForEditAsync(id);

            if (model == null)
            {
                return NotFound();
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CategoryFormViewModel model)
        {
            if (!string.IsNullOrWhiteSpace(model.Name)
                && await categoryService.ExistsByNameAsync(model.Name, id))
            {
                ModelState.AddModelError(
                    nameof(model.Name),
                    ErrorMessages.CategoryExists);
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var success = await categoryService.EditAsync(id, model);

            if (!success)
            {
                return NotFound();
            }

            TempData[TempDataKeys.SuccessMessage] = SuccessMessages.CategoryUpdated;

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var model = await categoryService.GetByIdAsync(id);

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
            if (await categoryService.HasDestinationsAsync(id))
            {
                var model = await categoryService.GetByIdAsync(id);

                if (model == null)
                {
                    return NotFound();
                }

                ModelState.AddModelError(
                    string.Empty,
                    ErrorMessages.CategoryHasDestinations);

                return View("Delete", model);
            }

            var success = await categoryService.DeleteAsync(id);

            if (!success)
            {
                return NotFound();
            }

            TempData[TempDataKeys.SuccessMessage] = SuccessMessages.CategoryDeleted;

            return RedirectToAction(nameof(Index));
        }
    }
}