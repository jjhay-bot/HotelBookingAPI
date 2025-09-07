using HotelBookingAPI.Models;
using System.Collections.Generic;
using System.Linq;

namespace HotelBookingAPI.Services;

public class UserService
{
    private static readonly List<User> _users = new List<User>();
    private static int _nextUserId = 1;

    // Placeholder for password hashing. In a real app, use a strong library like BCrypt.NET
    private string HashPassword(string password)
    {
        // For demonstration, a very simple "hash"
        return password + "_hashed"; 
    }

    // Placeholder for password verification
    private bool VerifyPassword(string password, string hashedPassword)
    {
        return HashPassword(password) == hashedPassword;
    }

    public User? Register(string username, string password)
    {
        if (_users.Any(u => u.Username == username))
        {
            return null; // Username already exists
        }

        var newUser = new User(
            Id: _nextUserId++,
            Username: username,
            PasswordHash: HashPassword(password)
        );

        _users.Add(newUser);
        return newUser;
    }

    public User? Authenticate(string username, string password)
    {
        var user = _users.FirstOrDefault(u => u.Username == username);
        if (user == null)
        {
            return null; // User not found
        }

        if (!VerifyPassword(password, user.PasswordHash))
        {
            return null; // Password incorrect
        }

        return user;
    }

    public IEnumerable<User> GetAllUsers()
    {
        return _users;
    }
}
