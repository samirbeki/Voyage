using Microsoft.AspNetCore.Mvc;
using Voyage.Common.Constants;
using Voyage.Common.Messages;
using Voyage.Services.Contracts;

namespace Voyage.Web.Areas.Admin.Controllers
{
    public class TravelExpertsController : AdminBaseController
    {
        private readonly ITravelExpertService travelExpertService;

        public TravelExpertsController(ITravelExpertService travelExpertService)
        {
            this.travelExpertService = travelExpertService;
        }

        public async Task<IActionResult> Index(string? search)
        {
            var model = await travelExpertService.GetUsersAsync(search);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Grant(string id, string? search)
        {
            var email = string.IsNullOrWhiteSpace(id)
                ? null
                : await travelExpertService.GrantAsync(id);

            SetResultMessage(email, SuccessMessages.TravelExpertGranted);

            return RedirectToAction(nameof(Index), new { search });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(string id, string? search)
        {
            var email = string.IsNullOrWhiteSpace(id)
                ? null
                : await travelExpertService.RemoveAsync(id);

            SetResultMessage(email, SuccessMessages.TravelExpertRemoved);

            return RedirectToAction(nameof(Index), new { search });
        }

        private void SetResultMessage(string? email, string successMessage)
        {
            if (email == null)
            {
                TempData[TempDataKeys.ErrorMessage] = ErrorMessages.TravelExpertChangeFailed;
            }
            else
            {
                TempData[TempDataKeys.SuccessMessage] = string.Format(successMessage, email);
            }
        }
    }
}
