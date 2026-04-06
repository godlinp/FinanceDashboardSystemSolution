using FinanceDashboardSystem.Models;
using FinanceDashboardSystem.Repositories.TransactionRepo;
using Microsoft.EntityFrameworkCore;

namespace FinanceDashboardSystem.Services.DashboardService;

public class DashboardService : IDashboardService
{
    private readonly ITransactionRepository _repo;

    public DashboardService(ITransactionRepository repo)
    {
        _repo = repo;
    }

    public async Task<object> GetSummary(string userId, bool allUsers = false)
    {
        // Global query filter already excludes soft-deleted records
        var query = _repo.Query();

        if (!allUsers)
            query = query.Where(x => x.UserId == userId);

        // ── Totals ─────────────────────────────────────────────────
        var totalIncome = await query
            .Where(x => x.Type == TransactionType.Income)
            .SumAsync(x => x.Amount);

        var totalExpense = await query
            .Where(x => x.Type == TransactionType.Expense)
            .SumAsync(x => x.Amount);

        // ── Category-wise breakdown ─────────────────────────────────
        var categoryTotals = await query
            .GroupBy(x => new { x.CategoryId, x.Category.Name })
            .Select(g => new
            {
                CategoryId = g.Key.CategoryId,
                Category = g.Key.Name,
                Income = g.Where(x => x.Type == TransactionType.Income).Sum(x => x.Amount),
                Expense = g.Where(x => x.Type == TransactionType.Expense).Sum(x => x.Amount),
                Net = g.Where(x => x.Type == TransactionType.Income).Sum(x => x.Amount)
                      - g.Where(x => x.Type == TransactionType.Expense).Sum(x => x.Amount)
            })
            .ToListAsync();

        // ── Recent activity (last 10) ───────────────────────────────
        var recent = await query
            .OrderByDescending(x => x.Date)
            .Take(10)
            .Select(x => new
            {
                x.Id,
                x.Amount,
                Type = x.Type.ToString(),
                Category = x.Category.Name,
                x.Date,
                x.Notes
            })
            .ToListAsync();

        // ── Monthly trends ──────────────────────────────────────────
        var monthly = await query
            .GroupBy(x => new { x.Date.Year, x.Date.Month })
            .Select(g => new
            {
                Period = $"{g.Key.Year}-{g.Key.Month:D2}",
                Income = g.Where(x => x.Type == TransactionType.Income).Sum(x => x.Amount),
                Expense = g.Where(x => x.Type == TransactionType.Expense).Sum(x => x.Amount),
                Net = g.Where(x => x.Type == TransactionType.Income).Sum(x => x.Amount)
                      - g.Where(x => x.Type == TransactionType.Expense).Sum(x => x.Amount)
            })
            .OrderBy(g => g.Period)
            .ToListAsync();

        // ── Weekly trends (last 8 weeks) ────────────────────────────
        // ToListAsync first, then group client-side (ISOWeek not translatable to SQL)
        var eightWeeksAgo = DateTime.UtcNow.AddDays(-56);
        var recentRaw = await query
            .Where(x => x.Date >= eightWeeksAgo)
            .ToListAsync();

        var weekly = recentRaw
            .GroupBy(x => System.Globalization.ISOWeek.GetWeekOfYear(x.Date))
            .Select(g => new
            {
                Week = g.Key,
                Income = g.Where(x => x.Type == TransactionType.Income).Sum(x => x.Amount),
                Expense = g.Where(x => x.Type == TransactionType.Expense).Sum(x => x.Amount)
            })
            .OrderBy(w => w.Week)
            .ToList();

        return new
        {
            summary = new
            {
                totalIncome,
                totalExpense,
                netBalance = totalIncome - totalExpense
            },
            categoryBreakdown = categoryTotals,
            recentActivity = recent,
            monthlyTrends = monthly,
            weeklyTrends = weekly
        };
    }
}