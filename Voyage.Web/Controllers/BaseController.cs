using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace Voyage.Web.Controllers
{
    public class BaseController : Controller
    {
        protected string GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        }
    }
}