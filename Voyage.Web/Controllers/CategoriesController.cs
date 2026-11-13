using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Voyage.Web.Common;
using Voyage.Web.Services.Contracts;
using Voyage.Web.ViewModels.Categories;

namespace Voyage.Web.Controllers
{
    public class CategoriesController : Controller
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

        [Authorize]
        [HttpGet]
        public IActionResult Add()
        {
            return View(new CategoryFormViewModel());
        }

        [Authorize]
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

            return RedirectToAction(nameof(Index));
        }

        [Authorize]
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

        [Authorize]
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

            return RedirectToAction(nameof(Index));
        }

        [Authorize]
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

        [Authorize]
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

            return RedirectToAction(nameof(Index));
        }
    }
}