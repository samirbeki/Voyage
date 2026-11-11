using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Voyage.Web.Services.Contracts;

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

        private string GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        }
    }
}