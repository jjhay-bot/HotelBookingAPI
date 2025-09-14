using System.Security.Cryptography;
using System.Text;
using HotelBookingAPI.Models;

namespace HotelBookingAPI.Services
{
    public class TwoFactorService : ITwoFactorService
    {
        private readonly UserService _userService;
        private readonly ILogger<TwoFactorService> _logger;
        private readonly IConfiguration _configuration;
        private readonly Dictionary<string, (string Token, DateTime Expiry)> _twoFactorTokens = new();

        public TwoFactorService(
            UserService userService, 
            ILogger<TwoFactorService> logger,
            IConfiguration configuration)
        {
            _userService = userService;
            _logger = logger;
            _configuration = configuration;
        }

        public string GenerateSecretKey()
        {
            var key = new byte[20]; // 160-bit key
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(key);
            }
            return ToBase32String(key);
        }

        public string GenerateQrCodeUri(string userIdentifier, string secretKey, string issuer = "HotelBookingAPI")
        {
            // Use username or email - both work fine for 2FA
            if (string.IsNullOrWhiteSpace(userIdentifier))
            {
                userIdentifier = "user"; // Fallback identifier
            }
            
            var otpUri = $"otpauth://totp/{Uri.EscapeDataString(issuer)}:{Uri.EscapeDataString(userIdentifier)}?secret={secretKey}&issuer={Uri.EscapeDataString(issuer)}";
            return otpUri;
        }

        public bool VerifyTotpCode(string secretKey, string userCode)
        {
            try
            {
                var keyBytes = FromBase32String(secretKey);
                
                // Get current Unix timestamp divided by 30 (TOTP time step)
                var unixTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                var timeStep = unixTimestamp / 30;
                
                // Check current time step and ±1 steps for clock drift tolerance
                for (int i = -1; i <= 1; i++)
                {
                    var testTimeStep = timeStep + i;
                    var timeStepBytes = BitConverter.GetBytes(testTimeStep);
                    if (BitConverter.IsLittleEndian)
                        Array.Reverse(timeStepBytes);
                    
                    var hash = ComputeHmacSha1(keyBytes, timeStepBytes);
                    var code = GenerateCodeFromHash(hash);
                    
                    if (code == userCode)
                    {
                        return true;
                    }
                }
                
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying TOTP code");
                return false;
            }
        }

        public List<string> GenerateRecoveryCodes(int count = 10)
        {
            var codes = new List<string>();
            
            for (int i = 0; i < count; i++)
            {
                // Generate 8-character recovery code
                var code = GenerateRandomCode(8);
                codes.Add(code);
            }
            
            return codes;
        }

        public async Task<(bool Success, string SecretKey, string QrCodeUri)> EnableTwoFactorAsync(User user)
        {
            try
            {
                // Generate new secret key
                var secretKey = GenerateSecretKey();
                
                // Generate QR code URI using username (no email required)
                var qrCodeUri = GenerateQrCodeUri(user.Username, secretKey);
                
                // Generate recovery codes
                var recoveryCodes = GenerateRecoveryCodes();
                
                // Update user with 2FA settings
                user.TwoFactorSecret = secretKey;
                user.RecoveryCodes = recoveryCodes;
                user.IsTwoFactorEnabled = false; // Will be enabled after verification
                
                await _userService.UpdateAsync(user.Id!, user);
                
                _logger.LogInformation("2FA setup initiated for user {UserId}", user.Id);
                
                return (true, secretKey, qrCodeUri);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enabling 2FA for user {UserId}", user.Id);
                return (false, string.Empty, string.Empty);
            }
        }

        public async Task<bool> DisableTwoFactorAsync(string userId)
        {
            try
            {
                var user = await _userService.GetAsync(userId);
                if (user == null)
                    return false;

                user.IsTwoFactorEnabled = false;
                user.TwoFactorSecret = null;
                user.RecoveryCodes = new List<string>();
                
                await _userService.UpdateAsync(userId, user);
                
                _logger.LogInformation("2FA disabled for user {UserId}", userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disabling 2FA for user {UserId}", userId);
                return false;
            }
        }

        public async Task<bool> VerifyTwoFactorLoginAsync(string userId, string code, bool isRecoveryCode = false)
        {
            try
            {
                var user = await _userService.GetAsync(userId);
                if (user == null || !user.IsTwoFactorEnabled)
                    return false;

                if (isRecoveryCode)
                {
                    // Verify recovery code
                    if (user.RecoveryCodes.Contains(code))
                    {
                        // Remove used recovery code
                        user.RecoveryCodes.Remove(code);
                        await _userService.UpdateAsync(userId, user);
                        
                        _logger.LogInformation("Recovery code used for user {UserId}", userId);
                        return true;
                    }
                    return false;
                }
                else
                {
                    // Verify TOTP code
                    var isValid = VerifyTotpCode(user.TwoFactorSecret!, code);
                    if (isValid)
                    {
                        user.LastTwoFactorUsed = DateTime.UtcNow;
                        await _userService.UpdateAsync(userId, user);
                    }
                    return isValid;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying 2FA for user {UserId}", userId);
                return false;
            }
        }

        public string GenerateTwoFactorToken(string userId)
        {
            var token = Guid.NewGuid().ToString("N");
            var expiry = DateTime.UtcNow.AddMinutes(10); // 10-minute expiry
            
            _twoFactorTokens[token] = (userId, expiry);
            
            // Clean up expired tokens
            CleanupExpiredTokens();
            
            return token;
        }

        public bool ValidateTwoFactorToken(string token, string userId)
        {
            if (_twoFactorTokens.TryGetValue(token, out var tokenInfo))
            {
                if (tokenInfo.Token == userId && tokenInfo.Expiry > DateTime.UtcNow)
                {
                    _twoFactorTokens.Remove(token); // Single use token
                    return true;
                }
                
                _twoFactorTokens.Remove(token); // Remove expired token
            }
            
            return false;
        }

        // Helper methods for TOTP implementation
        private string ToBase32String(byte[] input)
        {
            if (input == null || input.Length == 0)
                return string.Empty;

            const string base32Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
            var result = new StringBuilder();
            
            for (int i = 0; i < input.Length; i += 5)
            {
                var chunk = new byte[5];
                var chunkLength = Math.Min(5, input.Length - i);
                Array.Copy(input, i, chunk, 0, chunkLength);
                
                var b1 = chunk[0];
                var b2 = chunkLength > 1 ? chunk[1] : (byte)0;
                var b3 = chunkLength > 2 ? chunk[2] : (byte)0;
                var b4 = chunkLength > 3 ? chunk[3] : (byte)0;
                var b5 = chunkLength > 4 ? chunk[4] : (byte)0;
                
                result.Append(base32Chars[b1 >> 3]);
                result.Append(base32Chars[((b1 & 0x07) << 2) | (b2 >> 6)]);
                result.Append(chunkLength > 1 ? base32Chars[(b2 >> 1) & 0x1F] : '=');
                result.Append(chunkLength > 1 ? base32Chars[((b2 & 0x01) << 4) | (b3 >> 4)] : '=');
                result.Append(chunkLength > 2 ? base32Chars[((b3 & 0x0F) << 1) | (b4 >> 7)] : '=');
                result.Append(chunkLength > 3 ? base32Chars[(b4 >> 2) & 0x1F] : '=');
                result.Append(chunkLength > 3 ? base32Chars[((b4 & 0x03) << 3) | (b5 >> 5)] : '=');
                result.Append(chunkLength > 4 ? base32Chars[b5 & 0x1F] : '=');
            }
            
            return result.ToString();
        }

        private byte[] FromBase32String(string input)
        {
            if (string.IsNullOrEmpty(input))
                return Array.Empty<byte>();

            const string base32Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
            input = input.TrimEnd('=').ToUpper();
            
            var result = new List<byte>();
            
            for (int i = 0; i < input.Length; i += 8)
            {
                var chunk = input.Substring(i, Math.Min(8, input.Length - i)).PadRight(8, '=');
                
                var values = new int[8];
                for (int j = 0; j < 8; j++)
                {
                    if (chunk[j] == '=')
                        values[j] = 0;
                    else
                        values[j] = base32Chars.IndexOf(chunk[j]);
                }
                
                result.Add((byte)((values[0] << 3) | (values[1] >> 2)));
                if (chunk[2] != '=') result.Add((byte)((values[1] << 6) | (values[2] << 1) | (values[3] >> 4)));
                if (chunk[4] != '=') result.Add((byte)((values[3] << 4) | (values[4] >> 1)));
                if (chunk[5] != '=') result.Add((byte)((values[4] << 7) | (values[5] << 2) | (values[6] >> 3)));
                if (chunk[7] != '=') result.Add((byte)((values[6] << 5) | values[7]));
            }
            
            return result.ToArray();
        }

        private byte[] ComputeHmacSha1(byte[] key, byte[] data)
        {
            using var hmac = new HMACSHA1(key);
            return hmac.ComputeHash(data);
        }

        private string GenerateCodeFromHash(byte[] hash)
        {
            var offset = hash[hash.Length - 1] & 0x0F;
            var binaryCode = ((hash[offset] & 0x7F) << 24) |
                           ((hash[offset + 1] & 0xFF) << 16) |
                           ((hash[offset + 2] & 0xFF) << 8) |
                           (hash[offset + 3] & 0xFF);
            
            var code = binaryCode % 1000000;
            return code.ToString("D6"); // 6-digit code with leading zeros
        }

        private string GenerateRandomCode(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var result = new StringBuilder();
            
            using (var rng = RandomNumberGenerator.Create())
            {
                var buffer = new byte[4];
                for (int i = 0; i < length; i++)
                {
                    rng.GetBytes(buffer);
                    var randomIndex = Math.Abs(BitConverter.ToInt32(buffer, 0)) % chars.Length;
                    result.Append(chars[randomIndex]);
                }
            }
            
            return result.ToString();
        }

        private void CleanupExpiredTokens()
        {
            var expiredTokens = _twoFactorTokens
                .Where(kvp => kvp.Value.Expiry <= DateTime.UtcNow)
                .Select(kvp => kvp.Key)
                .ToList();
                
            foreach (var token in expiredTokens)
            {
                _twoFactorTokens.Remove(token);
            }
        }
    }
}
