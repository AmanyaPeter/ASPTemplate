using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Template.Data.Configurations;
using Template.Data.Entities;
using Template.Web.Models.Category;

namespace Template.Web.Controllers;

[Authorize(Roles = "Admin,InnovationTeam")]
public class CategoryController(ApplicationDbContext db) : Controller
{
    private const int PageSize = 10;

    [HttpGet]
    public async Task<IActionResult> Index(
        string? searchTerm,
        string? statusFilter,
        int page = 1,
        int? editId = null)
    {
        CategoryFormViewModel? form = null;
        if (editId.HasValue)
        {
            form = await db.Categories
                .AsNoTracking()
                .Where(category => category.Id == editId.Value)
                .Select(category => new CategoryFormViewModel
                {
                    Id = category.Id,
                    Name = category.Name,
                    Description = category.Description ?? string.Empty,
                    IsActive = category.IsActive
                })
                .FirstOrDefaultAsync();

            if (form == null)
            {
                TempData["ErrorMessage"] = "Category not found.";
                return RedirectToAction(nameof(Index));
            }
        }

        return View(await BuildPageModelAsync(searchTerm, statusFilter, page, form));
    }

    private async Task<CategoriesModel> BuildPageModelAsync(
        string? searchTerm,
        string? statusFilter,
        int page,
        CategoryFormViewModel? form = null)
    {
        page = Math.Max(page, 1);
        var query = db.Categories.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(searchTerm))
            query = query.Where(c => c.Name.Contains(searchTerm) || (c.Description != null && c.Description.Contains(searchTerm)));
        if (statusFilter == "active") query = query.Where(c => c.IsActive);
        if (statusFilter == "inactive") query = query.Where(c => !c.IsActive);
        var count = await query.CountAsync();
        var items = await query.OrderBy(c => c.Name).Skip((page - 1) * PageSize).Take(PageSize)
            .Select(c => new CategoryItemViewModel
            {
                Id = c.Id, Name = c.Name, Description = c.Description,
                IsActive = c.IsActive, CreatedDate = c.CreatedDate
            }).ToListAsync();
        return new CategoriesModel
        {
            SearchTerm = searchTerm, StatusFilter = statusFilter, Categories = items,
            CurrentPage = page, TotalPages = Math.Max(1, (int)Math.Ceiling(count / (double)PageSize)),
            Category = form ?? new CategoryFormViewModel()
        };
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(CategoriesModel model)
    {
        if (!ModelState.IsValid)
            return View(nameof(Index), await BuildPageModelAsync(
                model.SearchTerm, model.StatusFilter, Math.Max(model.CurrentPage, 1), model.Category));

        var duplicate = await db.Categories.AnyAsync(c =>
            c.Name == model.Category.Name && c.Id != model.Category.Id);
        if (duplicate)
        {
            TempData["ErrorMessage"] = "A category with that name already exists.";
            return RedirectToAction(nameof(Index));
        }

        Category entity;
        if (model.Category.Id == 0)
        {
            entity = new Category { Name = model.Category.Name.Trim() };
            db.Categories.Add(entity);
        }
        else
        {
            entity = await db.Categories.FindAsync(model.Category.Id) ?? throw new InvalidOperationException();
            entity.Name = model.Category.Name.Trim();
        }
        entity.Description = model.Category.Description?.Trim();
        entity.IsActive = model.Category.IsActive;
        await db.SaveChangesAsync();
        TempData["SuccessMessage"] = model.Category.Id == 0
            ? "Category created successfully."
            : "Category updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var category = await db.Categories.FindAsync(id);
        if (category == null) return NotFound();
        category.IsActive = false;
        await db.SaveChangesAsync();
        TempData["SuccessMessage"] = $"{category.Name} was removed from active categories.";
        return RedirectToAction(nameof(Index));
    }
}
