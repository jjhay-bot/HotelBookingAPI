using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace HotelBookingAPI.Models;

public class Room
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;

    [BsonElement("name")]
    public string Name { get; set; } = null!;

    [BsonElement("capacity")]
    public int Capacity { get; set; }
}

