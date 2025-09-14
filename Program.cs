using HotelBookingAPI.Services;
using HotelBookingAPI.Security;
using HotelBookingAPI.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using HotelBookingAPI.Models;
using Microsoft.AspNetCore.Http; // Added for StatusCodes
using MongoDB.Driver;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Configure for deployment platforms
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(int.Parse(port));
});

// Add Environment Variables support
builder.Configuration.AddEnvironmentVariables();

// Add services to the container.
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Where(ms => ms.Value!.Errors.Any()) // Added !
                .Select(ms => new ErrorDetail(
                    Field: ms.Key,
                    Message: ms.Value!.Errors.First().ErrorMessage // Added !
                ))
                .ToList();

            var errorInfo = new ErrorInfo(
                Code: StatusCodes.Status400BadRequest,
                Message: "One or more validation errors occurred.",
                Details: errors
            );

            var errorResponse = new ErrorResponse(errorInfo);

            return new BadRequestObjectResult(errorResponse)
            {
                ContentTypes = { "application/json" }
            };
        };
    });

// Register services with proper lifetime
builder.Services.AddScoped<RoomService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<JwtTokenService>();

// Add Memory Cache for server-side caching
builder.Services.AddMemoryCache();

// Add security services
builder.Services.AddSecurityServices(builder.Configuration);

// Configure and register MongoDbSettings with environment variable fallback
builder.Services.Configure<MongoDbSettings>(options =>
{
    var configSection = builder.Configuration.GetSection("MongoDbSettings");
    configSection.Bind(options);
    
    // Override with environment variables if available
    options.ConnectionString = Environment.GetEnvironmentVariable("MONGODB_CONNECTION_STRING") ?? options.ConnectionString;
    options.DatabaseName = Environment.GetEnvironmentVariable("DATABASE_NAME") ?? options.DatabaseName ?? "HotelBookingDb";
    options.RoomsCollectionName = options.RoomsCollectionName ?? "Rooms";
    options.UsersCollectionName = options.UsersCollectionName ?? "Users";
});

builder.Services.AddSingleton<IMongoClient>(serviceProvider => 
{
    var settings = serviceProvider.GetRequiredService<IOptions<MongoDbSettings>>().Value;
    
    // Get connection string from environment if not in config
    var connectionString = settings.ConnectionString ?? Environment.GetEnvironmentVariable("MONGODB_CONNECTION_STRING");
    
    if (string.IsNullOrEmpty(connectionString))
    {
        throw new InvalidOperationException("MongoDB connection string is not configured. Please set MONGODB_CONNECTION_STRING environment variable or MongoDbSettings:ConnectionString in appsettings.json");
    }
    
    return new MongoClient(connectionString);
});


// Add JWT Authentication with error handling
var jwtSettings = builder.Configuration.GetSection("Jwt");
var jwtKey = jwtSettings["Key"] ?? Environment.GetEnvironmentVariable("JWT_SECRET");
var jwtIssuer = jwtSettings["Issuer"] ?? Environment.GetEnvironmentVariable("JWT_ISSUER") ?? "HotelBookingAPI";
var jwtAudience = jwtSettings["Audience"] ?? Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? "HotelBookingAPIUsers";

if (string.IsNullOrEmpty(jwtKey))
{
    throw new InvalidOperationException("JWT Key is not configured. Please set JWT_SECRET environment variable or Jwt:Key in appsettings.json");
}

var key = Encoding.ASCII.GetBytes(jwtKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // Set to true in production
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,
        ValidateAudience = true,
        ValidAudience = jwtAudience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero // No leeway for token expiration
    };
});

builder.Services.AddAuthorization(); // Add authorization services

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Add some startup logging to help debug deployment issues
var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Application starting up...");
logger.LogInformation("Environment: {Environment}", app.Environment.EnvironmentName);
logger.LogInformation("Port: {Port}", port);

try
{
    // Test MongoDB connection at startup
    var mongoClient = app.Services.GetRequiredService<IMongoClient>();
    var database = mongoClient.GetDatabase("admin");
    await database.RunCommandAsync<MongoDB.Bson.BsonDocument>(new MongoDB.Bson.BsonDocument("ping", 1));
    logger.LogInformation("MongoDB connection successful");
}
catch (Exception ex)
{
    logger.LogError(ex, "MongoDB connection failed");
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Add security middleware
app.UseQueryProtection(); // Add query protection before other middleware
app.UseSecurityMiddleware();
app.UseSecurityHeaders();

app.UseAuthentication(); // Must be before UseAuthorization
app.UseAuthorization();  // Must be after UseAuthentication

app.MapControllers();

app.Run();
