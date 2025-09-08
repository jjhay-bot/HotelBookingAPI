using HotelBookingAPI.Models;
using HotelBookingAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace HotelBookingAPI.Controllers;

[ApiController]
[Route("api/users")]
[Authorize] // Protect user operations
public class UserController : ControllerBase
{
    private readonly UserService _userService;

    public UserController(UserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<List<User>> Get() =>
        await _userService.GetAsync();

    [HttpGet("{id:length(24)}")]
    public async Task<ActionResult<User>> Get(string id)
    {
        var user = await _userService.GetAsync(id);

        if (user is null)
        {
            return NotFound(new ErrorResponse(new ErrorInfo(
                Code: StatusCodes.Status404NotFound,
                Message: $"User with ID {id} not found."
            )));
        }

        return user;
    }

    [HttpPost]
    [AllowAnonymous] // Allow registration without authentication
    public async Task<IActionResult> Post(User newUser)
    {
        await _userService.CreateAsync(newUser);

        return CreatedAtAction(nameof(Get), new { id = newUser.Id }, newUser);
    }

    [HttpPut("{id:length(24)}")]
    public async Task<IActionResult> Update(string id, User updatedUser)
    {
        var user = await _userService.GetAsync(id);

        if (user is null)
        {
            return NotFound(new ErrorResponse(new ErrorInfo(
                Code: StatusCodes.Status404NotFound,
                Message: $"User with ID {id} not found."
            )));
        }

        updatedUser.Id = id; // Ensure the ID is set correctly

        await _userService.UpdateAsync(id, updatedUser);

        return NoContent();
    }

    [HttpPatch("{id:length(24)}")]
    public async Task<IActionResult> PartialUpdate(string id, User partialUser)
    {
        var existingUser = await _userService.GetAsync(id);

        if (existingUser is null)
        {
            return NotFound(new ErrorResponse(new ErrorInfo(
                Code: StatusCodes.Status404NotFound,
                Message: $"User with ID {id} not found."
            )));
        }

        // Only update non-null/non-default properties
        if (!string.IsNullOrEmpty(partialUser.Username))
            existingUser.Username = partialUser.Username;
        
        // Don't allow password hash updates through PATCH for security
        // Use a separate endpoint for password changes

        // Ensure the ID doesn't get changed
        existingUser.Id = id;

        await _userService.UpdateAsync(id, existingUser);

        return NoContent();
    }

    [HttpDelete("{id:length(24)}")]
    public async Task<IActionResult> Delete(string id)
    {
        var user = await _userService.GetAsync(id);

        if (user is null)
        {
            return NotFound(new ErrorResponse(new ErrorInfo(
                Code: StatusCodes.Status404NotFound,
                Message: $"User with ID {id} not found."
            )));
        }

        await _userService.RemoveAsync(id);

        return NoContent();
    }
}
