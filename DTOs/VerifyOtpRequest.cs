using System.ComponentModel.DataAnnotations;

namespace FinanceDashboardSystem.DTOs;

public class VerifyOtpRequest
{
    [Required]
    [Phone]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string ReferenceId { get; set; } = string.Empty;
    
    public string? UserType { get; set; }

    [Required]
    [StringLength(6, MinimumLength = 6, ErrorMessage = "OTP must be exactly 6 digits.")]
    public string Otp { get; set; } = string.Empty;
}