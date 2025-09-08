using HotelBookingAPI.Models;
using HotelBookingAPI.Services;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authorization;

namespace HotelBookingAPI.Controllers;

[ApiController]
[Route("api/[controller]")] // This will make the base route /api/Auth
public class AuthController : ControllerBase
{
    private readonly UserService _userService;
    private readonly IConfiguration _configuration;

    public AuthController(UserService userService, IConfiguration configuration)
    {
        _userService = userService;
        _configuration = configuration;
    }

    [HttpPost("register")] // This will make the full route /api/Auth/register
    [AllowAnonymous] // <-- Add this line
    public async Task<ActionResult<User>> Register(RegisterRequest request)
    {
        var user = await _userService.RegisterAsync(request.Username, request.Password);

        if (user == null)
        {
            // This could mean username already exists
            return Conflict(new ErrorResponse(new ErrorInfo(
                Code: StatusCodes.Status409Conflict,
                Message: "Username already exists."
            )));
        }

        // For security, you might want to return a DTO that doesn't expose PasswordHash
        return CreatedAtAction(nameof(Register), new { id = user.Id }, user);
    }

    [HttpPost("login")] // This will make the full route /api/Auth/login
    [AllowAnonymous] // <-- Add this line
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var user = await _userService.AuthenticateAsync(request.Username, request.Password);

        if (user == null)
        {
            return Unauthorized(new ErrorResponse(new ErrorInfo(
                Code: StatusCodes.Status401Unauthorized,
                Message: "Invalid username or password."
            )));
        }

        // --- JWT Generation ---
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]!);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.NameIdentifier, user.Id ?? string.Empty)
                // Add other claims like roles here if needed
            }),
            Expires = DateTime.UtcNow.AddDays(7), // Token valid for 7 days
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
            Issuer = _configuration["Jwt:Issuer"]!,
            Audience = _configuration["Jwt:Audience"]!
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);

        return Ok(new { Token = tokenString }); // Return the token
    }
}