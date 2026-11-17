using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Voyage.Web.Common;
using Voyage.Web.Services.Contracts;
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
            if (!await expenseService.IsDateWithinTripAsync(
                model.TripId,
                model.Date))
            {
                ModelState.AddModelError(
                    nameof(model.Date),
                    ErrorMessages.ExpenseDateOutsideTrip);
            }

            if (!ModelState.IsValid)
            {
                var form = await expenseService.GetAddFormAsync(
                    model.TripId,
                    GetUserId());

                if (form == null)
                {
                    return NotFound();
                }

                model.TripTitle = form.TripTitle;
                model.TripStartDate = form.TripStartDate;
                model.TripEndDate = form.TripEndDate;

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
    }
}