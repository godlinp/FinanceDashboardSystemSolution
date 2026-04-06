using FinanceDashboardSystem.Models;

namespace FinanceDashboardSystem.Repositories.TransactionRepo;

public interface ITransactionRepository
{
    Task<Transaction?> GetByIdAsync(Guid id);
    Task<List<Transaction>> GetAllAsync();
    Task AddAsync(Transaction transaction);
    Task UpdateAsync(Transaction transaction);
    Task SoftDeleteAsync(Guid id);
    IQueryable<Transaction> Query();
    Task SaveAsync();
}