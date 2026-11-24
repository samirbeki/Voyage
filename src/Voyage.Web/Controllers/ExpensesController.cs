using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Voyage.Common.Constants;
using Voyage.Common.Constants.Validation;
using Voyage.Common.Messages;
using Voyage.Common.Utilities;
using Voyage.Services.Contracts;
using Voyage.Web.ViewModels.Expenses;

namespace Voyage.Web.Controllers
{
    [Authorize]
    public class ExpensesController : BaseController
    {
        private readonly IExpenseService expenseService;

        public ExpensesController(IExpenseService expenseService)
        {
            this.expenseService = expenseService;
        }

        [HttpGet]
        public async Task<IActionResult> Add(int tripId)
        {
            var model = await expenseService.GetAddFormAsync(
                tripId,
                GetUserId());

            if (model == null)
            {
                return NotFound();
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(ExpenseFormViewModel model)
        {
            var form = await expenseService.GetAddFormAsync(
                model.TripId,
                GetUserId());

            if (form == null)
            {
                return NotFound();
            }

            CopyTripInfo(form, model);

            await ValidateExpenseAsync(model);

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var success = await expenseService.AddAsync(
                model,
                GetUserId());

            if (!success)
            {
                return NotFound();
            }

            TempData[TempDataKeys.SuccessMessage] = SuccessMessages.ExpenseAdded;

            return RedirectToAction(
                "Details",
                "Trips",
                new { id = model.TripId });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var model = await expenseService.GetForEditAsync(
                id,
                GetUserId());

            if (model == null)
            {
                return NotFound();
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            [FromRoute] int id,
            ExpenseFormViewModel model)
        {
            var form = await expenseService.GetForEditAsync(
                id,
                GetUserId());

            if (form == null)
            {
                return NotFound();
            }

            CopyTripInfo(form, model);

            await ValidateExpenseAsync(model);

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var success = await expenseService.EditAsync(
                id,
                model,
                GetUserId());

            if (!success)
            {
                return NotFound();
            }

            TempData[TempDataKeys.SuccessMessage] = SuccessMessages.ExpenseUpdated;

            return RedirectToAction(
                "Details",
                "Trips",
                new { id = model.TripId });
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var model = await expenseService.GetForDeleteAsync(
                id,
                GetUserId());

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
            var tripId = await expenseService.DeleteAsync(
                id,
                GetUserId());

            if (tripId == null)
            {
                return NotFound();
            }

            TempData[TempDataKeys.SuccessMessage] = SuccessMessages.ExpenseDeleted;

            return RedirectToAction(
                "Details",
                "Trips",
                new { id = tripId });
        }

        private static void CopyTripInfo(
            ExpenseFormViewModel source,
            ExpenseFormViewModel target)
        {
            target.TripId = source.TripId;
            target.TripTitle = source.TripTitle;
            target.TripStartDate = source.TripStartDate;
            target.TripEndDate = source.TripEndDate;
        }

        private async Task ValidateExpenseAsync(ExpenseFormViewModel model)
        {
            model.Title = TextNormalizer.SingleLine(model.Title);

            if (IsFieldValid(nameof(model.Title))
                && model.Title.Length < ExpenseValidationConstants.TitleMinLength)
            {
                ModelState.AddModelError(
                    nameof(model.Title),
                    string.Format(
                        ErrorMessages.ExpenseTitleLength,
                        nameof(model.Title),
                        ExpenseValidationConstants.TitleMaxLength,
                        ExpenseValidationConstants.TitleMinLength));
            }

            ValidateMoney(
                nameof(model.Amount),
                model.Amount,
                ErrorMessages.ExpenseAmountDecimals);

            if (IsFieldValid(nameof(model.Date))
                && !await expenseService.IsDateWithinTripAsync(
                    model.TripId,
                    model.Date))
            {
                ModelState.AddModelError(
                    nameof(model.Date),
                    ErrorMessages.ExpenseDateOutsideTrip);
            }
        }

        private bool IsFieldValid(string key)
        {
            return ModelState.GetFieldValidationState(key)
                != ModelValidationState.Invalid;
        }
    }
}
