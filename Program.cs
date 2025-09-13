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

builder.Services.AddSingleton<RoomService>();
builder.Services.AddSingleton<UserService>();

// Add security services
builder.Services.AddSecurityServices(builder.Configuration);

// Configure and register MongoDbSettings
builder.Services.Configure<MongoDbSettings>(
    builder.Configuration.GetSection("MongoDbSettings")
);

builder.Services.AddSingleton<IMongoClient>(serviceProvider => 
{
    var settings = serviceProvider.GetRequiredService<IOptions<MongoDbSettings>>().Value;
    return new MongoClient(settings.ConnectionString);
});


// Add JWT Authentication
var jwtSettings = builder.Configuration.GetSection("Jwt")!;
var key = Encoding.ASCII.GetBytes(jwtSettings["Key"]!);

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
        ValidIssuer = jwtSettings["Issuer"]!, // Added !
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"]!, // Added !
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero // No leeway for token expiration
    };
});

builder.Services.AddAuthorization(); // Add authorization services

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Add security middleware
app.UseSecurityMiddleware();
app.UseSecurityHeaders();

app.UseAuthentication(); // Must be before UseAuthorization
app.UseAuthorization();  // Must be after UseAuthentication

app.MapControllers();

app.Run();
