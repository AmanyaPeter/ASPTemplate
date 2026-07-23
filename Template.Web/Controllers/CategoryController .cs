 using Microsoft.AspNetCore.Mvc;

namespace Template.Web.Controllers
{
    [Microsoft.AspNetCore.Authorization.Authorize]
    public class CategoryController : Controller
    {
        public IActionResult Index()
        {
            return View(new Template.Web.Models.Category.CategoriesModel
            {
                CurrentPage = 1,
                TotalPages = 1
            });
        }
    }
}
