using Microsoft.AspNetCore.Mvc;
using Voyage.Web.Services.Contracts;

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

    }
}