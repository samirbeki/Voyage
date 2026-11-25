using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Voyage.Services.Contracts;

namespace Voyage.Web.Controllers
{
    [Authorize]
    public class StatisticsController : BaseController
    {
        private readonly IStatisticsService statisticsService;

        public StatisticsController(IStatisticsService statisticsService)
        {
            this.statisticsService = statisticsService;
        }

        public async Task<IActionResult> Index()
        {
            var model = await statisticsService.GetForUserAsync(GetUserId());

            return View(model);
        }
    }
}
