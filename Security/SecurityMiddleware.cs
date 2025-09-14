using System.Net;
using System.Text.Json;

namespace HotelBookingAPI.Security;

/// <summary>
/// Security middleware that adds protection against common attacks
/// </summary>
public class SecurityMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<SecurityMiddleware> _logger;
    private readonly Dictionary<string, DateTime> _rateLimitStore = new();
    private readonly object _rateLimitLock = new();

    public SecurityMiddleware(RequestDelegate next, ILogger<SecurityMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Add security headers
        AddSecurityHeaders(context);

        // Check for suspicious patterns
        if (await CheckForSuspiciousActivity(context))
        {
            await BlockRequest(context, "Suspicious activity detected");
            return;
        }

        // Basic rate limiting
        if (IsRateLimited(context))
        {
            await BlockRequest(context, "Rate limit exceeded");
            return;
        }

        // [📌] 
        // Validate request size
        if (context.Request.ContentLength > 1024 * 1024) // 1MB limit
        {
            await BlockRequest(context, "Request too large");
            return;
        }

        await _next(context);
    }

    private void AddSecurityHeaders(HttpContext context)
    {
        var response = context.Response;

        // Prevent clickjacking
        // OPTIONS for X-Frame-Options:
        // 1. "DENY" - Never allow framing (most secure)
        // 2. "SAMEORIGIN" - Allow framing only from same domain
        // 3. "ALLOW-FROM https://trusted-site.com" - Allow specific domain (deprecated in modern browsers)
        // 
        // WHAT DOES "SAME ORIGIN" MEAN?
        // Same Origin = Same Protocol + Same Domain + Same Port
        // Examples:
        // ✅ SAME ORIGIN:
        //    - https://myapp.com/frontend trying to frame https://myapp.com/api
        //    - https://localhost:3000/page trying to frame https://localhost:3000/api
        // ❌ DIFFERENT ORIGIN:
        //    - https://myapp.com trying to frame http://myapp.com (different protocol)
        //    - https://myapp.com trying to frame https://api.myapp.com (different subdomain)
        //    - https://localhost:3000 trying to frame https://localhost:5268 (different port)
        //    - https://frontend.com trying to frame https://backend.com (different domain)
        //
        // COMMON SCENARIOS:
        // 1. Frontend & Backend on SAME domain (e.g., both on myapp.com) → Use SAMEORIGIN
        // 2. Frontend & Backend on DIFFERENT domains/ports → Use CSP frame-ancestors with specific domains
        // 3. Development: Frontend on :3000, Backend on :5268 → Use CSP frame-ancestors
        //
        // MODERN ALTERNATIVE: Use Content-Security-Policy frame-ancestors directive instead
        // Example: "frame-ancestors 'self' https://trusted-site.com https://another-trusted.com;"
        //
        // TO ALLOW SPECIFIC WEBSITES:
        // Option A: Change to SAMEORIGIN (allows your own domain)
        // response.Headers.Append("X-Frame-Options", "SAMEORIGIN");
        //
        // Option B: Use CSP frame-ancestors (recommended for modern browsers)
        // Add to the CSP below: frame-ancestors 'self' https://trusted-domain.com;
        response.Headers.Append("X-Frame-Options", "DENY");

        // Prevent MIME type sniffing
        response.Headers.Append("X-Content-Type-Options", "nosniff");

        // XSS Protection
        response.Headers.Append("X-XSS-Protection", "1; mode=block");

        // Content Security Policy
        // TO ALLOW SPECIFIC WEBSITES TO FRAME YOUR API:
        // Add frame-ancestors directive with allowed domains:
        // "frame-ancestors 'self' https://trusted-site.com https://another-trusted.com;"
        // 
        // CURRENT CSP: Very restrictive, only allows same-origin resources
        // TO ALLOW EXTERNAL RESOURCES: Modify the directives below
        // Examples:
        // - script-src 'self' https://cdn.trusted.com - Allow scripts from CDN
        // - img-src 'self' data: https://images.trusted.com - Allow images from external domain
        response.Headers.Append("Content-Security-Policy",
            "default-src 'self'; script-src 'self'; style-src 'self' 'unsafe-inline'; img-src 'self' data:; font-src 'self'");

        // Strict Transport Security (HTTPS only)
        if (context.Request.IsHttps)
        {
            response.Headers.Append("Strict-Transport-Security", "max-age=31536000; includeSubDomains");
        }

        // Referrer Policy
        response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");

        // Feature Policy
        response.Headers.Append("Permissions-Policy", "geolocation=(), microphone=(), camera=()");
    }

    // [📌] Check for suspicious activity
    private async Task<bool> CheckForSuspiciousActivity(HttpContext context)
    {
        var request = context.Request;

        // Check for common injection patterns in query parameters
        foreach (var param in request.Query)
        {
            var value = param.Value.ToString().ToLower();

            // SQL injection patterns
            if (value.Contains("' or ") || value.Contains("' and ") ||
                value.Contains("union select") || value.Contains("drop table") ||
                value.Contains("delete from") || value.Contains("insert into"))
            {
                _logger.LogWarning("SQL injection attempt detected from {IP}: {Pattern}",
                    context.Connection.RemoteIpAddress, value);
                return true;
            }

            // NoSQL injection patterns
            if (value.Contains("$ne") || value.Contains("$gt") || value.Contains("$lt") ||
                value.Contains("$regex") || value.Contains("$where") || value.Contains("$eval"))
            {
                _logger.LogWarning("NoSQL injection attempt detected from {IP}: {Pattern}",
                    context.Connection.RemoteIpAddress, value);
                return true;
            }

            // Script injection patterns
            if (value.Contains("<script") || value.Contains("javascript:") ||
                value.Contains("onload=") || value.Contains("onerror="))
            {
                _logger.LogWarning("Script injection attempt detected from {IP}: {Pattern}",
                    context.Connection.RemoteIpAddress, value);
                return true;
            }
        }

        // Check request body for POST/PUT requests
        if (request.Method == "POST" || request.Method == "PUT")
        {
            if (request.ContentType?.Contains("application/json") == true)
            {
                request.EnableBuffering();
                var body = await new StreamReader(request.Body).ReadToEndAsync();
                request.Body.Position = 0;

                if (ContainsSuspiciousJsonPatterns(body))
                {
                    _logger.LogWarning("Suspicious JSON payload detected from {IP}: {Body}",
                        context.Connection.RemoteIpAddress, body);
                    return true;
                }
            }
        }

        return false;
    }

    // [📌] 
    private bool ContainsSuspiciousJsonPatterns(string json)
    {
        var suspiciousPatterns = new[]
        {
            "\"$ne\":", "\"$gt\":", "\"$lt\":", "\"$regex\":", "\"$where\":", "\"$eval\":",
            "\"$or\":", "\"$and\":", "\"$not\":", "\"$nor\":", "\"$exists\":",
            "function(", "javascript:", "<script", "eval(", "setTimeout("
        };

        return suspiciousPatterns.Any(pattern => json.Contains(pattern, StringComparison.OrdinalIgnoreCase));
    }

    // [📌] 
    private bool IsRateLimited(HttpContext context)
    {
        var clientIP = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var now = DateTime.UtcNow;

        lock (_rateLimitLock)
        {
            // Simple rate limiting: 100 requests per minute per IP
            var key = $"{clientIP}:{now:yyyy-MM-dd-HH-mm}";

            if (_rateLimitStore.ContainsKey(key))
            {
                // Clean old entries
                var keysToRemove = _rateLimitStore.Keys
                    .Where(k => _rateLimitStore[k] < now.AddMinutes(-2))
                    .ToList();

                foreach (var oldKey in keysToRemove)
                {
                    _rateLimitStore.Remove(oldKey);
                }

                // Check if rate limit exceeded
                var requestCount = _rateLimitStore.Count(kvp => kvp.Key.StartsWith($"{clientIP}:"));
                if (requestCount > 100)
                {
                    _logger.LogWarning("Rate limit exceeded for IP: {IP}", clientIP);
                    return true;
                }
            }

            _rateLimitStore[key] = now;
        }

        return false;
    }

    private async Task BlockRequest(HttpContext context, string reason)
    {
        _logger.LogWarning("Blocking request from {IP}: {Reason}",
            context.Connection.RemoteIpAddress, reason);

        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
        context.Response.ContentType = "application/json";

        var response = new
        {
            error = "Request blocked",
            message = "Your request has been blocked due to security policies",
            timestamp = DateTime.UtcNow
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}
