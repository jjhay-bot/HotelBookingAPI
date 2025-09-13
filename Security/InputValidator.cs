using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace HotelBookingAPI.Security;

/// <summary>
/// Input validation utilities to prevent injection attacks
/// </summary>
public static class InputValidator
{
    // MongoDB ObjectId pattern (24 hex characters)
    private static readonly Regex MongoIdPattern = new(@"^[0-9a-fA-F]{24}$", RegexOptions.Compiled);
    
    // Username pattern (alphanumeric, underscore, hyphen, 3-30 chars)
    private static readonly Regex UsernamePattern = new(@"^[a-zA-Z0-9_-]{3,30}$", RegexOptions.Compiled);
    
    // Room number pattern (alphanumeric, 1-10 chars)
    private static readonly Regex RoomNumberPattern = new(@"^[a-zA-Z0-9]{1,10}$", RegexOptions.Compiled);

    /// <summary>
    /// Validates if a string is a valid MongoDB ObjectId
    /// </summary>
    public static bool IsValidMongoId(string? id)
    {
        return !string.IsNullOrEmpty(id) && MongoIdPattern.IsMatch(id);
    }

    /// <summary>
    /// Validates username format
    /// </summary>
    public static bool IsValidUsername(string? username)
    {
        return !string.IsNullOrEmpty(username) && UsernamePattern.IsMatch(username);
    }

    /// <summary>
    /// Validates password strength
    /// </summary>
    public static ValidationResult ValidatePassword(string? password)
    {
        if (string.IsNullOrEmpty(password))
            return new ValidationResult("Password is required");

        if (password.Length < 8)
            return new ValidationResult("Password must be at least 8 characters long");

        if (password.Length > 128)
            return new ValidationResult("Password must be less than 128 characters");

        if (!password.Any(char.IsUpper))
            return new ValidationResult("Password must contain at least one uppercase letter");

        if (!password.Any(char.IsLower))
            return new ValidationResult("Password must contain at least one lowercase letter");

        if (!password.Any(char.IsDigit))
            return new ValidationResult("Password must contain at least one number");

        if (!password.Any(ch => "!@#$%^&*()_+-=[]{}|;:,.<>?".Contains(ch)))
            return new ValidationResult("Password must contain at least one special character");

        return ValidationResult.Success!;
    }

    /// <summary>
    /// Validates room number format
    /// </summary>
    public static bool IsValidRoomNumber(string? roomNumber)
    {
        return !string.IsNullOrEmpty(roomNumber) && RoomNumberPattern.IsMatch(roomNumber);
    }

    /// <summary>
    /// Sanitizes string input to prevent injection attacks
    /// </summary>
    public static string SanitizeString(string? input, int maxLength = 1000)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        // Remove null characters and control characters
        var sanitized = new string(input.Where(c => !char.IsControl(c) || char.IsWhiteSpace(c)).ToArray());
        
        // Trim and limit length
        sanitized = sanitized.Trim();
        if (sanitized.Length > maxLength)
            sanitized = sanitized[..maxLength];

        return sanitized;
    }

    /// <summary>
    /// Validates email format
    /// </summary>
    public static bool IsValidEmail(string? email)
    {
        if (string.IsNullOrEmpty(email))
            return false;

        try
        {
            var emailAttribute = new EmailAddressAttribute();
            return emailAttribute.IsValid(email);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Validates price value (positive decimal)
    /// </summary>
    public static bool IsValidPrice(decimal price)
    {
        return price >= 0 && price <= 999999.99m;
    }

    /// <summary>
    /// Validates date range
    /// </summary>
    public static bool IsValidDateRange(DateTime? startDate, DateTime? endDate)
    {
        if (!startDate.HasValue || !endDate.HasValue)
            return false;

        return startDate.Value <= endDate.Value && 
               startDate.Value >= DateTime.Today &&
               endDate.Value <= DateTime.Today.AddYears(2);
    }
}
