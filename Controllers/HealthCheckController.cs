using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Threading.Tasks;

namespace HotelBookingAPI.Controllers
{
    [ApiController]
    [Route("api/health")]
    public class HealthCheckController : ControllerBase
    {
        private readonly IMongoClient _mongoClient;

        public HealthCheckController(IMongoClient mongoClient)
        {
            _mongoClient = mongoClient;
        }

        [HttpGet("db")]
        public async Task<IActionResult> CheckDatabaseConnection()
        {
            try
            {
                var database = _mongoClient.GetDatabase("admin");
                await database.RunCommandAsync((Command<BsonDocument>)"{ ping: 1 }");
                return Ok("Successfully connected to the database.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Failed to connect to the database: {ex.Message}");
            }
        }
    }
}
