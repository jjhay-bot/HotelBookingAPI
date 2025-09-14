using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HotelBookingAPI.DTOs;
using HotelBookingAPI.Services;
using HotelBookingAPI.Models;

namespace HotelBookingAPI.Controllers
{
    [ApiController]
    [Route("api/2fa")]
    [Authorize]
    public class TwoFactorController : ControllerBase
    {
        private readonly ITwoFactorService _twoFactorService;
        private readonly UserService _userService;
        private readonly ILogger<TwoFactorController> _logger;

        public TwoFactorController(
            ITwoFactorService twoFactorService,
            UserService userService,
            ILogger<TwoFactorController> logger)
        {
            _twoFactorService = twoFactorService;
            _userService = userService;
            _logger = logger;
        }

        /// <summary>
        /// Setup 2FA for the current user
        /// </summary>
        [HttpPost("setup")]
        public async Task<IActionResult> Setup2FA()
        {
            try
            {
                var userId = User.FindFirst("userId")?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var user = await _userService.GetAsync(userId);
                if (user == null)
                    return NotFound("User not found");

                if (user.IsTwoFactorEnabled)
                    return BadRequest("2FA is already enabled for this user");

                var (success, secretKey, qrCodeUri) = await _twoFactorService.EnableTwoFactorAsync(user);
                
                if (!success)
                    return StatusCode(500, "Failed to setup 2FA");

                var recoveryCodes = _twoFactorService.GenerateRecoveryCodes();

                return Ok(new TwoFactorSetupResponse
                {
                    SecretKey = secretKey,
                    QrCodeUri = qrCodeUri,
                    RecoveryCodes = recoveryCodes,
                    ManualEntryCode = secretKey
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting up 2FA");
                return StatusCode(500, "An error occurred while setting up 2FA");
            }
        }

        /// <summary>
        /// Enable 2FA after verifying the setup code
        /// </summary>
        [HttpPost("enable")]
        public async Task<IActionResult> Enable2FA([FromBody] Enable2FARequest request)
        {
            try
            {
                var userId = User.FindFirst("userId")?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var user = await _userService.GetAsync(userId);
                if (user == null)
                    return NotFound("User not found");

                if (user.IsTwoFactorEnabled)
                    return BadRequest("2FA is already enabled");

                if (string.IsNullOrEmpty(user.TwoFactorSecret))
                    return BadRequest("2FA setup not found. Please run setup first.");

                // Verify the provided code
                var isValid = _twoFactorService.VerifyTotpCode(user.TwoFactorSecret, request.VerificationCode);
                
                if (!isValid)
                    return BadRequest("Invalid verification code");

                // Enable 2FA
                user.IsTwoFactorEnabled = true;
                await _userService.UpdateAsync(userId, user);

                _logger.LogInformation("2FA enabled for user {UserId}", userId);

                return Ok(new { message = "2FA has been successfully enabled" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enabling 2FA");
                return StatusCode(500, "An error occurred while enabling 2FA");
            }
        }

        /// <summary>
        /// Disable 2FA for the current user
        /// </summary>
        [HttpPost("disable")]
        public async Task<IActionResult> Disable2FA([FromBody] Disable2FARequest request)
        {
            try
            {
                var userId = User.FindFirst("userId")?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var user = await _userService.GetAsync(userId);
                if (user == null)
                    return NotFound("User not found");

                if (!user.IsTwoFactorEnabled)
                    return BadRequest("2FA is not enabled for this user");

                // Verify the provided code before disabling
                var isValid = _twoFactorService.VerifyTotpCode(user.TwoFactorSecret!, request.VerificationCode);
                
                if (!isValid)
                    return BadRequest("Invalid verification code");

                var success = await _twoFactorService.DisableTwoFactorAsync(userId);
                
                if (!success)
                    return StatusCode(500, "Failed to disable 2FA");

                _logger.LogInformation("2FA disabled for user {UserId}", userId);

                return Ok(new { message = "2FA has been successfully disabled" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disabling 2FA");
                return StatusCode(500, "An error occurred while disabling 2FA");
            }
        }

        /// <summary>
        /// Get 2FA status for the current user
        /// </summary>
        [HttpGet("status")]
        public async Task<IActionResult> Get2FAStatus()
        {
            try
            {
                var userId = User.FindFirst("userId")?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var user = await _userService.GetAsync(userId);
                if (user == null)
                    return NotFound("User not found");

                return Ok(new
                {
                    isEnabled = user.IsTwoFactorEnabled,
                    recoveryCodesRemaining = user.RecoveryCodes?.Count ?? 0,
                    lastUsed = user.LastTwoFactorUsed
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting 2FA status");
                return StatusCode(500, "An error occurred while getting 2FA status");
            }
        }

        /// <summary>
        /// Generate new recovery codes (replaces existing ones)
        /// </summary>
        [HttpPost("recovery-codes/regenerate")]
        public async Task<IActionResult> RegenerateRecoveryCodes([FromBody] Verify2FARequest request)
        {
            try
            {
                var userId = User.FindFirst("userId")?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var user = await _userService.GetAsync(userId);
                if (user == null)
                    return NotFound("User not found");

                if (!user.IsTwoFactorEnabled)
                    return BadRequest("2FA is not enabled for this user");

                // Verify the provided code before regenerating recovery codes
                var isValid = _twoFactorService.VerifyTotpCode(user.TwoFactorSecret!, request.Code);
                
                if (!isValid)
                    return BadRequest("Invalid verification code");

                // Generate new recovery codes
                var newRecoveryCodes = _twoFactorService.GenerateRecoveryCodes();
                user.RecoveryCodes = newRecoveryCodes;
                
                await _userService.UpdateAsync(userId, user);

                _logger.LogInformation("Recovery codes regenerated for user {UserId}", userId);

                return Ok(new RecoveryCodesResponse
                {
                    RecoveryCodes = newRecoveryCodes,
                    RemainingCodes = newRecoveryCodes.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error regenerating recovery codes");
                return StatusCode(500, "An error occurred while regenerating recovery codes");
            }
        }

        /// <summary>
        /// Get remaining recovery codes count
        /// </summary>
        [HttpGet("recovery-codes/count")]
        public async Task<IActionResult> GetRecoveryCodesCount()
        {
            try
            {
                var userId = User.FindFirst("userId")?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var user = await _userService.GetAsync(userId);
                if (user == null)
                    return NotFound("User not found");

                return Ok(new
                {
                    remainingCodes = user.RecoveryCodes?.Count ?? 0
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recovery codes count");
                return StatusCode(500, "An error occurred while getting recovery codes count");
            }
        }
    }
}
