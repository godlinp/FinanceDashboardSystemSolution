using System.ComponentModel.DataAnnotations;
using FinanceDashboardSystem.Models;

namespace FinanceDashboardSystem.DTOs;

public class UserCreateDto
{
    [Required]
    [Phone]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string ReferenceId { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? FirstName { get; set; }

    [MaxLength(100)]
    public string? LastName { get; set; }

    [Required]
    public UserRole Role { get; set; } = UserRole.Viewer;
}
