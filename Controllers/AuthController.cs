using HotelBookingAPI.Models;
using HotelBookingAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace HotelBookingAPI.Controllers;

[ApiController]
[Route("api/[controller]")] // This will make the base route /api/Auth
public class AuthController : ControllerBase
{
    private readonly UserService _userService;

    public AuthController(UserService userService)
    {
        _userService = userService;
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

        // Return 201 Created with the new user (without password hash)
        // For security, you might want to return a DTO that doesn't expose PasswordHash
        return CreatedAtAction(nameof(Register), new { id = user.Id }, user);
    }
}
