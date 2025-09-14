using System.ComponentModel.DataAnnotations;

namespace HotelBookingAPI.Models;

/// <summary>
/// Request model for updating room information
/// </summary>
public class RoomUpdateRequest
{
    [Required]
    [MinLength(1, ErrorMessage = "Room name cannot be empty")]
    [MaxLength(100, ErrorMessage = "Room name cannot exceed 100 characters")]
    public string Name { get; set; } = null!;

    [Range(1, 20, ErrorMessage = "Capacity must be between 1 and 20")]
    public int Capacity { get; set; }

    [Range(0.01, 10000.00, ErrorMessage = "Price per night must be between $0.01 and $10,000")]
    public decimal PricePerNight { get; set; }

    public bool IsAvailable { get; set; } = true;

    [MaxLength(50, ErrorMessage = "Room type cannot exceed 50 characters")]
    public string? RoomType { get; set; }

    [MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string? Description { get; set; }
}

/// <summary>
/// Request model for partial room updates (PATCH)
/// </summary>
public class RoomPartialUpdateRequest
{
    [MinLength(1, ErrorMessage = "Room name cannot be empty")]
    [MaxLength(100, ErrorMessage = "Room name cannot exceed 100 characters")]
    public string? Name { get; set; }

    [Range(1, 20, ErrorMessage = "Capacity must be between 1 and 20")]
    public int? Capacity { get; set; }

    [Range(0.01, 10000.00, ErrorMessage = "Price per night must be between $0.01 and $10,000")]
    public decimal? PricePerNight { get; set; }

    public bool? IsAvailable { get; set; }

    [MaxLength(50, ErrorMessage = "Room type cannot exceed 50 characters")]
    public string? RoomType { get; set; }

    [MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string? Description { get; set; }
}
