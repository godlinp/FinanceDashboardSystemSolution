using FinanceDashboardSystem.DTOs;
using FinanceDashboardSystem.Models;
using FinanceDashboardSystem.Repositories.TransactionRepo;
using Microsoft.EntityFrameworkCore;

namespace FinanceDashboardSystem.Services;

public class TransactionService : ITransactionService
{
    private readonly ITransactionRepository _repo;

    public TransactionService(ITransactionRepository repo)
    {
        _repo = repo;
    }

    public async Task<TransactionResponseDto> CreateAsync(string userId, TransactionCreateDto dto)
    {
        var transaction = new Transaction
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Amount = dto.Amount,
            Type = dto.Type,
            CategoryId = dto.CategoryId,
            Date = dto.Date,
            Notes = dto.Notes,
            CreatedAt = DateTime.UtcNow
        };

        await _repo.AddAsync(transaction);

        // Reload with navigation so CategoryName is available
        var created = await _repo.GetByIdAsync(transaction.Id)
            ?? throw new InvalidOperationException("Failed to retrieve created transaction.");

        return TransactionResponseDto.From(created);
    }

    public async Task<TransactionResponseDto> UpdateAsync(Guid id, string userId, TransactionUpdateDto dto)
    {
        var existing = await _repo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Transaction {id} not found.");

        // Admins can update any; others only their own
        if (existing.UserId != userId)
            throw new UnauthorizedAccessException("You do not own this transaction.");

        existing.Amount = dto.Amount;
        existing.Type = dto.Type;
        existing.CategoryId = dto.CategoryId;
        existing.Date = dto.Date;
        existing.Notes = dto.Notes;
        existing.UpdatedAt = DateTime.UtcNow;

        await _repo.UpdateAsync(existing);

        return TransactionResponseDto.From(existing);
    }

    public async Task DeleteAsync(Guid id, string userId)
    {
        var existing = await _repo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Transaction {id} not found.");

        if (existing.UserId != userId)
            throw new UnauthorizedAccessException("You do not own this transaction.");

        await _repo.SoftDeleteAsync(id);
    }

    public async Task<List<TransactionResponseDto>> GetFilteredAsync(
        string userId,
        int? categoryId,
        TransactionType? type,
        DateTime? startDate,
        DateTime? endDate,
        bool allUsers = false)
    {
        var query = _repo.Query();

        if (!allUsers)
            query = query.Where(x => x.UserId == userId);

        if (categoryId.HasValue)
            query = query.Where(x => x.CategoryId == categoryId);

        if (type.HasValue)
            query = query.Where(x => x.Type == type);

        if (startDate.HasValue)
            query = query.Where(x => x.Date >= startDate);

        if (endDate.HasValue)
            query = query.Where(x => x.Date <= endDate);

        var results = await query.OrderByDescending(x => x.Date).ToListAsync();
        return results.Select(TransactionResponseDto.From).ToList();
    }
}