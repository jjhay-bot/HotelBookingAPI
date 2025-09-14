using HotelBookingAPI.Models;
using HotelBookingAPI.Services;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Claims;

namespace HotelBookingAPI.Security;

/// <summary>
/// Optimized middleware that validates current user status with caching
/// Reduces database load while maintaining security
/// </summary>
public class OptimizedUserStatusValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<OptimizedUserStatusValidationMiddleware> _logger;
    private readonly IMemoryCache _cache;
    
    // Cache settings
    private static readonly TimeSpan CacheExpiry = TimeSpan.FromMinutes(5); // 5-minute cache
    private const string CacheKeyPrefix = "user_status_";

    public OptimizedUserStatusValidationMiddleware(
        RequestDelegate next, 
        ILogger<OptimizedUserStatusValidationMiddleware> logger,
        IMemoryCache cache)
    {
        _next = next;
        _logger = logger;
        _cache = cache;
    }

    public async Task InvokeAsync(HttpContext context, UserService userService)
    {
        // Only check authenticated requests
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var userId = context.User.FindFirst("userId")?.Value;
            
            if (!string.IsNullOrEmpty(userId))
            {
                try
                {
                    var userStatus = await GetUserStatusAsync(userId, userService);
                    
                    if (userStatus == null)
                    {
                        _logger.LogWarning("User {UserId} not found in database but has valid token", userId);
                        await ReturnUnauthorized(context, "User account not found");
                        return;
                    }
                    
                    if (!userStatus.IsActive)
                    {
                        var username = context.User.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown";
                        _logger.LogWarning("Deactivated user {Username} (ID: {UserId}) attempted to access protected resource", username, userId);
                        
                        // Remove from cache since user is deactivated
                        _cache.Remove(CacheKeyPrefix + userId);
                        
                        await ReturnUnauthorized(context, "Account is deactivated");
                        return;
                    }
                    
                    // Optional: Validate role hasn't changed (for extra security)
                    var tokenRole = context.User.FindFirst(ClaimTypes.Role)?.Value;
                    if (tokenRole != null && tokenRole != userStatus.Role.ToString())
                    {
                        _logger.LogWarning("User {UserId} role changed from {TokenRole} to {CurrentRole}, invalidating token", 
                            userId, tokenRole, userStatus.Role);
                        
                        // Remove from cache since role changed
                        _cache.Remove(CacheKeyPrefix + userId);
                        
                        await ReturnUnauthorized(context, "User role has changed, please login again");
                        return;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error validating user status for {UserId}", userId);
                    await ReturnUnauthorized(context, "Authentication validation failed");
                    return;
                }
            }
        }

        await _next(context);
    }

    private async Task<UserStatusInfo?> GetUserStatusAsync(string userId, UserService userService)
    {
        var cacheKey = CacheKeyPrefix + userId;
        
        // Try to get from cache first
        if (_cache.TryGetValue(cacheKey, out UserStatusInfo? cachedStatus))
        {
            _logger.LogDebug("User status cache hit for {UserId}", userId);
            return cachedStatus;
        }
        
        // Cache miss - query database
        _logger.LogDebug("User status cache miss for {UserId}, querying database", userId);
        var user = await userService.GetAsync(userId);
        
        if (user == null)
        {
            return null;
        }
        
        var userStatus = new UserStatusInfo
        {
            IsActive = user.IsActive,
            Role = user.Role
        };
        
        // Cache the result
        _cache.Set(cacheKey, userStatus, CacheExpiry);
        
        return userStatus;
    }

    private static async Task ReturnUnauthorized(HttpContext context, string message)
    {
        context.Response.StatusCode = 401;
        context.Response.ContentType = "application/json";
        
        var errorResponse = new
        {
            error = new
            {
                code = 401,
                message = message
            }
        };
        
        var json = System.Text.Json.JsonSerializer.Serialize(errorResponse);
        await context.Response.WriteAsync(json);
    }
}

/// <summary>
/// Lightweight user status information for caching
/// </summary>
public class UserStatusInfo
{
    public bool IsActive { get; set; }
    public UserRole Role { get; set; }
}

/// <summary>
/// Configuration options for user status validation
/// </summary>
public class UserStatusValidationOptions
{
    public TimeSpan CacheExpiry { get; set; } = TimeSpan.FromMinutes(5);
    public bool EnableCaching { get; set; } = true;
    public bool ValidateRoleChanges { get; set; } = true;
}
