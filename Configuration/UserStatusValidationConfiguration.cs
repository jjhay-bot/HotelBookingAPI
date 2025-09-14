using HotelBookingAPI.Security;

namespace HotelBookingAPI.Configuration;

/// <summary>
/// User status validation configuration options
/// </summary>
public static class UserStatusValidationConfiguration
{
    /// <summary>
    /// Add user status validation with caching (recommended for production)
    /// </summary>
    public static IApplicationBuilder UseOptimizedUserStatusValidation(this IApplicationBuilder app)
    {
        return app.UseMiddleware<OptimizedUserStatusValidationMiddleware>();
    }
    
    /// <summary>
    /// Add basic user status validation (simpler but more DB load)
    /// </summary>
    public static IApplicationBuilder UseBasicUserStatusValidation(this IApplicationBuilder app)
    {
        return app.UseMiddleware<UserStatusValidationMiddleware>();
    }
    
    /// <summary>
    /// Configure memory cache for user status validation
    /// </summary>
    public static IServiceCollection AddUserStatusValidationCache(this IServiceCollection services)
    {
        services.AddMemoryCache(options =>
        {
            options.SizeLimit = 10000; // Cache up to 10,000 user statuses
        });
        
        return services;
    }
}
