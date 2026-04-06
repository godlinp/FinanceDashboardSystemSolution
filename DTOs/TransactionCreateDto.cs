using System.ComponentModel.DataAnnotations;
using FinanceDashboardSystem.Models;

namespace FinanceDashboardSystem.DTOs;

public class TransactionCreateDto
{
    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero.")]
    public decimal Amount { get; set; }

    [Required]
    public TransactionType Type { get; set; }

    [Required]
    public int CategoryId { get; set; }

    [Required]
    public DateTime Date { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }
}
