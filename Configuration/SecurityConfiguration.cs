using HotelBookingAPI.Security;

namespace HotelBookingAPI.Configuration;

/// <summary>
/// Security configuration extensions for the application
/// </summary>
public static class SecurityConfiguration
{
    /// <summary>
    /// Configures security services for the application
    /// </summary>
    public static IServiceCollection AddSecurityServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Add CORS with restrictive policy
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(builder =>
            {
                builder
                    .WithOrigins(configuration.GetSection("AllowedOrigins").Get<string[]>() ?? new[] { "https://localhost:3000" })
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            });
        });

        // Add rate limiting (basic implementation)
        services.AddMemoryCache();
        
        return services;
    }

    /// <summary>
    /// Configures security middleware pipeline
    /// </summary>
    public static IApplicationBuilder UseSecurityMiddleware(this IApplicationBuilder app)
    {
        // Use HTTPS redirection
        app.UseHttpsRedirection();

        // Use security middleware (custom)
        app.UseMiddleware<SecurityMiddleware>();

        // Use CORS
        app.UseCors();

        return app;
    }

    /// <summary>
    /// Configures additional security headers
    /// </summary>
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app)
    {
        app.Use(async (context, next) =>
        {
            // Remove server header for security
            context.Response.Headers.Remove("Server");
            
            // Add custom security headers
            context.Response.Headers.Append("X-Powered-By", "ASP.NET Core");
            context.Response.Headers.Append("X-Application", "HotelBookingAPI");
            
            await next();
        });

        return app;
    }
}
