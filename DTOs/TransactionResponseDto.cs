using FinanceDashboardSystem.Models;

namespace FinanceDashboardSystem.DTOs;

public class TransactionResponseDto
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public string Type { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public static TransactionResponseDto From(Transaction t) => new()
    {
        Id = t.Id,
        Amount = t.Amount,
        Type = t.Type.ToString(),
        CategoryId = t.CategoryId,
        CategoryName = t.Category?.Name ?? string.Empty,
        Date = t.Date,
        Notes = t.Notes,
        CreatedAt = t.CreatedAt,
        UpdatedAt = t.UpdatedAt
    };
}
