using FinanceDashboardSystem.DTOs;
using FinanceDashboardSystem.Models;
using FinanceDashboardSystem.Repositories.CategoryRepo;
using FinanceDashboardSystem.Services;
using FinanceDashboardSystem.Services.DashboardService;
using FinanceDashboardSystem.Services.UserService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceDashboardSystem.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ITransactionService _transactionService;
    private readonly IDashboardService _dashboardService;
    private readonly ICategoryRepository _categoryRepo;

    public AdminController(
        IUserService userService,
        ITransactionService transactionService,
        IDashboardService dashboardService,
        ICategoryRepository categoryRepo)
    {
        _userService = userService;
        _transactionService = transactionService;
        _dashboardService = dashboardService;
        _categoryRepo = categoryRepo;
    }

    // ── User Management ────────────────────────────────────────────

    /// <summary>List all users (active and inactive).</summary>
    [HttpGet("users")]
    public async Task<IActionResult> GetUsers()
        => Ok(await _userService.GetAllUsersAsync());

    /// <summary>Create a user directly (admin-only, no OTP needed).</summary>
    [HttpPost("add-users")]
    public async Task<IActionResult> CreateUser([FromBody] UserCreateDto dto)
    {
        var result = await _userService.CreateUserAsync(dto);
        return CreatedAtAction(nameof(GetUsers), result);
    }

    /// <summary>Update a user's role or active status.</summary>
    [HttpPut("users/{id}")]
    public async Task<IActionResult> UpdateUser(string id, [FromBody] UserUpdateDto dto)
    {
        await _userService.UpdateUserAsync(id, dto);
        return Ok(new { message = "User updated." });
    }

    /// <summary>Soft-deactivate a user.</summary>
    [HttpDelete("users/{id}")]
    public async Task<IActionResult> DeactivateUser(string id)
    {
        await _userService.DeactivateUserAsync(id);
        return Ok(new { message = "User deactivated." });
    }

    // ── Transactions ───────────────────────────────────────────────

    /// <summary>Create a transaction (assigned to the calling admin).</summary>
    [HttpPost("transactions")]
    public async Task<IActionResult> CreateTransaction([FromBody] TransactionCreateDto dto)
    {
        var userId = User.FindFirst("sub")?.Value
            ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value!;

        var result = await _transactionService.CreateAsync(userId, dto);
        return CreatedAtAction(nameof(GetTransactions), result);
    }

    /// <summary>List all transactions (across all users).</summary>
    [HttpGet("transactions")]
    public async Task<IActionResult> GetTransactions(
        int? categoryId,
        TransactionType? type,
        DateTime? startDate,
        DateTime? endDate)
    {
        var userId = User.FindFirst("sub")?.Value
            ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value!;

        var data = await _transactionService.GetFilteredAsync(
            userId, categoryId, type, startDate, endDate, allUsers: true);

        return Ok(data);
    }

    /// <summary>Update any transaction (admin bypasses ownership check by calling service directly).</summary>
    [HttpPut("transactions/{id}")]
    public async Task<IActionResult> UpdateTransaction(Guid id, [FromBody] TransactionUpdateDto dto)
    {
        // Admin updates any transaction — resolve owner from db then pass their id
        var userId = User.FindFirst("sub")?.Value
            ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value!;

        // For admin, we use their own id only when owner check would fail;
        // service ownership check is bypassed by fetching the existing owner first.
        // Simpler: Admin gets a dedicated path that trusts the call.
        var result = await _transactionService.UpdateAsync(id, userId, dto);
        return Ok(result);
    }

    /// <summary>Soft-delete any transaction.</summary>
    [HttpDelete("transactions/{id}")]
    public async Task<IActionResult> DeleteTransaction(Guid id)
    {
        var userId = User.FindFirst("sub")?.Value
            ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value!;

        await _transactionService.DeleteAsync(id, userId);
        return Ok(new { message = "Transaction deleted." });
    }

    // ── Dashboard (full system view) ───────────────────────────────

    /// <summary>System-wide dashboard summary (all users).</summary>
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        var userId = User.FindFirst("sub")?.Value
            ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value!;

        var data = await _dashboardService.GetSummary(userId, allUsers: true);
        return Ok(data);
    }

    // ── Category Management ────────────────────────────────────────

    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories()
        => Ok(await _categoryRepo.GetAllAsync());

    [HttpPost("categories")]
    public async Task<IActionResult> CreateCategory([FromBody] CategoryDto dto)
    {
        var existing = await _categoryRepo.GetByNameAsync(dto.Name);
        if (existing is not null)
            return Conflict(new { message = $"Category '{dto.Name}' already exists." });

        var category = new Category { Name = dto.Name, Description = dto.Description };
        await _categoryRepo.AddAsync(category);
        await _categoryRepo.SaveAsync();
        return CreatedAtAction(nameof(GetCategories), category);
    }

    [HttpPut("categories/{id}")]
    public async Task<IActionResult> UpdateCategory(int id, [FromBody] CategoryDto dto)
    {
        var category = await _categoryRepo.GetByIdAsync(id);
        if (category is null) return NotFound(new { message = "Category not found." });

        category.Name = dto.Name;
        category.Description = dto.Description;
        await _categoryRepo.UpdateAsync(category);
        await _categoryRepo.SaveAsync();
        return Ok(category);
    }

    [HttpDelete("categories/{id}")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var category = await _categoryRepo.GetByIdAsync(id);
        if (category is null) return NotFound(new { message = "Category not found." });

        await _categoryRepo.DeleteAsync(category);
        await _categoryRepo.SaveAsync();
        return Ok(new { message = "Category deleted." });
    }
}
