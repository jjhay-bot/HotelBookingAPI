using System.ComponentModel.DataAnnotations;

namespace HotelBookingAPI.DTOs
{
    public class Enable2FARequest
    {
        [Required]
        [StringLength(6, MinimumLength = 6)]
        public string VerificationCode { get; set; } = string.Empty;
    }

    public class Verify2FARequest
    {
        [Required]
        [StringLength(6, MinimumLength = 6)]
        public string Code { get; set; } = string.Empty;

        public bool IsRecoveryCode { get; set; } = false;
    }

    public class Disable2FARequest
    {
        [Required]
        [StringLength(6, MinimumLength = 6)]
        public string VerificationCode { get; set; } = string.Empty;
    }

    public class TwoFactorSetupResponse
    {
        public string SecretKey { get; set; } = string.Empty;
        public string QrCodeUri { get; set; } = string.Empty;
        public List<string> RecoveryCodes { get; set; } = new();
        public string ManualEntryCode { get; set; } = string.Empty;
    }

    public class TwoFactorLoginRequest
    {
        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        [Required]
        [StringLength(6, MinimumLength = 6)]
        public string TwoFactorCode { get; set; } = string.Empty;

        public bool IsRecoveryCode { get; set; } = false;
        
        [Required]
        public string TwoFactorToken { get; set; } = string.Empty;
    }

    public class TwoFactorLoginResponse
    {
        public string TwoFactorToken { get; set; } = string.Empty;
        public bool RequiresTwoFactor { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class RecoveryCodesResponse
    {
        public List<string> RecoveryCodes { get; set; } = new();
        public int RemainingCodes { get; set; }
    }
}
