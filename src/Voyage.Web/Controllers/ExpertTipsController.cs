using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Voyage.Common.Constants;
using Voyage.Common.Messages;
using Voyage.Services.Contracts;
using Voyage.Services.Results;
using Voyage.Web.ViewModels.ExpertTips;

namespace Voyage.Web.Controllers
{
    [Authorize(Roles = RoleConstants.TravelExpert)]
    public class ExpertTipsController : BaseController
    {
        private readonly IExpertTipService expertTipService;

        public ExpertTipsController(IExpertTipService expertTipService)
        {
            this.expertTipService = expertTipService;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int destinationId, ExpertTipFormModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData[TempDataKeys.ErrorMessage] = ErrorMessages.ExpertTipCouldNotBeSaved;

                return RedirectToDetails(destinationId);
            }

            var result = await expertTipService.CreateAsync(destinationId, model, GetUserId());

            switch (result)
            {
                case ExpertTipResult.DestinationNotFound:
                    return NotFound();
                case ExpertTipResult.NotExpert:
                    return Forbid();
                case ExpertTipResult.AlreadyExists:
                    TempData[TempDataKeys.ErrorMessage] = ErrorMessages.ExpertTipAlreadyExists;
                    break;
                default:
                    TempData[TempDataKeys.SuccessMessage] = SuccessMessages.ExpertTipCreated;
                    break;
            }

            return RedirectToDetails(destinationId);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var model = await expertTipService.GetForEditAsync(id, GetUserId());

            if (model == null)
            {
                return NotFound();
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([FromRoute] int id, ExpertTipEditViewModel model)
        {
            var current = await expertTipService.GetForEditAsync(id, GetUserId());

            if (current == null)
            {
                return NotFound();
            }

            model.Id = current.Id;
            model.DestinationId = current.DestinationId;
            model.DestinationName = current.DestinationName;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var destinationId = await expertTipService.EditAsync(id, model, GetUserId());

            if (destinationId == null)
            {
                return NotFound();
            }

            TempData[TempDataKeys.SuccessMessage] = SuccessMessages.ExpertTipUpdated;

            return RedirectToDetails(destinationId.Value);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var model = await expertTipService.GetForEditAsync(id, GetUserId());

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
            var destinationId = await expertTipService.DeleteAsync(id, GetUserId());

            if (destinationId == null)
            {
                return NotFound();
            }

            TempData[TempDataKeys.SuccessMessage] = SuccessMessages.ExpertTipDeleted;

            return RedirectToDetails(destinationId.Value);
        }

        private IActionResult RedirectToDetails(int destinationId)
        {
            return RedirectToAction(
                "Details",
                "Destinations",
                new { id = destinationId },
                "expert-tips");
        }
    }
}
