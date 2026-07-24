using Template.Data.Entities;

namespace Template.Core.Repository.Categories;

public interface ICategoryRepository
{
    Task<(IReadOnlyList<Category> Items, int TotalCount)> GetPageAsync(
        string? searchTerm,
        bool? isActive,
        int page,
        int pageSize);
    Task<Category?> GetByIdAsync(int id, bool tracking = false);
    Task<bool> NameExistsAsync(string name, int? excludingId = null);
    Task<bool> IsInUseAsync(int id);
    void Add(Category category);
    void Remove(Category category);
    Task SaveChangesAsync();
}
