using Microsoft.Extensions.Caching.Memory;

namespace FinanceDashboardSystem.Services.OtpService;

public class OtpService : IOtpService
{
    private readonly IMemoryCache _cache;

    public OtpService(IMemoryCache cache)
    {
        _cache = cache;
    }

    //  Generate OTP
    public string GenerateOtp(string phoneNumber, string referenceId)
    {
        var otp = new Random().Next(100000, 999999).ToString();

        var key = GetCacheKey(phoneNumber, referenceId);

        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
        };

        _cache.Set(key, otp, cacheOptions);

        return otp;
    }

    //  Validate OTP
    public bool ValidateOtp(string phoneNumber, string referenceId, string otp)
    {
        var key = GetCacheKey(phoneNumber, referenceId);

        if (_cache.TryGetValue(key, out string? storedOtp))
        {
            // Optional: Remove after successful validation
            if (storedOtp == otp)
            {
                _cache.Remove(key);
                return true;
            }
        }

        return false;
    }

    //  Private helper
    private string GetCacheKey(string phoneNumber, string referenceId)
    {
        return $"OTP_{phoneNumber}_{referenceId}";
    }
}