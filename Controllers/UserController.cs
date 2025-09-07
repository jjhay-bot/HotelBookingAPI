using HotelBookingAPI.Models;
using HotelBookingAPI.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic; // Needed for IEnumerable

namespace HotelBookingAPI.Controllers;

[ApiController]
[Route("api/users")] // This will make the base route /api/users
public class UserController : ControllerBase
{
    private readonly UserService _userService;

    public UserController(UserService userService)
    {
        _userService = userService;
    }

    [HttpGet] // This will make the full route GET /api/users
    public ActionResult<IEnumerable<User>> GetAll()
    {
        var users = _userService.GetAllUsers();
        // IMPORTANT SECURITY NOTE: In a real app, you would map User to a DTO
        // that does NOT expose PasswordHash before returning.
        return Ok(users);
    }
}
