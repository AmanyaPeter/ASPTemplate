using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartBreadcrumbs.Attributes;
using Template.Common.Static;
using Template.Core.Models.Dashboard;
using Template.Core.Repository.Dashboard;
using Template.Web.Models;

namespace Template.Web.Controllers;

[Authorize]
[DefaultBreadcrumb]
public class HomeController(
    ILogger<HomeController> logger,
    IDashboardRepository dashboardRepository) : Controller
{
    public async Task<IActionResult> Index()
    {
        var model = new DashboardPageViewModel();

        if (User.IsInRole(RoleConstants.ItAdmin))
        {
            model.ItAdmin = await dashboardRepository.GetItAdminDashboardAsync();
        }
        else if (User.IsInRole(RoleConstants.InnovationTeam))
        {
            model.InnovationTeam = await dashboardRepository.GetInnovationTeamDashboardAsync();
        }
        else if (User.IsInRole(RoleConstants.Staff))
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdValue, out var userId))
            {
                logger.LogWarning("Authenticated Staff user has no valid NameIdentifier claim.");
                return Forbid();
            }

            model.Staff = await dashboardRepository.GetStaffDashboardAsync(userId);
        }

        return View(model);
    }

    public IActionResult TestPage() => View();

    [Breadcrumb("UI Kit", FromAction = nameof(Index))]
    public IActionResult UiKit() => View();

    [Breadcrumb("Privacy", FromAction = nameof(Index))]
    public IActionResult Privacy() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        logger.LogError("Unhandled request error. Trace identifier {TraceId}", HttpContext.TraceIdentifier);
        return View(new ErrorViewModel
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
        });
    }
}
