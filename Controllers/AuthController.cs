using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FinanceDashboardSystem.DTOs;
using FinanceDashboardSystem.Models;
using FinanceDashboardSystem.Repositories.UserRepo;
using FinanceDashboardSystem.Services.OtpService;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace FinanceDashboardSystem.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IOtpService _otpService;
    private readonly IUserRepository _userRepo;
    private readonly UserManager<User> _userManager;
    private readonly IConfiguration _configuration;

    public AuthController(
        IOtpService otpService,
        IUserRepository userRepo,
        UserManager<User> userManager,
        IConfiguration configuration)
    {
        _otpService = otpService;
        _userRepo = userRepo;
        _userManager = userManager;
        _configuration = configuration;
    }

    /// <summary>
    /// Step 1 — Send OTP for a given phone + referenceId combination.
    /// For development the OTP is returned in the response body; in production
    /// it would be delivered via SMS.
    /// </summary>
    [HttpPost("send-otp")]
    public async Task<IActionResult> SendOtp([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.PhoneNumber))
            return BadRequest(new { message = "Phone number is required" });

        // 🔹 Check user exists
        var user = await _userManager.Users
            .FirstOrDefaultAsync(u => u.PhoneNumber == request.PhoneNumber);

        var isExistingUser = user != null;

        // 🔥 CASE 1: USER DOES NOT EXIST → REQUIRE ROLE
        if (!isExistingUser)
        {
            if (string.IsNullOrWhiteSpace(request.UserType))
            {
                return BadRequest(new
                {
                    message = "UserType is required for new users (Admin / Analyst / Viewer)"
                });
            }

            // Validate role
            if (!Enum.TryParse<UserRole>(request.UserType, true, out var parsedRole))
            {
                return BadRequest(new
                {
                    message = "Invalid UserType. Allowed: Admin, Analyst, Viewer"
                });
            }
        }

        // 🔹 Generate ReferenceId (always new per request)
        var referenceId = Guid.NewGuid().ToString("N").Substring(0, 8);

        // 🔹 Generate OTP
        var otp = _otpService.GenerateOtp(request.PhoneNumber, referenceId);

        return Ok(new
        {
            message = "OTP sent successfully",
            otp, // ⚠ remove in production
            referenceId,
            isExistingUser
        });
    }

    /// <summary>
    /// Step 2 — Verify OTP and receive a JWT.
    /// New users are auto-registered with the Viewer role.
    /// </summary>
    [HttpPost("verify")]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var isValid = _otpService.ValidateOtp(
            request.PhoneNumber, request.ReferenceId, request.Otp);

        if (!isValid)
            return BadRequest(new { message = "Invalid or expired OTP." });

        var user = await _userManager.Users
            .FirstOrDefaultAsync(u => u.PhoneNumber == request.PhoneNumber);

        // Auto-register new users as Viewer
        if (user is null)
        {
            if (string.IsNullOrWhiteSpace(request.UserType))
            {
                return BadRequest(new
                {
                    message = "UserType is required for new user registration"
                });
            }
            if (!Enum.TryParse<UserRole>(request.UserType, true, out var parsedRole))
            {
                return BadRequest(new
                {
                    message = "Invalid UserType. Allowed: Admin, Analyst, Viewer"
                });
            }
            if (parsedRole == UserRole.Admin)
            {
                return BadRequest("Admin role cannot be self-assigned.");
            }
            user = new User
            {
                UserName = request.PhoneNumber,
                PhoneNumber = request.PhoneNumber,
                ReferenceId = request.ReferenceId,
                Role = parsedRole,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(
                user, Guid.NewGuid().ToString("N") + "Aa1!");

            if (!result.Succeeded)
                return StatusCode(500, new
                {
                    message = "Failed to create user.",
                    errors = result.Errors.Select(e => e.Description)
                });
        }

        if (!user.IsActive)
            return Forbid();

        var token = GenerateJwtToken(user);
        return Ok(new { token, role = user.Role.ToString() });
    }

    // ── Private helpers ────────────────────────────────────────────

    private string GenerateJwtToken(User user)
    {
        var jwtSettings = _configuration.GetSection("Jwt");
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtSettings["Key"]!));

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.MobilePhone, user.PhoneNumber ?? string.Empty),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddDays(
                double.Parse(jwtSettings["ExpiryDays"] ?? "1")),
            signingCredentials: new SigningCredentials(
                key, SecurityAlgorithms.HmacSha256));

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
