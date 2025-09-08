using HotelBookingAPI.Models;
using HotelBookingAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

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

    [HttpGet]
    [AllowAnonymous]
    public async Task<List<Room>> Get() =>
        await _roomService.GetAsync();

    [HttpGet("{id:length(24)}")]
    [AllowAnonymous]
    public async Task<ActionResult<Room>> Get(string id)
    {
        var room = await _roomService.GetAsync(id);

        if (room is null)
        {
            return NotFound(new ErrorResponse(new ErrorInfo(
                Code: StatusCodes.Status404NotFound,
                Message: $"Room with ID {id} not found."
            )));
        }

        return room;
    }

    [HttpPost]
    [AllowAnonymous] // Temporarily added for testing
    public async Task<IActionResult> Post(Room newRoom)
    {
        await _roomService.CreateAsync(newRoom);

        return CreatedAtAction(nameof(Get), new { id = newRoom.Id }, newRoom);
    }

    [HttpPut("{id:length(24)}")]
    public async Task<IActionResult> Update(string id, Room updatedRoom)
    {
        var room = await _roomService.GetAsync(id);

        if (room is null)
        {
            return NotFound(new ErrorResponse(new ErrorInfo(
                Code: StatusCodes.Status404NotFound,
                Message: $"Room with ID {id} not found."
            )));
        }

        updatedRoom.Id = id; // Ensure the ID is set correctly

        await _roomService.UpdateAsync(id, updatedRoom);

        return NoContent();
    }

    [HttpPatch("{id:length(24)}")]
    public async Task<IActionResult> PartialUpdate(string id, Room partialRoom)
    {
        var existingRoom = await _roomService.GetAsync(id);

        if (existingRoom is null)
        {
            return NotFound(new ErrorResponse(new ErrorInfo(
                Code: StatusCodes.Status404NotFound,
                Message: $"Room with ID {id} not found."
            )));
        }

        // Only update non-null/non-default properties
        if (!string.IsNullOrEmpty(partialRoom.Name))
            existingRoom.Name = partialRoom.Name;
        
        if (partialRoom.Capacity > 0)
            existingRoom.Capacity = partialRoom.Capacity;

        // Ensure the ID doesn't get changed
        existingRoom.Id = id;

        await _roomService.UpdateAsync(id, existingRoom);

        return NoContent();
    }

    [HttpDelete("{id:length(24)}")]
    public async Task<IActionResult> Delete(string id)
    {
        var room = await _roomService.GetAsync(id);

        if (room is null)
        {
            return NotFound(new ErrorResponse(new ErrorInfo(
                Code: StatusCodes.Status404NotFound,
                Message: $"Room with ID {id} not found."
            )));
        }

        await _roomService.RemoveAsync(id);

        return NoContent();
    }
}
