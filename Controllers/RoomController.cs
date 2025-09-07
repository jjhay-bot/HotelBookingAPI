using HotelBookingAPI.Models;
using HotelBookingAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace HotelBookingAPI.Controllers;

[ApiController]
[Route("api/rooms")]
[Authorize]
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

    [HttpGet("{id}")]
    public ActionResult<Room> GetById(int id)
    {
        var room = _roomService.GetById(id);

        if (room == null)
        {
            return NotFound(); // Returns HTTP 404 Not Found
        }

        return Ok(room); // Returns HTTP 200 OK with the room data
    }

    [HttpPatch("{id}")]
    public IActionResult Update(int id, Room updatedRoom)
    {
        var existingRoom = _roomService.GetById(id);
        if (existingRoom == null)
        {
            return NotFound();
        }

        // Ensure the ID from the URL matches the ID in the body (if provided)
        // This is a common validation step for PUT/PATCH
        if (updatedRoom.Id != 0 && updatedRoom.Id != id)
        {
            return BadRequest("Room ID not found.");
        }

        // Update the room using the service
        var result = _roomService.Update(id, updatedRoom);

        if (result == null)
        {
            // This case should ideally not happen if existingRoom was found,
            // but good for robustness.
            return NotFound();
        }

        return NoContent(); // Returns HTTP 204 No Content
    }

    [HttpPut("{id}")]
    public IActionResult Replace(int id, Room newRoom)
    {
        // Validate that the ID in the URL matches the ID in the body
        if (id != newRoom.Id)
        {
            return BadRequest("Room ID not found.");
        }

        var result = _roomService.Replace(id, newRoom);

        if (result == null)
        {
            return NotFound(); // Returns HTTP 404 Not Found
        }

        return NoContent(); // Returns HTTP 204 No Content
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        var deleted = _roomService.Delete(id);

        if (!deleted)
        {
            return NotFound(); // Returns HTTP 404 Not Found
        }

        return NoContent(); // Returns HTTP 204 No Content
    }
}
