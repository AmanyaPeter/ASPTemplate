using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Template.Common.Static;
using Template.Core.Repository.Categories;
using Template.Data.Entities;
using Template.Web.Models.Category;

namespace Template.Web.Controllers;

[Authorize(Roles = RoleConstants.InnovationTeam)]
public sealed class CategoryController(ICategoryRepository categories) : Controller
{
    private const int PageSize = 10;

    [HttpGet]
    public async Task<IActionResult> Index(
        string? searchTerm,
        string? statusFilter,
        int page = 1)
    {
        var currentPage = Math.Max(page, 1);
        bool? isActive = bool.TryParse(statusFilter, out var parsedStatus)
            ? parsedStatus
            : null;
        var result = await categories.GetPageAsync(
            searchTerm,
            isActive,
            currentPage,
            PageSize);

        return View(new CategoriesModel
        {
            SearchTerm = searchTerm,
            StatusFilter = statusFilter,
            CurrentPage = currentPage,
            TotalPages = Math.Max(1, (int)Math.Ceiling(result.TotalCount / (double)PageSize)),
            Categories = result.Items.Select(category => new CategoryItemViewModel
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                CreatedDate = category.CreatedDate,
                IsActive = category.IsActive
            }).ToList()
        });
    }

    [HttpGet]
    public async Task<IActionResult> Get(int id)
    {
        var category = await categories.GetByIdAsync(id);
        return category == null
            ? NotFound()
            : Json(new
            {
                category.Id,
                category.Name,
                category.Description,
                category.IsActive
            });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(CategoryFormViewModel category)
    {
        category.Name = category.Name.Trim();
        category.Description = category.Description?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(category.Name))
        {
            TempData["ErrorMessage"] = "Category name is required.";
            return RedirectToAction(nameof(Index));
        }
        if (await categories.NameExistsAsync(category.Name, category.Id == 0 ? null : category.Id))
        {
            TempData["ErrorMessage"] = "A category with that name already exists.";
            return RedirectToAction(nameof(Index));
        }

        if (category.Id == 0)
        {
            categories.Add(new Category
            {
                Name = category.Name,
                Description = category.Description,
                IsActive = category.IsActive,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = CurrentUserId()
            });
            TempData["SuccessMessage"] = "Category created.";
        }
        else
        {
            var entity = await categories.GetByIdAsync(category.Id, tracking: true);
            if (entity == null) return NotFound();
            entity.Name = category.Name;
            entity.Description = category.Description;
            entity.IsActive = category.IsActive;
            entity.ModifiedDate = DateTime.UtcNow;
            entity.ModifiedBy = CurrentUserId();
            TempData["SuccessMessage"] = "Category updated.";
        }

        await categories.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var category = await categories.GetByIdAsync(id, tracking: true);
        if (category == null) return NotFound();

        if (await categories.IsInUseAsync(id))
        {
            category.IsActive = false;
            category.ModifiedDate = DateTime.UtcNow;
            category.ModifiedBy = CurrentUserId();
            TempData["SuccessMessage"] = "Category is used by existing ideas, so it was deactivated instead of deleted.";
        }
        else
        {
            categories.Remove(category);
            TempData["SuccessMessage"] = "Category deleted.";
        }

        await categories.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private string CurrentUserId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "system";
}
