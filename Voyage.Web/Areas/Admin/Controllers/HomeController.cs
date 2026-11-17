using Microsoft.AspNetCore.Mvc;

namespace Voyage.Web.Areas.Admin.Controllers
{
    public class HomeController : AdminBaseController
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}