using System.ComponentModel.DataAnnotations;

namespace FinanceDashboardSystem.Models;

public enum TransactionType
{
    Income = 1,
    Expense = 2
}

/// <summary>
/// A financial record (income or expense) created by a user.
/// </summary>
public class Transaction
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero.")]
    public decimal Amount { get; set; }

    [Required]
    public TransactionType Type { get; set; }

    // Normalized FK → Category  (replaces raw string)
    [Required]
    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    [Required]
    public DateTime Date { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }

    // Soft delete
    public bool IsDeleted { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // FK → Identity User
    public string UserId { get; set; } = string.Empty;
    public User User { get; set; } = null!;
}