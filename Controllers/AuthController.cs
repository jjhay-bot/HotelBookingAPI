using HotelBookingAPI.Models;
using HotelBookingAPI.Services;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;

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
    public ActionResult<User> Register(RegisterRequest request)
    {
        var user = _userService.Register(request.Username, request.Password);

        if (user == null)
        {
            // This could mean username already exists
            return Conflict("Username already exists."); // HTTP 409 Conflict
        }

        // For security, you might want to return a DTO that doesn't expose PasswordHash
        return CreatedAtAction(nameof(Register), new { id = user.Id }, user);
    }

    [HttpPost("login")] // This will make the full route /api/Auth/login
    public IActionResult Login(LoginRequest request)
    {
        var user = _userService.Authenticate(request.Username, request.Password);

        if (user == null)
        {
            return Unauthorized("Invalid username or password."); // HTTP 401 Unauthorized
        }

        // --- JWT Generation ---
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
                // Add other claims like roles here if needed
            }),
            Expires = DateTime.UtcNow.AddDays(7), // Token valid for 7 days
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
            Issuer = _configuration["Jwt:Issuer"],
            Audience = _configuration["Jwt:Audience"]
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);

        return Ok(new { Token = tokenString }); // Return the token
    }
}