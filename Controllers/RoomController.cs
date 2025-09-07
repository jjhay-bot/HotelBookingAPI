using HotelBookingAPI.Models;
using HotelBookingAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http; // New

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
    [AllowAnonymous] // <-- Add this line
    public ActionResult<IEnumerable<Room>> GetAll()
    {
        var rooms = _roomService.GetAllRooms();
        return Ok(rooms);
    }

    [HttpGet("{id}")]
    [AllowAnonymous] // <-- Add this line
    public ActionResult<Room> GetById(int id)
    {
        var room = _roomService.GetById(id);

        if (room == null)
        {
            return NotFound(new ErrorResponse(new ErrorInfo(
                Code: StatusCodes.Status404NotFound,
                Message: $"Room with ID {id} not found."
            )));
        }

        return Ok(room);
    }

    [HttpPatch("{id}")]
    public IActionResult Update(int id, Room updatedRoom)
    {
        var existingRoom = _roomService.GetById(id);
        if (existingRoom == null)
        {
            return NotFound(new ErrorResponse(new ErrorInfo(
                Code: StatusCodes.Status404NotFound,
                Message: $"Room with ID {id} not found."
            )));
        }

        if (updatedRoom.Id != 0 && updatedRoom.Id != id)
        {
            return BadRequest(new ErrorResponse(new ErrorInfo(
                Code: StatusCodes.Status400BadRequest,
                Message:$"Room with ID {id} not found."
            )));
        }

        var result = _roomService.Update(id, updatedRoom);

        if (result == null)
        {
            return NotFound(new ErrorResponse(new ErrorInfo(
                Code: StatusCodes.Status404NotFound,
                Message: $"Room with ID {id} not found."
            )));
        }

        return NoContent();
    }

    [HttpPut("{id}")]
    public IActionResult Replace(int id, Room newRoom)
    {
        if (id != newRoom.Id)
        {
            return BadRequest(new ErrorResponse(new ErrorInfo(
                Code: StatusCodes.Status400BadRequest,
                Message: $"Room with ID {id} not found."
            )));
        }

        var result = _roomService.Replace(id, newRoom);

        if (result == null)
        {
            return NotFound(new ErrorResponse(new ErrorInfo(
                Code: StatusCodes.Status404NotFound,
                Message: $"Room with ID {id} not found for replacement."
            )));
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        var deleted = _roomService.Delete(id);

        if (!deleted)
        {
            return NotFound(new ErrorResponse(new ErrorInfo(
                Code: StatusCodes.Status404NotFound,
                Message: $"Room with ID {id} not found for deletion."
            )));
        }

        return NoContent();
    }
}