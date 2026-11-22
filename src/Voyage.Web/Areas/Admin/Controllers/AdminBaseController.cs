using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Voyage.Common.Constants;

namespace Voyage.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = RoleConstants.Administrator)]
    public abstract class AdminBaseController : Controller
    {
    }
}