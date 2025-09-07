using System.Collections.Generic;

namespace HotelBookingAPI.Models;

public record ErrorInfo(int Code, string Message, IEnumerable<ErrorDetail>? Details = null);
