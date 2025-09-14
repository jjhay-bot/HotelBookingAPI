using HotelBookingAPI.Models;
using HotelBookingAPI.Security;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;

namespace HotelBookingAPI.Services;

public class UserService
{
    private readonly IMongoCollection<User> _usersCollection;

    public UserService(IMongoClient mongoClient, IOptions<MongoDbSettings> mongoDbSettings)
    {
        var mongoDatabase = mongoClient.GetDatabase(mongoDbSettings.Value.DatabaseName);
        _usersCollection = mongoDatabase.GetCollection<User>(mongoDbSettings.Value.UsersCollectionName);
    }

    // MongoDB CRUD Operations
    public async Task<List<User>> GetAsync() =>
        await _usersCollection.Find(_ => true).ToListAsync();

    public async Task<User?> GetAsync(string id) =>
        await _usersCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

    public async Task<User?> GetByUsernameAsync(string username) =>
        await _usersCollection.Find(x => x.Username == username).FirstOrDefaultAsync();

    public async Task CreateAsync(User newUser) =>
        await _usersCollection.InsertOneAsync(newUser);

    public async Task UpdateAsync(string id, User updatedUser) =>
        await _usersCollection.ReplaceOneAsync(x => x.Id == id, updatedUser);

    public async Task RemoveAsync(string id) =>
        await _usersCollection.DeleteOneAsync(x => x.Id == id);

    // Authentication methods
    public async Task<User?> RegisterAsync(string username, string password, UserRole role = UserRole.User, string? email = null)
    {
        // Input validation
        if (!InputValidator.IsValidUsername(username))
        {
            return null; // Invalid username format
        }

        var passwordValidation = InputValidator.ValidatePassword(password);
        if (passwordValidation != System.ComponentModel.DataAnnotations.ValidationResult.Success)
        {
            return null; // Invalid password
        }

        // Optional email validation
        if (!string.IsNullOrWhiteSpace(email) && !email.Contains("@"))
        {
            return null; // Invalid email format if provided
        }

        var existingUser = await GetByUsernameAsync(username);
        if (existingUser != null)
        {
            return null; // Username already exists
        }

        var newUser = new User
        {
            Username = username,
            Email = email ?? $"{username}@hotelapi.local", // Provide default email for 2FA
            PasswordHash = PasswordHasher.HashPassword(password),
            Role = role,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        await CreateAsync(newUser);
        return newUser;
    }

    public async Task<User?> AuthenticateAsync(string username, string password)
    {
        // Input validation
        if (!InputValidator.IsValidUsername(username))
        {
            return null; // Invalid username format
        }

        var user = await GetByUsernameAsync(username);
        if (user == null)
        {
            return null; // User not found
        }

        if (!PasswordHasher.VerifyPassword(password, user.PasswordHash))
        {
            return null; // Password incorrect
        }

        return user;
    }

    // Role management methods
    public async Task<bool> UpdateUserRoleAsync(string userId, UserRole newRole)
    {
        var user = await GetAsync(userId);
        if (user == null)
        {
            return false; // User not found
        }

        user.Role = newRole;
        await UpdateAsync(userId, user);
        return true;
    }

    public async Task<bool> DeactivateUserAsync(string userId)
    {
        var user = await GetAsync(userId);
        if (user == null)
        {
            return false; // User not found
        }

        user.IsActive = false;
        await UpdateAsync(userId, user);
        return true;
    }

    public async Task<bool> ReactivateUserAsync(string userId)
    {
        var user = await GetAsync(userId);
        if (user == null)
        {
            return false; // User not found
        }

        user.IsActive = true;
        await UpdateAsync(userId, user);
        return true;
    }

    public async Task<List<User>> GetUsersByRoleAsync(UserRole role)
    {
        return await _usersCollection.Find(x => x.Role == role && x.IsActive).ToListAsync();
    }

    // Legacy method for backward compatibility
    public IEnumerable<User> GetAllUsers()
    {
        return GetAsync().Result;
    }
}
