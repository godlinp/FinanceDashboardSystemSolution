using FinanceDashboardSystem.DTOs;

namespace FinanceDashboardSystem.Services.UserService;

public interface IUserService
{
    Task<List<UserResponseDto>> GetAllUsersAsync();
    Task<List<UserResponseDto>> GetActiveUsersAsync();
    Task<UserResponseDto?> GetByIdAsync(string id);
    Task<UserResponseDto> CreateUserAsync(UserCreateDto dto);
    Task UpdateUserAsync(string id, UserUpdateDto dto);
    Task DeactivateUserAsync(string id);
}
