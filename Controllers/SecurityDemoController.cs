using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MongoDB.Bson;
using HotelBookingAPI.Models;
using HotelBookingAPI.Security;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using System.Text.Json;

namespace HotelBookingAPI.Controllers;

/// <summary>
/// Security demonstration controller showing injection attacks and prevention
/// WARNING: This controller contains vulnerable examples for educational purposes only!
/// DO NOT use vulnerable methods in production!
/// </summary>
[ApiController]
[Route("api/security-demo")]
public class SecurityDemoController : ControllerBase
{
    private readonly IMongoCollection<User> _usersCollection;
    private readonly ILogger<SecurityDemoController> _logger;

    public SecurityDemoController(IMongoClient mongoClient, ILogger<SecurityDemoController> logger)
    {
        var database = mongoClient.GetDatabase("HotelBookingDB");
        _usersCollection = database.GetCollection<User>("Users");
        _logger = logger;
    }

    /// <summary>
    /// VULNERABLE: Direct JSON deserialization can lead to NoSQL injection
    /// Example attack payload: {"username": {"$ne": ""}, "password": {"$ne": ""}}
    /// </summary>
    [HttpPost("vulnerable/login")]
    public async Task<IActionResult> VulnerableLogin([FromBody] JsonElement loginData)
    {
        try
        {
            // Convert JsonElement to JSON string
            var jsonString = loginData.GetRawText();
            
            // DANGEROUS: Direct parsing of user-provided JSON into BsonDocument
            var bsonDocument = BsonDocument.Parse(jsonString);
            
            _logger.LogWarning("SECURITY DEMO: Vulnerable login attempted with payload: {Payload}", 
                bsonDocument.ToJson());

            // DANGEROUS: Direct use of user-provided BsonDocument as MongoDB filter
            var filter = bsonDocument;
            var user = await _usersCollection.Find(filter).FirstOrDefaultAsync();

            if (user != null)
            {
                return Ok(new { message = "Login successful", userId = user.Id });
            }

            return Unauthorized(new { message = "Invalid credentials" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in vulnerable login");
            return BadRequest(new { message = "Login failed", error = ex.Message });
        }
    }

    /// <summary>
    /// SECURE: Proper validation and typed parameters prevent injection
    /// </summary>
    [HttpPost("secure/login")]
    public async Task<IActionResult> SecureLogin([FromBody] LoginRequest request)
    {
        try
        {
            // Input validation
            if (!InputValidator.IsValidUsername(request.Username))
            {
                _logger.LogWarning("Invalid username format attempted: {Username}", request.Username);
                return BadRequest(new { message = "Invalid username format" });
            }

            var passwordValidation = InputValidator.ValidatePassword(request.Password);
            if (passwordValidation != ValidationResult.Success)
            {
                return BadRequest(new { message = passwordValidation.ErrorMessage });
            }

            // Secure query using strongly typed filter
            var user = await _usersCollection
                .Find(u => u.Username == request.Username)
                .FirstOrDefaultAsync();

            if (user != null && PasswordHasher.VerifyPassword(request.Password, user.PasswordHash))
            {
                _logger.LogInformation("Successful login for user: {Username}", request.Username);
                return Ok(new { message = "Login successful", userId = user.Id });
            }

            _logger.LogWarning("Failed login attempt for username: {Username}", request.Username);
            return Unauthorized(new { message = "Invalid credentials" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in secure login");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// VULNERABLE: String concatenation in filter
    /// Example attack: ?username=admin"; db.users.drop(); var x="
    /// </summary>
    [HttpGet("vulnerable/user")]
    public async Task<IActionResult> VulnerableUserSearch([FromQuery] string username)
    {
        try
        {
            _logger.LogWarning("SECURITY DEMO: Vulnerable user search with username: {Username}", username);

            // DANGEROUS: This would be vulnerable if we were building queries as strings
            // MongoDB C# driver protects us, but showing the concept
            var filter = $"{{username: '{username}'}}";
            _logger.LogWarning("Would execute filter: {Filter}", filter);

            // Even though we log the dangerous filter, we use safe query
            var user = await _usersCollection
                .Find(u => u.Username == username)
                .FirstOrDefaultAsync();

            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in vulnerable user search");
            return BadRequest(new { message = "Search failed" });
        }
    }

    /// <summary>
    /// SECURE: Proper validation and sanitization
    /// </summary>
    [HttpGet("secure/user")]
    public async Task<IActionResult> SecureUserSearch([FromQuery] string username)
    {
        try
        {
            // Input validation and sanitization
            if (string.IsNullOrWhiteSpace(username))
            {
                return BadRequest(new { message = "Username is required" });
            }

            username = InputValidator.SanitizeString(username, 30);

            if (!InputValidator.IsValidUsername(username))
            {
                _logger.LogWarning("Invalid username format in search: {Username}", username);
                return BadRequest(new { message = "Invalid username format" });
            }

            // Secure query with validated input
            var user = await _usersCollection
                .Find(u => u.Username == username)
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            // Return safe user data (exclude password hash)
            var safeUser = new
            {
                user.Id,
                user.Username,
                // Don't return password hash or other sensitive data
            };

            _logger.LogInformation("Successful user search for: {Username}", username);
            return Ok(safeUser);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in secure user search for username: {Username}", username);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Demonstrates common injection attack patterns that would fail
    /// </summary>
    [HttpPost("attack-examples")]
    public IActionResult AttackExamples()
    {
        var attackExamples = new
        {
            NoSQLInjectionAttempts = new[]
            {
                new { 
                    Type = "Authentication Bypass",
                    Payload = """{"username": {"$ne": ""}, "password": {"$ne": ""}}""",
                    Description = "Attempts to bypass authentication using MongoDB $ne operator"
                },
                new {
                    Type = "Data Extraction",
                    Payload = """{"username": {"$regex": ".*"}}""",
                    Description = "Attempts to extract all usernames using regex"
                },
                new {
                    Type = "JavaScript Injection",
                    Payload = """{"$where": "this.username == 'admin' || '1' == '1'"}""",
                    Description = "Attempts to inject JavaScript code (if $where is enabled)"
                }
            },
            SQLInjectionEquivalents = new[]
            {
                new {
                    SQL = "SELECT * FROM users WHERE username = '' OR '1'='1' --",
                    NoSQL = """{"$or": [{"username": ""}, {"1": "1"}]}""",
                    Description = "Always true condition"
                },
                new {
                    SQL = "SELECT * FROM users; DROP TABLE users; --",
                    NoSQL = "Not directly applicable - MongoDB operations are atomic",
                    Description = "Multiple statement execution"
                }
            },
            PreventionMethods = new[]
            {
                "Use strongly typed queries with MongoDB C# driver",
                "Validate and sanitize all inputs",
                "Use parameterized queries (for SQL databases)",
                "Implement proper authentication and authorization",
                "Never trust user input",
                "Use allowlists for expected values",
                "Implement rate limiting",
                "Log and monitor for suspicious activities"
            }
        };

        return Ok(attackExamples);
    }

    /// <summary>
    /// Demonstrates secure query building patterns
    /// </summary>
    [HttpGet("secure-patterns")]
    public async Task<IActionResult> SecurePatterns([FromQuery] string? searchTerm, [FromQuery] int page = 1)
    {
        try
        {
            // Input validation
            if (page < 1) page = 1;
            if (page > 100) page = 100; // Limit pagination

            var pageSize = 10;
            var skip = (page - 1) * pageSize;

            FilterDefinition<User> filter = Builders<User>.Filter.Empty;

            // Secure search term handling
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = InputValidator.SanitizeString(searchTerm, 50);
                
                // Only allow alphanumeric search terms
                if (InputValidator.IsValidUsername(searchTerm))
                {
                    filter = Builders<User>.Filter.Regex(u => u.Username, 
                        new BsonRegularExpression($"^{Regex.Escape(searchTerm)}", "i"));
                }
                else
                {
                    return BadRequest(new { message = "Invalid search term format" });
                }
            }

            // Secure aggregation pipeline
            var users = await _usersCollection
                .Find(filter)
                .Skip(skip)
                .Limit(pageSize)
                .Project(u => new { u.Id, u.Username }) // Only return safe fields
                .ToListAsync();

            var totalCount = await _usersCollection.CountDocumentsAsync(filter);

            var result = new
            {
                Users = users,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in secure patterns demo");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }
}
