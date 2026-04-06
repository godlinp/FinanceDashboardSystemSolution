using System.ComponentModel.DataAnnotations;
using FinanceDashboardSystem.Models;

namespace FinanceDashboardSystem.DTOs;

public class UserUpdateDto
{
    [Required]
    public UserRole Role { get; set; }

    [Required]
    public bool IsActive { get; set; }
}
