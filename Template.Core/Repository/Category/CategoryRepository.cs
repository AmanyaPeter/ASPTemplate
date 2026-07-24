using Microsoft.EntityFrameworkCore;
using Template.Data.Configurations;

namespace Template.Core.Repository.Category;

public class CategoryRepository : ICategoryRepository
{
    private readonly ApplicationDbContext _db;

    public CategoryRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<List<Data.Entities.Category>> GetAllAsync(string? searchTerm = null, string? statusFilter = null)
    {
        var query = _db.Categories.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
            query = query.Where(c => c.Name.Contains(searchTerm) || (c.Description != null && c.Description.Contains(searchTerm)));

        if (statusFilter == "active") query = query.Where(c => c.IsActive);
        if (statusFilter == "inactive") query = query.Where(c => !c.IsActive);

        return await query.OrderBy(c => c.Name).ToListAsync();
    }

    public async Task<Data.Entities.Category?> GetByIdAsync(int id)
    {
        return await _db.Categories.FindAsync(id);
    }

    public async Task<bool> IsNameUniqueAsync(string name, int? excludeId = null)
    {
        return !await _db.Categories.AnyAsync(c =>
            c.Name == name && c.Id != excludeId);
    }

    public async Task<Data.Entities.Category> CreateAsync(string name, string? description, bool isActive)
    {
        var entity = new Data.Entities.Category
        {
            Name = name.Trim(),
            Description = description?.Trim(),
            IsActive = isActive
        };
        _db.Categories.Add(entity);
        await _db.SaveChangesAsync();
        return entity;
    }

    public async Task<Data.Entities.Category> UpdateAsync(int id, string name, string? description, bool isActive)
    {
        var entity = await _db.Categories.FindAsync(id)
            ?? throw new InvalidOperationException($"Category with id {id} not found.");

        entity.Name = name.Trim();
        entity.Description = description?.Trim();
        entity.IsActive = isActive;
        await _db.SaveChangesAsync();
        return entity;
    }

    public async Task<bool> SoftDeleteAsync(int id)
    {
        var entity = await _db.Categories.FindAsync(id);
        if (entity == null) return false;

        entity.IsActive = false;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<int> GetCountAsync(string? searchTerm = null, string? statusFilter = null)
    {
        var query = _db.Categories.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
            query = query.Where(c => c.Name.Contains(searchTerm) || (c.Description != null && c.Description.Contains(searchTerm)));

        if (statusFilter == "active") query = query.Where(c => c.IsActive);
        if (statusFilter == "inactive") query = query.Where(c => !c.IsActive);

        return await query.CountAsync();
    }
}