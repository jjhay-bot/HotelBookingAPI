# 2FA Testing Guide - Fixed Version

## Overview
This guide walks you through testing the Two-Factor Authentication (2FA) functionality that has been implemented in the Hotel Booking API. **No email is required** - 2FA works with just username and password.

## Prerequisites
1. The API is running on `http://localhost:8080`
2. MongoDB is connected and running
3. You have an authenticator app (Google Authenticator, Authy, etc.) installed on your phone

## Test Files Available
- `test-2fa-working.http` - Clean version with placeholders (recommended)
- `test-2fa-simple.http` - Version with variables (now fixed)
- `test-2fa-flow.http` - Complete flow test
- `test-2fa-individual.http` - Individual endpoint tests

## Step-by-Step Testing Process

### Step 1: Register a New User
Use the registration endpoint to create a test user. **No email is required**.

```http
POST http://localhost:8080/api/Auth/register
Content-Type: application/json

{
  "username": "test2fa_user",
  "password": "TestPass123!"
}
```

**Expected Response:** 
- Status: 201 Created
- Message: "User registered successfully"

### Step 2: Login to Get JWT Token
Login with your new user credentials:

```http
POST http://localhost:8080/api/Auth/login
Content-Type: application/json

{
  "username": "test2fa_user",
  "password": "TestPass123!"
}
```

**Expected Response:**
- Status: 200 OK
- JWT token in response
- `requiresTwoFactor: false` (since 2FA is not enabled yet)

**ACTION:** Copy the JWT token for use in subsequent requests.

### Step 3: Setup 2FA
Get the QR code and setup key for your authenticator app:

```http
POST http://localhost:8080/api/2fa/setup
Authorization: Bearer YOUR_JWT_TOKEN
Content-Type: application/json
```

**Expected Response:**
- Status: 200 OK
- `qrCodeUri`: Use this to scan QR code with your authenticator app
- `manualEntryKey`: Alternative to QR code for manual entry
- `issuer`: "HotelBookingAPI"
- `username`: Your username (e.g., "test2fa_user")

**ACTION:** 
1. Open your authenticator app
2. Scan the QR code or manually enter the key
3. Your app will start generating 6-digit codes every 30 seconds

### Step 4: Enable 2FA
Enable 2FA using a code from your authenticator app:

```http
POST http://localhost:8080/api/2fa/enable
Authorization: Bearer YOUR_JWT_TOKEN
Content-Type: application/json

{
  "verificationCode": "123456"
}
```

**Expected Response:**
- Status: 200 OK
- Message: "Two-factor authentication enabled successfully"

**ACTION:** Replace "123456" with the current code from your authenticator app.

### Step 5: Test Regular Login (Now Requires 2FA)
Try logging in normally - this should now require 2FA:

```http
POST http://localhost:8080/api/Auth/login
Content-Type: application/json

{
  "username": "test2fa_user",
  "password": "TestPass123!"
}
```

**Expected Response:**
- Status: 200 OK
- `requiresTwoFactor: true`
- `twoFactorToken`: A temporary token for completing 2FA

**ACTION:** Copy the `twoFactorToken` for the next step.

### Step 6: Complete 2FA Login
Complete the login using your authenticator code and the two-factor token:

```http
POST http://localhost:8080/api/Auth/login-2fa
Content-Type: application/json

{
  "username": "test2fa_user",
  "password": "TestPass123!",
  "twoFactorCode": "456789",
  "isRecoveryCode": false,
  "twoFactorToken": "YOUR_TWO_FACTOR_TOKEN"
}
```

**Expected Response:**
- Status: 200 OK
- Full JWT token for authenticated access
- User details

**ACTION:** Replace "456789" with the current code from your authenticator app.

### Step 7: Get Recovery Codes
Generate backup recovery codes:

```http
POST http://localhost:8080/api/2fa/recovery-codes/regenerate
Authorization: Bearer YOUR_JWT_TOKEN
Content-Type: application/json

{
  "code": "789012"
}
```

**Expected Response:**
- Status: 200 OK
- Array of recovery codes (e.g., ["ABC123", "DEF456", ...])

**ACTION:** Save these recovery codes in a safe place. You can use them if you lose access to your authenticator app.

### Step 8: Test Recovery Code Login
Test logging in with a recovery code instead of an authenticator code:

```http
POST http://localhost:8080/api/Auth/login-2fa
Content-Type: application/json

{
  "username": "test2fa_user",
  "password": "TestPass123!",
  "twoFactorCode": "ABC123",
  "isRecoveryCode": true,
  "twoFactorToken": "YOUR_TWO_FACTOR_TOKEN"
}
```

**Expected Response:**
- Status: 200 OK
- Full JWT token for authenticated access

**Note:** Each recovery code can only be used once.

### Step 9: Check 2FA Status
Verify the current 2FA status:

```http
GET http://localhost:8080/api/2fa/status
Authorization: Bearer YOUR_JWT_TOKEN
```

**Expected Response:**
- Status: 200 OK
- `isEnabled: true`
- `hasRecoveryCodes: true`

### Step 10: Disable 2FA (Optional)
When you're done testing, you can disable 2FA:

```http
POST http://localhost:8080/api/2fa/disable
Authorization: Bearer YOUR_JWT_TOKEN
Content-Type: application/json

{
  "verificationCode": "345678"
}
```

**Expected Response:**
- Status: 200 OK
- Message: "Two-factor authentication disabled successfully"

## Troubleshooting

### Common Issues

1. **"Invalid verification code"**
   - Ensure your device's time is synchronized
   - The code changes every 30 seconds - use a fresh code
   - Check that you're using the correct authenticator app entry

2. **"Invalid two-factor token"**
   - The twoFactorToken expires - get a new one by doing a regular login
   - Ensure you're copying the token exactly without extra spaces

3. **"User not found" or "Invalid credentials"**
   - Verify the username and password are correct
   - Ensure the user was registered successfully

4. **JSON parsing errors**
   - Ensure there are no extra characters or line breaks in your JSON
   - Verify Content-Type is set to application/json
   - Check that variable assignments (@variable = value) are on separate lines

### Testing Tips

1. **Use the `test-2fa-working.http` file** - it has clean formatting and clear placeholders
2. **Test in order** - each step builds on the previous one
3. **Copy tokens carefully** - JWT tokens and two-factor tokens are long strings
4. **Time sensitivity** - Authenticator codes expire every 30 seconds
5. **Save recovery codes** - They're your backup if you lose your authenticator app

## What Works

✅ **No email required** - 2FA works with just username and password  
✅ **Standard TOTP** - Compatible with Google Authenticator, Authy, etc.  
✅ **Recovery codes** - Backup access method  
✅ **JWT integration** - Works with existing authentication  
✅ **Secure implementation** - Follows TOTP RFC 6238 standard  

## What's Implemented

- **TwoFactorService**: Core 2FA logic with TOTP generation
- **QR Code Generation**: For easy authenticator app setup
- **Recovery Codes**: Backup authentication method
- **2FA Login Flow**: Two-step authentication process
- **2FA Management**: Enable, disable, status endpoints
- **Security**: Proper token validation and expiration

The 2FA implementation is now fully functional and ready for testing!
