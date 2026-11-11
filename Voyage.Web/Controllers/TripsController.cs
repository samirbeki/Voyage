using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Voyage.Web.Common;
using Voyage.Web.Services.Contracts;
using Voyage.Web.ViewModels.Trips;

namespace Voyage.Web.Controllers
{
    [Authorize]
    public class TripsController : Controller
    {
        private readonly ITripService tripService;

        public TripsController(ITripService tripService)
        {
            this.tripService = tripService;
        }

        public async Task<IActionResult> Index()
        {
            var userId = GetUserId();

            var trips = await tripService.GetMineAsync(userId);

            return View(trips);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new TripFormViewModel
            {
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(7),
                AvailableDestinations =
                    await tripService.GetDestinationOptionsAsync()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TripFormViewModel model)
        {
            if (model.SelectedDestinationIds.Any()
                && !await tripService.DestinationsExistAsync(
                    model.SelectedDestinationIds))
            {
                ModelState.AddModelError(
                    nameof(model.SelectedDestinationIds),
                    ErrorMessages.DestinationNotFound);
            }

            if (!ModelState.IsValid)
            {
                model.AvailableDestinations =
                    await tripService.GetDestinationOptionsAsync();

                return View(model);
            }

            var tripId = await tripService.CreateAsync(model, GetUserId());

            return RedirectToAction(nameof(Details), new { id = tripId });
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var userId = GetUserId();

            var model = await tripService.GetDetailsAsync(id, userId);

            if (model == null)
            {
                return NotFound();
            }

            return View(model);
        }

        private string GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        }
    }
}