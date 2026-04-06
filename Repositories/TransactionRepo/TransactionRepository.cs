using FinanceDashboardSystem.DbContext;
using FinanceDashboardSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace FinanceDashboardSystem.Repositories.TransactionRepo;

public class TransactionRepository : ITransactionRepository
{
    private readonly FinanceDbContext _context;

    public TransactionRepository(FinanceDbContext context)
    {
        _context = context;
    }

    public async Task<Transaction?> GetByIdAsync(Guid id)
        => await _context.Transactions
            .Include(t => t.Category)
            .FirstOrDefaultAsync(t => t.Id == id);

    public async Task<List<Transaction>> GetAllAsync()
        => await _context.Transactions
            .Include(t => t.Category)
            .ToListAsync();

    public async Task AddAsync(Transaction transaction)
    {
        await _context.Transactions.AddAsync(transaction);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Transaction transaction)
    {
        _context.Transactions.Update(transaction);
        await _context.SaveChangesAsync();
    }

    /// <summary>Sets IsDeleted = true (soft delete). EF query filter hides it automatically.</summary>
    public async Task SoftDeleteAsync(Guid id)
    {
        // IgnoreQueryFilters to find even if somehow already deleted
        var transaction = await _context.Transactions
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Id == id);

        if (transaction == null)
            throw new KeyNotFoundException($"Transaction {id} not found.");

        transaction.IsDeleted = true;
        transaction.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    /// <summary>Returns a queryable that respects the global soft-delete filter.</summary>
    public IQueryable<Transaction> Query()
        => _context.Transactions
            .Include(t => t.Category)
            .AsQueryable();

    public async Task SaveAsync()
        => await _context.SaveChangesAsync();
}