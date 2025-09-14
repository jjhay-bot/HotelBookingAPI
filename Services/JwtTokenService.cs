using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using HotelBookingAPI.Models;

namespace HotelBookingAPI.Services;

/// <summary>
/// Service for generating and validating JWT tokens
/// </summary>
public class JwtTokenService
{
    private readonly IConfiguration _configuration;
    private readonly byte[] _key;

    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
        _key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]!);
    }

    /// <summary>
    /// Generates a JWT token for a user with role claims
    /// </summary>
    /// <param name="user">The user to generate token for</param>
    /// <returns>JWT token string</returns>
    // [🔒] Generate token
    public string GenerateToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id!),
                new Claim(ClaimTypes.Name, user.Username),
                // [🔒] 
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim("userId", user.Id!),
                new Claim("role", user.Role.ToString()),
                new Claim("isActive", user.IsActive.ToString())
            }),
            Expires = DateTime.UtcNow.AddHours(24), // Token expires in 24 hours
            Issuer = _configuration["Jwt:Issuer"],
            Audience = _configuration["Jwt:Audience"],
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(_key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    /// <summary>
    /// Validates a JWT token and returns the claims
    /// </summary>
    /// <param name="token">The token to validate</param>
    /// <returns>Claims principal if valid, null if invalid</returns>

    // [🔒] Validate token
    public ClaimsPrincipal? ValidateToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(_key),
                ValidateIssuer = true,
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidateAudience = true,
                ValidAudience = _configuration["Jwt:Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
            return principal;
        }
        catch
        {
            return null;
        }
    }
}
