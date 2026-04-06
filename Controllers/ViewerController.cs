using FinanceDashboardSystem.Models;
using FinanceDashboardSystem.Services;
using FinanceDashboardSystem.Services.DashboardService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceDashboardSystem.Controllers;

[ApiController]
[Route("api/viewer")]
[Authorize(Roles = "Viewer,Analyst,Admin")]
public class ViewerController : ControllerBase
{
    private readonly ITransactionService _transactionService;
    private readonly IDashboardService _dashboardService;

    public ViewerController(
        ITransactionService transactionService,
        IDashboardService dashboardService)
    {
        _transactionService = transactionService;
        _dashboardService = dashboardService;
    }

    /// <summary>
    /// View own transactions. Supports filtering by category and type.
    /// Viewers cannot filter by date range (analyst feature).
    /// </summary>
    [HttpGet("transactions")]
    public async Task<IActionResult> GetTransactions(
        int? categoryId,
        TransactionType? type)
    {
        var userId = User.FindFirst("sub")?.Value
            ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value!;

        var data = await _transactionService
            .GetFilteredAsync(userId, categoryId, type, null, null);

        return Ok(data);
    }

    /// <summary>
    /// Basic dashboard summary for the viewer's own data.
    /// </summary>
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        var userId = User.FindFirst("sub")?.Value
            ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value!;

        var data = await _dashboardService.GetSummary(userId);
        return Ok(data);
    }
}
