using HotelBookingAPI.Models;
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
    public async Task<User?> RegisterAsync(string username, string password)
    {
        var existingUser = await GetByUsernameAsync(username);
        if (existingUser != null)
        {
            return null; // Username already exists
        }

        var newUser = new User
        {
            Username = username,
            PasswordHash = HashPassword(password)
        };

        await CreateAsync(newUser);
        return newUser;
    }

    public async Task<User?> AuthenticateAsync(string username, string password)
    {
        var user = await GetByUsernameAsync(username);
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

    // Legacy method for backward compatibility
    public IEnumerable<User> GetAllUsers()
    {
        return GetAsync().Result;
    }
}
