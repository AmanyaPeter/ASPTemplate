using Microsoft.AspNetCore.Mvc;

namespace Template.Web.Controllers
{
    public class CategoryController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
