using Microsoft.AspNetCore.Identity;

namespace FinanceDashboardSystem.Models;

public enum UserRole
{
    Viewer = 1,
    Analyst = 2,
    Admin = 3
}

public class User : IdentityUser
{
    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    /// <summary>
    /// External reference identifier (e.g. employee ID, client code) used
    /// together with PhoneNumber to uniquely identify a user during OTP login.
    /// </summary>
    public string ReferenceId { get; set; } = string.Empty;

    public UserRole Role { get; set; } = UserRole.Viewer;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}