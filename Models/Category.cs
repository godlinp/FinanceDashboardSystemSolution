using System.ComponentModel.DataAnnotations;

namespace FinanceDashboardSystem.Models;

/// <summary>
/// Normalized category lookup table for transactions.
/// </summary>
public class Category
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? Description { get; set; }

    // Navigation
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
