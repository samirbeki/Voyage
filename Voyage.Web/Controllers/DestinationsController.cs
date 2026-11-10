using Microsoft.AspNetCore.Mvc;
using Voyage.Web.Services.Contracts;
using Voyage.Web.ViewModels.Destinations;

namespace Voyage.Web.Controllers
{
    public class DestinationsController : Controller
    {
        private readonly IDestinationService destinationService;

        public DestinationsController(IDestinationService destinationService)
        {
            this.destinationService = destinationService;
        }

        public async Task<IActionResult> Index()
        {
            var destinations = await destinationService.GetAllAsync();

            return View(destinations);
        }

        public async Task<IActionResult> Details(int id)
        {
            var destination = await destinationService.GetDetailsAsync(id);

            if (destination == null)
            {
                return NotFound();
            }

            return View(destination);
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
            if (!ModelState.IsValid)
            {
                model.Categories = await destinationService.GetCategoriesAsync();

                return View(model);
            }

            await destinationService.AddAsync(model);

            return RedirectToAction(nameof(Index));
        }
    }
}
