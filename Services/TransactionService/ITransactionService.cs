using FinanceDashboardSystem.DTOs;
using FinanceDashboardSystem.Models;

namespace FinanceDashboardSystem.Services;

public interface ITransactionService
{
    Task<TransactionResponseDto> CreateAsync(string userId, TransactionCreateDto dto);
    Task<TransactionResponseDto> UpdateAsync(Guid id, string userId, TransactionUpdateDto dto);
    Task DeleteAsync(Guid id, string userId);
    Task<List<TransactionResponseDto>> GetFilteredAsync(
        string userId,
        int? categoryId,
        TransactionType? type,
        DateTime? startDate,
        DateTime? endDate,
        bool allUsers = false);
}