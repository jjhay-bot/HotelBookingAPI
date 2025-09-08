namespace HotelBookingAPI.Models;

public class MongoDbSettings
{
    public string ConnectionString { get; set; } = null!;
    public string DatabaseName { get; set; } = null!;
    public string RoomsCollectionName { get; set; } = null!;
    public string UsersCollectionName { get; set; } = null!;
}