using HotelBookingAPI.Models;
using HotelBookingAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace HotelBookingAPI.Controllers;

[ApiController]
[Route("api/rooms")]
public class RoomController : ControllerBase
{
    private readonly RoomService _roomService;

    public RoomController(RoomService roomService)
    {
        _roomService = roomService;
    }

    [HttpPost]
    public ActionResult<Room> Create(Room room)
    {
        var newRoom = _roomService.CreateRoom(room);
        return CreatedAtAction(nameof(Create), new { id = newRoom.Id }, newRoom);
    }

    [HttpGet]
    public ActionResult<IEnumerable<Room>> GetAll()
    {
        var rooms = _roomService.GetAllRooms();
        return Ok(rooms);
    }
}
