using System.ComponentModel.DataAnnotations;

namespace FinanceDashboardSystem.DTOs;

public class CategoryDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? Description { get; set; }
}
