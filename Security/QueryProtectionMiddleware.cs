using System.Collections.Concurrent;
using System.Net;
using System.Text.Json;

namespace HotelBookingAPI.Security;

/// <summary>
/// Advanced rate limiting and query protection middleware
/// Prevents infinite queries, server overload, and abusive patterns
/// </summary>
public class QueryProtectionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<QueryProtectionMiddleware> _logger;
    
    // Rate limiting storage
    private static readonly ConcurrentDictionary<string, UserRequestInfo> _userRequests = new();
    private static readonly ConcurrentDictionary<string, EndpointStats> _endpointStats = new();
    
    // Configuration
    private static readonly TimeSpan RequestWindow = TimeSpan.FromMinutes(1);
    private static readonly TimeSpan BurstWindow = TimeSpan.FromSeconds(10);
    private static readonly TimeSpan CleanupInterval = TimeSpan.FromMinutes(5);
    private static DateTime _lastCleanup = DateTime.UtcNow;

    // Rate limits per endpoint type
    private static readonly Dictionary<string, RateLimitConfig> EndpointLimits = new()
    {
        // Authentication endpoints - stricter limits
        { "/api/auth/login", new RateLimitConfig(5, 20, 100) },
        { "/api/auth/register", new RateLimitConfig(3, 10, 50) },
        
        // User endpoints - moderate limits  
        { "/api/user", new RateLimitConfig(30, 100, 500) },
        
        // Room endpoints - higher limits (search/browse heavy)
        { "/api/room", new RateLimitConfig(60, 200, 1000) },
        
        // Default for other endpoints
        { "default", new RateLimitConfig(20, 80, 400) }
    };

    public QueryProtectionMiddleware(RequestDelegate next, ILogger<QueryProtectionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Cleanup old entries periodically
        await CleanupOldEntries();
        
        // Get client identifier
        var clientId = GetClientIdentifier(context);
        var endpoint = GetEndpointCategory(context.Request.Path);
        var now = DateTime.UtcNow;

        // Check rate limits
        var rateLimitResult = CheckRateLimits(clientId, endpoint, now);
        if (!rateLimitResult.IsAllowed)
        {
            await HandleRateLimitExceeded(context, rateLimitResult);
            return;
        }

        // Track request
        TrackRequest(clientId, endpoint, now);
        
        // Add rate limit headers
        AddRateLimitHeaders(context, rateLimitResult);

        await _next(context);
    }

    private string GetClientIdentifier(HttpContext context)
    {
        // Try to get user ID from JWT token first
        var userId = context.User?.FindFirst("sub")?.Value 
                    ?? context.User?.FindFirst("userId")?.Value;
        
        if (!string.IsNullOrEmpty(userId))
        {
            return $"user:{userId}";
        }

        // Fall back to IP address for unauthenticated requests
        var ipAddress = context.Connection.RemoteIpAddress?.ToString() 
                       ?? context.Request.Headers["X-Forwarded-For"].FirstOrDefault()
                       ?? context.Request.Headers["X-Real-IP"].FirstOrDefault()
                       ?? "unknown";
        
        return $"ip:{ipAddress}";
    }

    private string GetEndpointCategory(string path)
    {
        if (string.IsNullOrEmpty(path)) return "default";
        
        // Match specific endpoints
        foreach (var endpoint in EndpointLimits.Keys)
        {
            if (endpoint != "default" && path.StartsWith(endpoint, StringComparison.OrdinalIgnoreCase))
            {
                return endpoint;
            }
        }
        
        return "default";
    }

    private RateLimitResult CheckRateLimits(string clientId, string endpoint, DateTime now)
    {
        var config = EndpointLimits.GetValueOrDefault(endpoint, EndpointLimits["default"]);
        var userInfo = _userRequests.GetOrAdd(clientId, _ => new UserRequestInfo());

        lock (userInfo.Lock)
        {
            // Clean old requests
            userInfo.RequestTimes.RemoveAll(time => now - time > RequestWindow);
            userInfo.BurstRequests.RemoveAll(time => now - time > BurstWindow);

            // Check burst limit (10 seconds)
            if (userInfo.BurstRequests.Count >= config.BurstLimit)
            {
                _logger.LogWarning("Burst limit exceeded for {ClientId} on {Endpoint}: {Count} requests in 10s", 
                    clientId, endpoint, userInfo.BurstRequests.Count);
                
                return new RateLimitResult
                {
                    IsAllowed = false,
                    Reason = "Burst limit exceeded",
                    RetryAfter = BurstWindow,
                    RemainingRequests = 0
                };
            }

            // Check per-minute limit
            if (userInfo.RequestTimes.Count >= config.PerMinuteLimit)
            {
                _logger.LogWarning("Rate limit exceeded for {ClientId} on {Endpoint}: {Count} requests per minute", 
                    clientId, endpoint, userInfo.RequestTimes.Count);
                
                return new RateLimitResult
                {
                    IsAllowed = false,
                    Reason = "Rate limit exceeded",
                    RetryAfter = RequestWindow,
                    RemainingRequests = 0
                };
            }

            // Check daily limit
            var todayRequests = userInfo.DailyRequestCount.GetValueOrDefault(now.Date, 0);
            if (todayRequests >= config.DailyLimit)
            {
                _logger.LogWarning("Daily limit exceeded for {ClientId} on {Endpoint}: {Count} requests today", 
                    clientId, endpoint, todayRequests);
                
                return new RateLimitResult
                {
                    IsAllowed = false,
                    Reason = "Daily limit exceeded", 
                    RetryAfter = TimeSpan.FromDays(1),
                    RemainingRequests = 0
                };
            }

            return new RateLimitResult
            {
                IsAllowed = true,
                RemainingRequests = config.PerMinuteLimit - userInfo.RequestTimes.Count,
                RemainingBurst = config.BurstLimit - userInfo.BurstRequests.Count,
                RemainingDaily = config.DailyLimit - todayRequests
            };
        }
    }

    private void TrackRequest(string clientId, string endpoint, DateTime now)
    {
        var userInfo = _userRequests.GetOrAdd(clientId, _ => new UserRequestInfo());
        
        lock (userInfo.Lock)
        {
            userInfo.RequestTimes.Add(now);
            userInfo.BurstRequests.Add(now);
            
            var today = now.Date;
            userInfo.DailyRequestCount[today] = userInfo.DailyRequestCount.GetValueOrDefault(today, 0) + 1;
            
            // Clean old daily counts (keep only last 2 days)
            var oldDates = userInfo.DailyRequestCount.Keys.Where(date => now.Date - date > TimeSpan.FromDays(1)).ToList();
            foreach (var oldDate in oldDates)
            {
                userInfo.DailyRequestCount.Remove(oldDate);
            }
        }

        // Track endpoint statistics
        var endpointStat = _endpointStats.GetOrAdd(endpoint, _ => new EndpointStats());
        lock (endpointStat.Lock)
        {
            endpointStat.RequestCount++;
            endpointStat.LastRequest = now;
        }
    }

    private async Task HandleRateLimitExceeded(HttpContext context, RateLimitResult result)
    {
        context.Response.StatusCode = 429; // Too Many Requests
        context.Response.ContentType = "application/json";
        
        // Add retry-after header
        context.Response.Headers["Retry-After"] = ((int)result.RetryAfter.TotalSeconds).ToString();
        
        var response = new
        {
            error = new
            {
                code = 429,
                message = "Too many requests",
                details = result.Reason,
                retryAfter = (int)result.RetryAfter.TotalSeconds
            }
        };

        var json = JsonSerializer.Serialize(response);
        await context.Response.WriteAsync(json);
    }

    private void AddRateLimitHeaders(HttpContext context, RateLimitResult result)
    {
        if (result.IsAllowed)
        {
            context.Response.Headers["X-RateLimit-Remaining"] = result.RemainingRequests.ToString();
            context.Response.Headers["X-RateLimit-Burst-Remaining"] = result.RemainingBurst.ToString();
            context.Response.Headers["X-RateLimit-Daily-Remaining"] = result.RemainingDaily.ToString();
            context.Response.Headers["X-RateLimit-Reset"] = DateTimeOffset.UtcNow.Add(RequestWindow).ToUnixTimeSeconds().ToString();
        }
    }

    private async Task CleanupOldEntries()
    {
        if (DateTime.UtcNow - _lastCleanup < CleanupInterval) return;
        
        await Task.Run(() =>
        {
            var now = DateTime.UtcNow;
            var keysToRemove = new List<string>();

            foreach (var kvp in _userRequests)
            {
                var userInfo = kvp.Value;
                lock (userInfo.Lock)
                {
                    userInfo.RequestTimes.RemoveAll(time => now - time > RequestWindow);
                    userInfo.BurstRequests.RemoveAll(time => now - time > BurstWindow);
                    
                    // Remove user if no recent activity
                    if (!userInfo.RequestTimes.Any() && !userInfo.BurstRequests.Any())
                    {
                        keysToRemove.Add(kvp.Key);
                    }
                }
            }

            foreach (var key in keysToRemove)
            {
                _userRequests.TryRemove(key, out _);
            }

            _lastCleanup = now;
            
            if (keysToRemove.Count > 0)
            {
                _logger.LogInformation("Cleaned up {Count} inactive rate limit entries", keysToRemove.Count);
            }
        });
    }
}

/// <summary>
/// Rate limit configuration for different endpoint types
/// </summary>
public record RateLimitConfig(int BurstLimit, int PerMinuteLimit, int DailyLimit);

/// <summary>
/// Rate limit check result
/// </summary>
public class RateLimitResult
{
    public bool IsAllowed { get; set; }
    public string Reason { get; set; } = string.Empty;
    public TimeSpan RetryAfter { get; set; }
    public int RemainingRequests { get; set; }
    public int RemainingBurst { get; set; }
    public int RemainingDaily { get; set; }
}

/// <summary>
/// Per-user request tracking information
/// </summary>
public class UserRequestInfo
{
    public readonly object Lock = new();
    public List<DateTime> RequestTimes { get; } = new();
    public List<DateTime> BurstRequests { get; } = new();
    public Dictionary<DateTime, int> DailyRequestCount { get; } = new();
}

/// <summary>
/// Endpoint statistics tracking
/// </summary>
public class EndpointStats
{
    public readonly object Lock = new();
    public long RequestCount { get; set; }
    public DateTime LastRequest { get; set; }
}
