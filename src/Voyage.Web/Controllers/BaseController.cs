using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Voyage.Web.Common;

namespace Voyage.Web.Controllers
{
    public class BaseController : Controller
    {
        protected string GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        }

        protected bool IsAjaxRequest()
        {
            return Request.Headers.XRequestedWith == "XMLHttpRequest";
        }

        protected void ValidateMoney(string key, decimal? value, string decimalsMessage)
        {
            var rawValue = ModelState[key]?.AttemptedValue;

            if (!string.IsNullOrEmpty(rawValue) && rawValue.Contains(','))
            {
                ModelState.AddModelError(key, ErrorMessages.DecimalFormat);
            }
            else if (value.HasValue && decimal.Round(value.Value, 2) != value.Value)
            {
                ModelState.AddModelError(key, decimalsMessage);
            }
        }
    }
}