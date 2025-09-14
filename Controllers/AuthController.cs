using HotelBookingAPI.Models;
using HotelBookingAPI.Services;
using HotelBookingAPI.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace HotelBookingAPI.Controllers;

[ApiController]
[Route("api/[controller]")] // This will make the base route /api/Auth
public class AuthController : ControllerBase
{
    private readonly UserService _userService;
    private readonly JwtTokenService _jwtTokenService;
    private readonly ITwoFactorService _twoFactorService;

    public AuthController(UserService userService, JwtTokenService jwtTokenService, ITwoFactorService twoFactorService)
    {
        _userService = userService;
        _jwtTokenService = jwtTokenService;
        _twoFactorService = twoFactorService;
    }

    [HttpPost("register")] // This will make the full route /api/Auth/register
    [AllowAnonymous]
    public async Task<ActionResult<User>> Register(RegisterRequest request)
    {
        // Default role is User, only Admin can create Admin/Manager accounts
        var user = await _userService.RegisterAsync(request.Username, request.Password, UserRole.User);

        if (user == null)
        {
            return Conflict(new ErrorResponse(new ErrorInfo(
                Code: StatusCodes.Status409Conflict,
                Message: "Username already exists or invalid input."
            )));
        }

        // Generate token for immediate login after registration
        var token = _jwtTokenService.GenerateToken(user);

        return Ok(new 
        { 
            Message = "Registration successful",
            User = new { user.Id, user.Username, user.Role },
            Token = token
        });
    }

    [HttpPost("login")] // This will make the full route /api/Auth/login
    [AllowAnonymous]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var user = await _userService.AuthenticateAsync(request.Username, request.Password);

        if (user == null)
        {
            return Unauthorized(new ErrorResponse(new ErrorInfo(
                Code: StatusCodes.Status401Unauthorized,
                Message: "Invalid username or password."
            )));
        }

        if (!user.IsActive)
        {
            return Unauthorized(new ErrorResponse(new ErrorInfo(
                Code: StatusCodes.Status401Unauthorized,
                Message: "Account is deactivated."
            )));
        }

        // Check if 2FA is enabled
        if (user.IsTwoFactorEnabled)
        {
            // Generate temporary 2FA token
            var twoFactorToken = _twoFactorService.GenerateTwoFactorToken(user.Id!);
            
            return Ok(new TwoFactorLoginResponse
            {
                RequiresTwoFactor = true,
                TwoFactorToken = twoFactorToken,
                Message = "Please provide your 2FA code"
            });
        }

        // Generate JWT token with role claims
        var token = _jwtTokenService.GenerateToken(user);

        return Ok(new 
        { 
            Message = "Login successful",
            User = new { user.Id, user.Username, user.Role },
            Token = token
        });
    }

    [HttpPost("login-2fa")] // Two-factor authentication login
    [AllowAnonymous]
    public async Task<IActionResult> LoginWith2FA(TwoFactorLoginRequest request)
    {
        // First verify the username/password like regular login
        var user = await _userService.AuthenticateAsync(request.Username, request.Password);

        if (user == null)
        {
            return Unauthorized(new ErrorResponse(new ErrorInfo(
                Code: StatusCodes.Status401Unauthorized,
                Message: "Invalid username or password."
            )));
        }

        if (!user.IsActive)
        {
            return Unauthorized(new ErrorResponse(new ErrorInfo(
                Code: StatusCodes.Status401Unauthorized,
                Message: "Account is deactivated."
            )));
        }

        if (!user.IsTwoFactorEnabled)
        {
            return BadRequest(new ErrorResponse(new ErrorInfo(
                Code: StatusCodes.Status400BadRequest,
                Message: "2FA is not enabled for this account."
            )));
        }

        // Validate the 2FA token
        if (!_twoFactorService.ValidateTwoFactorToken(request.TwoFactorToken, user.Id!))
        {
            return Unauthorized(new ErrorResponse(new ErrorInfo(
                Code: StatusCodes.Status401Unauthorized,
                Message: "Invalid or expired 2FA token."
            )));
        }

        // Verify the 2FA code
        var isCodeValid = await _twoFactorService.VerifyTwoFactorLoginAsync(user.Id!, request.TwoFactorCode, request.IsRecoveryCode);
        
        if (!isCodeValid)
        {
            return Unauthorized(new ErrorResponse(new ErrorInfo(
                Code: StatusCodes.Status401Unauthorized,
                Message: request.IsRecoveryCode ? "Invalid recovery code." : "Invalid 2FA code."
            )));
        }

        // Generate JWT token
        var token = _jwtTokenService.GenerateToken(user);

        return Ok(new 
        { 
            Message = "2FA login successful",
            User = new { user.Id, user.Username, user.Role },
            Token = token
        });
    }

    [HttpPost("register-admin")] // Admin endpoint to create Admin/Manager accounts
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<User>> RegisterAdmin(AdminRegisterRequest request)
    {
        var user = await _userService.RegisterAsync(request.Username, request.Password, request.Role);

        if (user == null)
        {
            return Conflict(new ErrorResponse(new ErrorInfo(
                Code: StatusCodes.Status409Conflict,
                Message: "Username already exists or invalid input."
            )));
        }

        return Ok(new 
        { 
            Message = "Admin/Manager registration successful",
            User = new { user.Id, user.Username, user.Role }
        });
    }
}

/// <summary>
/// Request model for admin registration
/// </summary>
public record AdminRegisterRequest(string Username, string Password, string Email, UserRole Role);