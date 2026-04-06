using System.ComponentModel.DataAnnotations;

namespace FinanceDashboardSystem.DTOs;

public class LoginRequest
{
    [Required]
    [RegularExpression(@"^[6-9][0-9]{9}$")]
    public string PhoneNumber { get; set; } = string.Empty;

    public string? UserType { get; set; }
}