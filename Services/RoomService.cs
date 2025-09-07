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

    public IEnumerable<Room> GetAllRooms()
    {
        return Rooms;
    }

    public Room? GetById(int id)
    {
        return Rooms.FirstOrDefault(r => r.Id == id);
    }
    public Room? Update(int id, Room updatedRoom)
    {
        var existingRoom = GetById(id);
        if (existingRoom == null)
        {
            return null;
        }

        if (updatedRoom.Name != null)
        {
            existingRoom.Name = updatedRoom.Name;
        }

        if (updatedRoom.Capacity > 0)
        {
            existingRoom.Capacity = updatedRoom.Capacity;
        }

        return existingRoom;
    }

    public Room? Replace(int id, Room newRoom)
    {
        var index = Rooms.FindIndex(r => r.Id == id);
        if (index == -1)
        {
            return null; // Room not found
        }

        newRoom.Id = id;
        Rooms[index] = newRoom; // Replace the room at the found index

        return newRoom;
    }

    public bool Delete(int id)
    {
        var roomToRemove = GetById(id);
        if (roomToRemove == null)
        {
            return false; // Room not found
        }

        Rooms.Remove(roomToRemove);
        return true; // Room successfully deleted
    }
}
