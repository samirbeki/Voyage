using Microsoft.AspNetCore.Mvc;
using Voyage.Web.Services.Contracts;

namespace Voyage.Web.Controllers
{
    public class CategoriesController : BaseController
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
    }
}