using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Voyage.Services.Contracts;

namespace Voyage.Web.Controllers
{
    [AllowAnonymous]
    public class SharedTripsController : BaseController
    {
        private readonly ITripSharingService tripSharingService;

        public SharedTripsController(ITripSharingService tripSharingService)
        {
            this.tripSharingService = tripSharingService;
        }

        [HttpGet("/share/{token:guid}")]
        public async Task<IActionResult> Index(Guid token)
        {
            var model = await tripSharingService.GetSharedAsync(token);

            if (model == null)
            {
                return NotFound();
            }

            return View(model);
        }
    }
}
