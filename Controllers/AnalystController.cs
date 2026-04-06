using FinanceDashboardSystem.Models;
using FinanceDashboardSystem.Services;
using FinanceDashboardSystem.Services.DashboardService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceDashboardSystem.Controllers;

[ApiController]
[Route("api/analyst")]
[Authorize(Roles = "Analyst,Admin")]
public class AnalystController : ControllerBase
{
    private readonly ITransactionService _transactionService;
    private readonly IDashboardService _dashboardService;

    public AnalystController(
        ITransactionService transactionService,
        IDashboardService dashboardService)
    {
        _transactionService = transactionService;
        _dashboardService = dashboardService;
    }

    /// <summary>
    /// Full filtered transaction list with date range support.
    /// Analysts see only their own transactions.
    /// </summary>
    [HttpGet("transactions")]
    public async Task<IActionResult> GetTransactions(
        int? categoryId,
        TransactionType? type,
        DateTime? startDate,
        DateTime? endDate)
    {
        var userId = User.FindFirst("sub")?.Value
            ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value!;

        var data = await _transactionService
            .GetFilteredAsync(userId, categoryId, type, startDate, endDate);

        return Ok(data);
    }

    /// <summary>
    /// Extended insights/dashboard for analysts — their own data.
    /// </summary>
    [HttpGet("insights")]
    public async Task<IActionResult> GetInsights()
    {
        var userId = User.FindFirst("sub")?.Value
            ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value!;

        var data = await _dashboardService.GetSummary(userId);
        return Ok(data);
    }
}
