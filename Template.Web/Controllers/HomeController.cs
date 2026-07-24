using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartBreadcrumbs.Attributes;
using Template.Common.Static;
using Template.Web.Models;
using Template.Core.Models.Dashboard;
using Template.Core.Repository.Dashboard;

namespace Template.Web.Controllers;

[Authorize]
[DefaultBreadcrumb]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IDashboardRepository _dashboardRepository;

    public HomeController(
        ILogger<HomeController> logger,
        IDashboardRepository dashboardRepository)
    {
        _logger = logger;
        _dashboardRepository = dashboardRepository;
    }

    public async Task<IActionResult> Index()
    {
        var model = new DashboardPageViewModel();

        if (User.IsInRole(RoleConstants.ItAdmin))
        {
            model.ItAdmin = await _dashboardRepository.GetItAdminDashboardAsync();
        }
        else if (User.IsInRole(RoleConstants.InnovationTeam))
        {
            model.InnovationTeam =
                await _dashboardRepository.GetInnovationTeamDashboardAsync();
        }
        else if (User.IsInRole(RoleConstants.Staff))
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdValue, out var userId))
            {
                _logger.LogWarning(
                    "Authenticated Staff user has no valid NameIdentifier claim.");
                return Forbid();
            }

            model.Staff = await _dashboardRepository.GetStaffDashboardAsync(userId);
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
        _logger.LogError("Unhandled request error. Trace identifier {TraceId}", HttpContext.TraceIdentifier);
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
