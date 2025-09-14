using System.ComponentModel.DataAnnotations;

namespace HotelBookingAPI.Models;

/// <summary>
/// Request model for updating user information
/// </summary>
public class UserUpdateRequest
{
    [Required]
    [MinLength(3, ErrorMessage = "Username must be at least 3 characters long")]
    [MaxLength(50, ErrorMessage = "Username cannot exceed 50 characters")]
    public string Username { get; set; } = null!;

    [MinLength(8, ErrorMessage = "Password must be at least 8 characters long")]
    public string? Password { get; set; } // Optional - only set if changing password

    public UserRole? Role { get; set; } // Optional - only set if changing role
}

/// <summary>
/// Request model for partial user updates (PATCH)
/// </summary>
public class UserPartialUpdateRequest
{
    [MinLength(3, ErrorMessage = "Username must be at least 3 characters long")]
    [MaxLength(50, ErrorMessage = "Username cannot exceed 50 characters")]
    public string? Username { get; set; }

    [MinLength(8, ErrorMessage = "Password must be at least 8 characters long")]
    public string? Password { get; set; }

    public UserRole? Role { get; set; }
}
