using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Template.Common.Static;
using Template.Core.Repository.Category;
using Template.Data.Entities;
using Template.Web.Models.Category;

namespace Template.Web.Controllers;

[Authorize(Roles = RoleConstants.InnovationTeam)]
public class CategoryController : Controller
{
    private readonly ICategoryRepository _repository;
    private const int PageSize = 10;

    public CategoryController(ICategoryRepository repository)
    {
        _repository = repository;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? searchTerm, string? statusFilter, int page = 1)
    {
        page = Math.Max(page, 1);
        var count = await _repository.GetCountAsync(searchTerm, statusFilter);
        var items = await _repository.GetAllAsync(searchTerm, statusFilter);

        var pagedItems = items
            .OrderBy(c => c.Name)
            .Skip((page - 1) * PageSize)
            .Take(PageSize)
            .Select(c => new CategoryItemViewModel
            {
                Id = c.Id, Name = c.Name, Description = c.Description,
                IsActive = c.IsActive, CreatedDate = c.CreatedDate
            }).ToList();

        return View(new CategoriesModel
        {
            SearchTerm = searchTerm, StatusFilter = statusFilter, Categories = pagedItems,
            CurrentPage = page, TotalPages = Math.Max(1, (int)Math.Ceiling(count / (double)PageSize))
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(CategoriesModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Please correct the validation errors.";
            return RedirectToAction(nameof(Index));
        }

        var isUnique = await _repository.IsNameUniqueAsync(model.Category.Name.Trim(),
            model.Category.Id == 0 ? null : model.Category.Id);
        if (!isUnique)
        {
            TempData["ErrorMessage"] = "A category with that name already exists.";
            return RedirectToAction(nameof(Index));
        }

        if (model.Category.Id == 0)
        {
            await _repository.CreateAsync(
                model.Category.Name.Trim(),
                model.Category.Description?.Trim(),
                model.Category.IsActive);
            TempData["SuccessMessage"] = "Category created successfully.";
        }
        else
        {
            await _repository.UpdateAsync(
                model.Category.Id,
                model.Category.Name.Trim(),
                model.Category.Description?.Trim(),
                model.Category.IsActive);
            TempData["SuccessMessage"] = "Category updated successfully.";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _repository.SoftDeleteAsync(id);
        if (!deleted) return NotFound();

        TempData["SuccessMessage"] = "Category deactivated successfully.";
        return RedirectToAction(nameof(Index));
    }
}
