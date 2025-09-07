using HotelBookingAPI.Models;

namespace HotelBookingAPI.Services;

public class RoomService
{
    private static readonly List<Room> Rooms = new List<Room>();
    private static int _nextId = 1;

    public Room CreateRoom(Room room)
    {
        room.Id = _nextId++;
        Rooms.Add(room);
        return room;
    }
}
