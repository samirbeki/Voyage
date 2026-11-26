using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Voyage.Common.Constants;
using Voyage.Common.Messages;
using Voyage.Services.Contracts;

namespace Voyage.Web.Areas.Moderator.Controllers
{
    [Area("Moderator")]
    [Authorize(Roles = RoleConstants.Moderator + "," + RoleConstants.Administrator)]
    public class ExpertTipsController : Controller
    {
        private readonly IExpertTipService expertTipService;

        public ExpertTipsController(IExpertTipService expertTipService)
        {
            this.expertTipService = expertTipService;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, int destinationId)
        {
            var tipDestinationId = await expertTipService.DeleteByModeratorAsync(id);

            if (tipDestinationId == null)
            {
                TempData[TempDataKeys.ErrorMessage] = ErrorMessages.ExpertTipNoLongerExists;
            }
            else
            {
                TempData[TempDataKeys.SuccessMessage] = SuccessMessages.ExpertTipDeletedByModerator;
                destinationId = tipDestinationId.Value;
            }

            return RedirectToAction(
                "Details",
                "Destinations",
                new { area = "", id = destinationId },
                "expert-tips");
        }
    }
}
