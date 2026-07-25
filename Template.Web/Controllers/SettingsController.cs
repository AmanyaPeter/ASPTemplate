using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartBreadcrumbs.Attributes;
using Template.Common.Static;

namespace Template.Web.Controllers;

[Authorize(Roles = RoleConstants.ItAdmin)]
public class SettingsController : Controller
{
    [Breadcrumb("System Settings", FromAction = nameof(Index), FromController = typeof(HomeController))]
    public IActionResult Index() => View();

    [Breadcrumb("Help & Support", FromAction = nameof(Index), FromController = typeof(HomeController))]
    public IActionResult Support() => View();
}
