namespace FinanceDashboardSystem.Services.DashboardService;

public interface IDashboardService
{
    Task<object> GetSummary(string userId, bool allUsers = false);
}