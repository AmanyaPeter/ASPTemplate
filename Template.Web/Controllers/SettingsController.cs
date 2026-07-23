using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartBreadcrumbs.Attributes;

namespace Template.Web.Controllers;

[Authorize]
public class SettingsController : Controller
{
    [Breadcrumb("System Settings", FromAction = nameof(Index), FromController = typeof(HomeController))]
    public IActionResult Index() => User.IsInRole("Admin") ? View() : Forbid();

    [Breadcrumb("Help & Support", FromAction = nameof(Index), FromController = typeof(HomeController))]
    public IActionResult Support() => View();
}
