using FinanceDashboardSystem.DTOs;
using FinanceDashboardSystem.Models;
using FinanceDashboardSystem.Repositories.UserRepo;
using Microsoft.AspNetCore.Identity;

namespace FinanceDashboardSystem.Services.UserService;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepo;
    private readonly UserManager<User> _userManager;

    public UserService(IUserRepository userRepo, UserManager<User> userManager)
    {
        _userRepo = userRepo;
        _userManager = userManager;
    }

    public async Task<List<UserResponseDto>> GetAllUsersAsync()
    {
        var users = await _userRepo.GetAllAsync();
        return users.Select(UserResponseDto.From).ToList();
    }

    public async Task<List<UserResponseDto>> GetActiveUsersAsync()
    {
        var users = await _userRepo.GetAllActiveAsync();
        return users.Select(UserResponseDto.From).ToList();
    }

    public async Task<UserResponseDto?> GetByIdAsync(string id)
    {
        var user = await _userRepo.GetByIdAsync(id);
        return user is null ? null : UserResponseDto.From(user);
    }

    /// <summary>
    /// Admin creates a user directly (no OTP required).
    /// A dummy password is set — the user is expected to log in via OTP flow.
    /// </summary>
    public async Task<UserResponseDto> CreateUserAsync(UserCreateDto dto)
    {
        var existing = await _userRepo.GetByPhoneAndReferenceAsync(dto.PhoneNumber, dto.ReferenceId);
        if (existing is not null)
            throw new InvalidOperationException("A user with this phone number and reference ID already exists.");

        var user = new User
        {
            UserName = dto.PhoneNumber,
            PhoneNumber = dto.PhoneNumber,
            ReferenceId = dto.ReferenceId,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Role = dto.Role,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // Identity requires a password; OTP flow doesn't use it but Identity needs one.
        var result = await _userManager.CreateAsync(user, Guid.NewGuid().ToString("N") + "Aa1!");

        if (!result.Succeeded)
            throw new InvalidOperationException(
                string.Join("; ", result.Errors.Select(e => e.Description)));

        return UserResponseDto.From(user);
    }

    public async Task UpdateUserAsync(string id, UserUpdateDto dto)
    {
        var user = await _userManager.FindByIdAsync(id)
            ?? throw new KeyNotFoundException($"User {id} not found.");

        user.Role = dto.Role;
        user.IsActive = dto.IsActive;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            throw new InvalidOperationException(
                string.Join("; ", result.Errors.Select(e => e.Description)));
    }

    public async Task DeactivateUserAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id)
            ?? throw new KeyNotFoundException($"User {id} not found.");

        user.IsActive = false;
        await _userManager.UpdateAsync(user);
    }
}
