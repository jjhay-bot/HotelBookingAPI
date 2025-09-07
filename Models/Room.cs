// IMPORT NAME
namespace HotelBookingAPI.Models;

// MODERN WAY
// public record Room(int Id, string? Name, int Capacity);

// TRADITIONAL WAY
public class Room
{

    public int Id { get; set; }


    public string? Name { get; set; }
    public int Capacity { get; set; }
}

