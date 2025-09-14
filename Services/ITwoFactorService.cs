using HotelBookingAPI.Models;

namespace HotelBookingAPI.Services
{
    public interface ITwoFactorService
    {
        /// <summary>
        /// Generates a new secret key for TOTP authentication
        /// </summary>
        string GenerateSecretKey();

        /// <summary>
        /// Generates QR code URI for authenticator apps
        /// </summary>
        string GenerateQrCodeUri(string userIdentifier, string secretKey, string issuer = "HotelBookingAPI");

        /// <summary>
        /// Verifies a TOTP code against the user's secret key
        /// </summary>
        bool VerifyTotpCode(string secretKey, string userCode);

        /// <summary>
        /// Generates recovery codes for 2FA backup
        /// </summary>
        List<string> GenerateRecoveryCodes(int count = 10);

        /// <summary>
        /// Enables 2FA for a user
        /// </summary>
        Task<(bool Success, string SecretKey, string QrCodeUri)> EnableTwoFactorAsync(User user);

        /// <summary>
        /// Disables 2FA for a user
        /// </summary>
        Task<bool> DisableTwoFactorAsync(string userId);

        /// <summary>
        /// Verifies 2FA code during login
        /// </summary>
        Task<bool> VerifyTwoFactorLoginAsync(string userId, string code, bool isRecoveryCode = false);

        /// <summary>
        /// Generates a temporary 2FA token for login process
        /// </summary>
        string GenerateTwoFactorToken(string userId);

        /// <summary>
        /// Validates a temporary 2FA token
        /// </summary>
        bool ValidateTwoFactorToken(string token, string userId);
    }
}
