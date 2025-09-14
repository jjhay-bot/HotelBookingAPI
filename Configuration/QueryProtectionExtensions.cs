using HotelBookingAPI.Security;

namespace HotelBookingAPI.Configuration;

/// <summary>
/// Extension methods for adding query protection middleware
/// </summary>
public static class QueryProtectionExtensions
{
    /// <summary>
    /// Adds query protection middleware to prevent infinite queries and server overload
    /// </summary>
    public static IApplicationBuilder UseQueryProtection(this IApplicationBuilder app)
    {
        return app.UseMiddleware<QueryProtectionMiddleware>();
    }
}
