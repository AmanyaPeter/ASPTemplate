using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Template.Web.Controllers;

[Authorize]
public class ProfileController : Controller
{
    public IActionResult Index() => View();
}
