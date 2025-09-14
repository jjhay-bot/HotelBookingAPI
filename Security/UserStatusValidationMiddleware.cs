using HotelBookingAPI.Services;
using System.Security.Claims;

namespace HotelBookingAPI.Security;

/// <summary>
/// Middleware that validates current user status for each authenticated request
/// This prevents deactivated users from using old valid tokens
/// </summary>
public class UserStatusValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<UserStatusValidationMiddleware> _logger;

    public UserStatusValidationMiddleware(RequestDelegate next, ILogger<UserStatusValidationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
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
                    // Get current user status from database
                    var currentUser = await userService.GetAsync(userId);
                    
                    if (currentUser == null)
                    {
                        _logger.LogWarning("User {UserId} not found in database but has valid token", userId);
                        await ReturnUnauthorized(context, "User account not found");
                        return;
                    }
                    
                    if (!currentUser.IsActive)
                    {
                        var username = context.User.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown";
                        _logger.LogWarning("Deactivated user {Username} (ID: {UserId}) attempted to access protected resource", username, userId);
                        await ReturnUnauthorized(context, "Account is deactivated");
                        return;
                    }
                    
                    // Optional: Validate role hasn't changed (for extra security)
                    var tokenRole = context.User.FindFirst(ClaimTypes.Role)?.Value;
                    if (tokenRole != null && tokenRole != currentUser.Role.ToString())
                    {
                        _logger.LogWarning("User {UserId} role changed from {TokenRole} to {CurrentRole}, invalidating token", 
                            userId, tokenRole, currentUser.Role);
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
