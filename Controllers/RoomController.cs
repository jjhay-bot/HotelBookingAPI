using HotelBookingAPI.Models;
using HotelBookingAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace HotelBookingAPI.Controllers;

[ApiController]
[Route("api/rooms")]
[Authorize] // Default protection for all room operations
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
    [Authorize(Roles = "Admin,Manager")] // Only Admin and Manager can create rooms
    public async Task<IActionResult> Post(RoomUpdateRequest createRequest)
    {
        var newRoom = new Room
        {
            Name = createRequest.Name,
            Capacity = createRequest.Capacity,
            PricePerNight = createRequest.PricePerNight,
            IsAvailable = createRequest.IsAvailable,
            RoomType = createRequest.RoomType,
            Description = createRequest.Description,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _roomService.CreateAsync(newRoom);

        return CreatedAtAction(nameof(Get), new { id = newRoom.Id }, newRoom);
    }

    [HttpPut("{id:length(24)}")]
    [Authorize(Roles = "Admin,Manager")] // Only Admin and Manager can update rooms
    public async Task<IActionResult> Update(string id, RoomUpdateRequest updateRequest)
    {
        var room = await _roomService.GetAsync(id);

        if (room is null)
        {
            return NotFound(new ErrorResponse(new ErrorInfo(
                Code: StatusCodes.Status404NotFound,
                Message: $"Room with ID {id} not found."
            )));
        }

        // Update room properties
        room.Name = updateRequest.Name;
        room.Capacity = updateRequest.Capacity;
        room.PricePerNight = updateRequest.PricePerNight;
        room.IsAvailable = updateRequest.IsAvailable;
        room.RoomType = updateRequest.RoomType;
        room.Description = updateRequest.Description;
        room.UpdatedAt = DateTime.UtcNow;

        await _roomService.UpdateAsync(id, room);

        return NoContent();
    }

    [HttpPatch("{id:length(24)}")]
    [Authorize(Roles = "Admin,Manager")] // Only Admin and Manager can partially update rooms
    public async Task<IActionResult> PartialUpdate(string id, RoomPartialUpdateRequest partialUpdateRequest)
    {
        var existingRoom = await _roomService.GetAsync(id);

        if (existingRoom is null)
        {
            return NotFound(new ErrorResponse(new ErrorInfo(
                Code: StatusCodes.Status404NotFound,
                Message: $"Room with ID {id} not found."
            )));
        }

        // Only update provided properties
        if (!string.IsNullOrEmpty(partialUpdateRequest.Name))
            existingRoom.Name = partialUpdateRequest.Name;
        
        if (partialUpdateRequest.Capacity.HasValue)
            existingRoom.Capacity = partialUpdateRequest.Capacity.Value;

        if (partialUpdateRequest.PricePerNight.HasValue)
            existingRoom.PricePerNight = partialUpdateRequest.PricePerNight.Value;

        if (partialUpdateRequest.IsAvailable.HasValue)
            existingRoom.IsAvailable = partialUpdateRequest.IsAvailable.Value;

        if (!string.IsNullOrEmpty(partialUpdateRequest.RoomType))
            existingRoom.RoomType = partialUpdateRequest.RoomType;

        if (!string.IsNullOrEmpty(partialUpdateRequest.Description))
            existingRoom.Description = partialUpdateRequest.Description;

        // Update timestamp
        existingRoom.UpdatedAt = DateTime.UtcNow;

        await _roomService.UpdateAsync(id, existingRoom);

        return NoContent();
    }

    [HttpDelete("{id:length(24)}")]
    [Authorize(Roles = "Admin")] // Only Admin can delete rooms
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
