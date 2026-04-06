using FinanceDashboardSystem.DTOs;
using FinanceDashboardSystem.Models;

namespace FinanceDashboardSystem.Repositories.UserRepo;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(string id);
    Task<User?> GetByPhoneAndReferenceAsync(string phone, string reference);
    Task<List<User>> GetAllAsync();
    Task<List<User>> GetAllActiveAsync();
}