using FinanceDashboardSystem.DbContext;
using FinanceDashboardSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace FinanceDashboardSystem.Repositories.CategoryRepo;

public class CategoryRepository : ICategoryRepository
{
    private readonly FinanceDbContext _context;

    public CategoryRepository(FinanceDbContext context)
    {
        _context = context;
    }

    public async Task<List<Category>> GetAllAsync()
        => await _context.Categories.ToListAsync();

    public async Task<Category?> GetByIdAsync(int id)
        => await _context.Categories.FindAsync(id);

    public async Task<Category?> GetByNameAsync(string name)
        => await _context.Categories
            .FirstOrDefaultAsync(c => c.Name.ToLower() == name.ToLower());

    public async Task AddAsync(Category category)
        => await _context.Categories.AddAsync(category);

    public Task UpdateAsync(Category category)
    {
        _context.Categories.Update(category);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Category category)
    {
        _context.Categories.Remove(category);
        return Task.CompletedTask;
    }

    public async Task SaveAsync()
        => await _context.SaveChangesAsync();
}
