using Microsoft.EntityFrameworkCore;
using Template.Data.Configurations;
using Template.Data.Entities;

namespace Template.Core.Repository.Categories;

public sealed class CategoryRepository(ApplicationDbContext context) : ICategoryRepository
{
    public async Task<(IReadOnlyList<Category> Items, int TotalCount)> GetPageAsync(
        string? searchTerm,
        bool? isActive,
        int page,
        int pageSize)
    {
        var query = context.Categories.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim();
            query = query.Where(category =>
                category.Name.Contains(term) ||
                (category.Description != null && category.Description.Contains(term)));
        }
        if (isActive.HasValue)
        {
            query = query.Where(category => category.IsActive == isActive.Value);
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderBy(category => category.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        return (items, totalCount);
    }

    public Task<Category?> GetByIdAsync(int id, bool tracking = false)
    {
        var query = tracking
            ? context.Categories.AsQueryable()
            : context.Categories.AsNoTracking();
        return query.FirstOrDefaultAsync(category => category.Id == id);
    }

    public Task<bool> NameExistsAsync(string name, int? excludingId = null) =>
        context.Categories.AnyAsync(category =>
            category.Name == name &&
            (!excludingId.HasValue || category.Id != excludingId.Value));

    public Task<bool> IsInUseAsync(int id) =>
        context.InnovationIdeas.AnyAsync(idea => idea.CategoryId == id);

    public void Add(Category category) => context.Categories.Add(category);
    public void Remove(Category category) => context.Categories.Remove(category);
    public Task SaveChangesAsync() => context.SaveChangesAsync();
}
