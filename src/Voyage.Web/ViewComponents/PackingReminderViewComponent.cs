using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Voyage.Services.Contracts;

namespace Voyage.Web.ViewComponents
{
    public class PackingReminderViewComponent : ViewComponent
    {
        private readonly IPackingService packingService;

        public PackingReminderViewComponent(IPackingService packingService)
        {
            this.packingService = packingService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            string? userId = UserClaimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userId))
            {
                return Content(string.Empty);
            }

            var reminders = await packingService.GetRemindersAsync(userId);

            if (!reminders.Any())
            {
                return Content(string.Empty);
            }

            return View(reminders);
        }
    }
}
