namespace HotelBookingAPI.Models;

/// <summary>
/// User roles for authorization
/// </summary>
public enum UserRole
{
    /// <summary>
    /// Regular user - can view and book rooms
    /// </summary>
    User = 0,
    
    /// <summary>
    /// Manager - can manage rooms and view user data
    /// </summary>
    Manager = 1,
    
    /// <summary>
    /// Administrator - full system access
    /// </summary>
    Admin = 2
}
