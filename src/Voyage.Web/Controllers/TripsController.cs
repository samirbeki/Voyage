using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Voyage.Common.Constants;
using Voyage.Common.Messages;
using Voyage.Services.Contracts;
using Voyage.Web.ViewModels.Trips;

namespace Voyage.Web.Controllers
{
    [Authorize]
    public class TripsController : BaseController
    {
        private readonly ITripService tripService;
        private readonly IPackingService packingService;
        private readonly ITripSharingService tripSharingService;
        private readonly ITripWeatherService tripWeatherService;

        public TripsController(
            ITripService tripService,
            IPackingService packingService,
            ITripSharingService tripSharingService,
            ITripWeatherService tripWeatherService)
        {
            this.tripService = tripService;
            this.packingService = packingService;
            this.tripSharingService = tripSharingService;
            this.tripWeatherService = tripWeatherService;
        }

        public async Task<IActionResult> Index()
        {
            var userId = GetUserId();

            var trips = await tripService.GetMineAsync(userId);

            return View(trips);
        }

        [HttpGet]
        public async Task<IActionResult> Create(int? destinationId)
        {
            var model = new TripFormViewModel
            {
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(7),
                AvailableDestinations =
                    await tripService.GetDestinationOptionsAsync()
            };

            if (destinationId.HasValue
                && model.AvailableDestinations.Any(d => d.Id == destinationId))
            {
                model.SelectedDestinationIds.Add(destinationId.Value);
            }

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

            if (model.StartDate.Date < DateTime.Today)
            {
                ModelState.AddModelError(
                    nameof(model.StartDate),
                    ErrorMessages.StartDateInPast);
            }

            ValidateMoney(
                nameof(model.Budget),
                model.Budget,
                ErrorMessages.TripBudgetDecimals);

            if (!ModelState.IsValid)
            {
                model.AvailableDestinations =
                    await tripService.GetDestinationOptionsAsync();

                return View(model);
            }

            var tripId = await tripService.CreateAsync(model, GetUserId());

            TempData[TempDataKeys.SuccessMessage] = SuccessMessages.TripCreated;

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

            model.PackingProgress =
                await packingService.GetProgressAsync(id);

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Weather(int id)
        {
            var model = await tripWeatherService.GetForTripAsync(id, GetUserId());

            if (model == null)
            {
                return NotFound();
            }

            if (model.IsFinished || (!model.IsTooEarly && !model.Destinations.Any()))
            {
                return NoContent();
            }

            return PartialView("_TripWeather", model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var model = await tripService.GetForEditAsync(id, GetUserId());

            if (model == null)
            {
                return NotFound();
            }

            model.AvailableDestinations =
                await tripService.GetDestinationOptionsAsync();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([FromRoute] int id, TripFormViewModel model)
        {
            if (model.SelectedDestinationIds.Any()
                && !await tripService.DestinationsExistAsync(
                    model.SelectedDestinationIds))
            {
                ModelState.AddModelError(
                    nameof(model.SelectedDestinationIds),
                    ErrorMessages.DestinationNotFound);
            }

            if (await tripService.HasExpensesOutsideAsync(
                id,
                model.StartDate,
                model.EndDate))
            {
                ModelState.AddModelError(
                    nameof(model.EndDate),
                    ErrorMessages.TripDatesExcludeExpenses);
            }

            ValidateMoney(
                nameof(model.Budget),
                model.Budget,
                ErrorMessages.TripBudgetDecimals);

            if (!ModelState.IsValid)
            {
                model.AvailableDestinations =
                    await tripService.GetDestinationOptionsAsync();

                return View(model);
            }

            var success = await tripService.EditAsync(
                id,
                model,
                GetUserId());

            if (!success)
            {
                return NotFound();
            }

            TempData[TempDataKeys.SuccessMessage] = SuccessMessages.TripUpdated;

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var model = await tripService.GetDetailsAsync(id, GetUserId());

            if (model == null)
            {
                return NotFound();
            }

            return View(model);
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed([FromRoute] int id)
        {
            var success = await tripService.DeleteAsync(id, GetUserId());

            if (!success)
            {
                return NotFound();
            }

            TempData[TempDataKeys.SuccessMessage] = SuccessMessages.TripDeleted;

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Share(int id)
        {
            var token = await tripSharingService.ShareAsync(id, GetUserId());

            if (token == null)
            {
                return NotFound();
            }

            TempData[TempDataKeys.SuccessMessage] = SuccessMessages.TripShared;

            return RedirectToAction(nameof(Details), null, new { id }, "share");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegenerateShareLink(int id)
        {
            var token = await tripSharingService.RegenerateAsync(id, GetUserId());

            if (token == null)
            {
                return NotFound();
            }

            TempData[TempDataKeys.SuccessMessage] = SuccessMessages.TripShareLinkRegenerated;

            return RedirectToAction(nameof(Details), null, new { id }, "share");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StopSharing(int id)
        {
            var success = await tripSharingService.StopSharingAsync(id, GetUserId());

            if (!success)
            {
                return NotFound();
            }

            TempData[TempDataKeys.SuccessMessage] = SuccessMessages.TripSharingStopped;

            return RedirectToAction(nameof(Details), null, new { id }, "share");
        }
    }
}