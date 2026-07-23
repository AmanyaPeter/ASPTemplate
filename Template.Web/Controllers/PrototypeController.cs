using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Template.Web.Models.Prototype;

namespace Template.Web.Controllers;

[Authorize]
public sealed class PrototypeController : Controller
{
    [HttpGet]
    public IActionResult Index() => View(new PrototypePageViewModel());

    [HttpGet]
    public IActionResult Forms() => View(new PrototypePageViewModel());
}
