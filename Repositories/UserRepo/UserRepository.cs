using FinanceDashboardSystem.DbContext;
using FinanceDashboardSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace FinanceDashboardSystem.Repositories.UserRepo;

public class UserRepository : IUserRepository
{
    private readonly FinanceDbContext _context;

    public UserRepository(FinanceDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(string id)
        => await _context.Users.FindAsync(id);

    public async Task<User?> GetByPhoneAndReferenceAsync(string phone, string reference)
        => await _context.Users
            .FirstOrDefaultAsync(u =>
                u.PhoneNumber == phone &&
                u.ReferenceId == reference);

    public async Task<List<User>> GetAllAsync()
        => await _context.Users.ToListAsync();

    public async Task<List<User>> GetAllActiveAsync()
        => await _context.Users
            .Where(u => u.IsActive)
            .ToListAsync();
}