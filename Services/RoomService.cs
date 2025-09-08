using HotelBookingAPI.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace HotelBookingAPI.Services;

public class RoomService
{
    private readonly IMongoCollection<Room> _roomsCollection;

    public RoomService(IMongoClient mongoClient, IOptions<MongoDbSettings> mongoDbSettings)
    {
        var mongoDatabase = mongoClient.GetDatabase(mongoDbSettings.Value.DatabaseName);
        _roomsCollection = mongoDatabase.GetCollection<Room>(mongoDbSettings.Value.RoomsCollectionName);
    }

    public async Task<List<Room>> GetAsync() =>
        await _roomsCollection.Find(_ => true).ToListAsync();

    public async Task<Room?> GetAsync(string id) =>
        await _roomsCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

    public async Task CreateAsync(Room newRoom) =>
        await _roomsCollection.InsertOneAsync(newRoom);

    public async Task UpdateAsync(string id, Room updatedRoom) =>
        await _roomsCollection.ReplaceOneAsync(x => x.Id == id, updatedRoom);

    public async Task RemoveAsync(string id) =>
        await _roomsCollection.DeleteOneAsync(x => x.Id == id);
}
