using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace HotelBookingAPI.Models;

public class User
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("username")]
    public string Username { get; set; } = null!;

    [BsonElement("passwordHash")]
    [JsonIgnore] // This prevents PasswordHash from appearing in API responses
    public string PasswordHash { get; set; } = null!;

    [BsonElement("role")]
    public UserRole Role { get; set; } = UserRole.User; // Default to User role

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("isActive")]
    public bool IsActive { get; set; } = true;

    // Two-Factor Authentication properties (email optional)
    [BsonElement("email")]
    public string? Email { get; set; }

    [BsonElement("isTwoFactorEnabled")]
    public bool IsTwoFactorEnabled { get; set; } = false;

    [BsonElement("twoFactorSecret")]
    [JsonIgnore] // Don't expose secret in API responses
    public string? TwoFactorSecret { get; set; }

    [BsonElement("recoveryCodes")]
    [JsonIgnore] // Don't expose recovery codes in API responses
    public List<string> RecoveryCodes { get; set; } = new();

    [BsonElement("lastTwoFactorUsed")]
    public DateTime? LastTwoFactorUsed { get; set; }
}

// Original design - keeping as reference
// public record User(int Id, string Username, string PasswordHash);
