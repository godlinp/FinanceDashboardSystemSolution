using FinanceDashboardSystem.Models;

namespace FinanceDashboardSystem.Repositories.CategoryRepo;

public interface ICategoryRepository
{
    Task<List<Category>> GetAllAsync();
    Task<Category?> GetByIdAsync(int id);
    Task<Category?> GetByNameAsync(string name);
    Task AddAsync(Category category);
    Task UpdateAsync(Category category);
    Task DeleteAsync(Category category);
    Task SaveAsync();
}
