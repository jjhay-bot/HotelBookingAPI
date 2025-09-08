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
}

// Original design - keeping as reference
// public record User(int Id, string Username, string PasswordHash);
