namespace Template.Core.Repository.Category;

public interface ICategoryRepository
{
    Task<List<Data.Entities.Category>> GetAllAsync(string? searchTerm = null, string? statusFilter = null);
    Task<Data.Entities.Category?> GetByIdAsync(int id);
    Task<bool> IsNameUniqueAsync(string name, int? excludeId = null);
    Task<Data.Entities.Category> CreateAsync(string name, string? description, bool isActive);
    Task<Data.Entities.Category> UpdateAsync(int id, string name, string? description, bool isActive);
    Task<bool> SoftDeleteAsync(int id);
    Task<int> GetCountAsync(string? searchTerm = null, string? statusFilter = null);
}