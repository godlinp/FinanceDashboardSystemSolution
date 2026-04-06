namespace FinanceDashboardSystem.Services.OtpService;

public interface IOtpService
{
    string GenerateOtp(string phoneNumber, string referenceId);

    bool ValidateOtp(string phoneNumber, string referenceId, string otp);
}