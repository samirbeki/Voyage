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
    }
}