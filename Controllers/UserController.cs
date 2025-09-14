using HotelBookingAPI.Models;
using HotelBookingAPI.Services;
using HotelBookingAPI.Security;
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
    [Authorize(Roles = "Admin,Manager")] // Only Admin and Manager can view all users
    public async Task<List<User>> Get() =>
        await _userService.GetAsync();

    [HttpGet("{id:length(24)}")]
    [Authorize] // Any authenticated user can view user details (could be restricted further)
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
    [Authorize(Roles = "Admin")] // Only Admin can update user data
    public async Task<IActionResult> Update(string id, UserUpdateRequest updateRequest)
    {
        var user = await _userService.GetAsync(id);

        if (user is null)
        {
            return NotFound(new ErrorResponse(new ErrorInfo(
                Code: StatusCodes.Status404NotFound,
                Message: $"User with ID {id} not found."
            )));
        }

        // Update username
        user.Username = updateRequest.Username;

        // Update password if provided
        if (!string.IsNullOrEmpty(updateRequest.Password))
        {
            // Validate password strength
            var passwordValidation = InputValidator.ValidatePassword(updateRequest.Password);
            if (passwordValidation != System.ComponentModel.DataAnnotations.ValidationResult.Success)
            {
                return BadRequest(new ErrorResponse(new ErrorInfo(
                    Code: StatusCodes.Status400BadRequest,
                    Message: passwordValidation?.ErrorMessage ?? "Password validation failed."
                )));
            }

            user.PasswordHash = PasswordHasher.HashPassword(updateRequest.Password);
        }

        // Update role if provided
        if (updateRequest.Role.HasValue)
        {
            user.Role = updateRequest.Role.Value;
        }

        await _userService.UpdateAsync(id, user);

        return NoContent();
    }

    [HttpPatch("{id:length(24)}")]
    [Authorize(Roles = "Admin")] // Only Admin can partially update user data
    public async Task<IActionResult> PartialUpdate(string id, UserPartialUpdateRequest partialUpdateRequest)
    {
        var existingUser = await _userService.GetAsync(id);

        if (existingUser is null)
        {
            return NotFound(new ErrorResponse(new ErrorInfo(
                Code: StatusCodes.Status404NotFound,
                Message: $"User with ID {id} not found."
            )));
        }

        // Only update provided properties
        if (!string.IsNullOrEmpty(partialUpdateRequest.Username))
        {
            if (!InputValidator.IsValidUsername(partialUpdateRequest.Username))
            {
                return BadRequest(new ErrorResponse(new ErrorInfo(
                    Code: StatusCodes.Status400BadRequest,
                    Message: "Invalid username format. Must be 3-30 characters with letters, numbers, underscore, or hyphen only."
                )));
            }
            existingUser.Username = partialUpdateRequest.Username;
        }
        
        // Update password if provided
        if (!string.IsNullOrEmpty(partialUpdateRequest.Password))
        {
            var passwordValidation = InputValidator.ValidatePassword(partialUpdateRequest.Password);
            if (passwordValidation != System.ComponentModel.DataAnnotations.ValidationResult.Success)
            {
                return BadRequest(new ErrorResponse(new ErrorInfo(
                    Code: StatusCodes.Status400BadRequest,
                    Message: passwordValidation?.ErrorMessage ?? "Password validation failed."
                )));
            }

            existingUser.PasswordHash = PasswordHasher.HashPassword(partialUpdateRequest.Password);
        }

        // Update role if provided
        if (partialUpdateRequest.Role.HasValue)
        {
            existingUser.Role = partialUpdateRequest.Role.Value;
        }

        await _userService.UpdateAsync(id, existingUser);

        return NoContent();
    }

    [HttpDelete("{id:length(24)}")]
    [Authorize(Roles = "Admin")] // Only Admin can delete users
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

    // Role management endpoints
    [HttpPut("{id:length(24)}/role")]
    [Authorize(Roles = "Admin")] // Only Admin can change user roles
    public async Task<IActionResult> UpdateUserRole(string id, [FromBody] UserRoleUpdateRequest request)
    {
        var success = await _userService.UpdateUserRoleAsync(id, request.Role);
        
        if (!success)
        {
            return NotFound(new ErrorResponse(new ErrorInfo(
                Code: StatusCodes.Status404NotFound,
                Message: $"User with ID {id} not found."
            )));
        }

        return NoContent();
    }

    [HttpPut("{id:length(24)}/deactivate")]
    [Authorize(Roles = "Admin")] // Only Admin can deactivate users
    public async Task<IActionResult> DeactivateUser(string id)
    {
        var success = await _userService.DeactivateUserAsync(id);
        
        if (!success)
        {
            return NotFound(new ErrorResponse(new ErrorInfo(
                Code: StatusCodes.Status404NotFound,
                Message: $"User with ID {id} not found."
            )));
        }

        return NoContent();
    }

    [HttpPut("{id:length(24)}/reactivate")]
    [Authorize(Roles = "Admin")] // Only Admin can reactivate users
    public async Task<IActionResult> ReactivateUser(string id)
    {
        var success = await _userService.ReactivateUserAsync(id);
        
        if (!success)
        {
            return NotFound(new ErrorResponse(new ErrorInfo(
                Code: StatusCodes.Status404NotFound,
                Message: $"User with ID {id} not found."
            )));
        }

        return NoContent();
    }

    [HttpGet("role/{role}")]
    [Authorize(Roles = "Admin,Manager")] // Admin and Manager can view users by role
    public async Task<List<User>> GetUsersByRole(UserRole role) =>
        await _userService.GetUsersByRoleAsync(role);
}

/// <summary>
/// Request model for updating user roles
/// </summary>
public record UserRoleUpdateRequest(UserRole Role);
