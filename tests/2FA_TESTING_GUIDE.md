# 🔐 2FA Testing Guide

## Prerequisites
1. **Install Authenticator App** on your phone:
   - Google Authenticator (iOS/Android)
   - Microsoft Authenticator (iOS/Android)
   - Authy (iOS/Android)

2. **API Server Running**: http://localhost:8080

## 📱 Complete 2FA Testing Flow

### Step 1: Register a New User
```http
POST http://localhost:8080/api/Auth/register
Content-Type: application/json

{
  "username": "testuser2fa",
  "password": "TestPass123!"
}
```

**Expected Response:**
```json
{
  "message": "Registration successful",
  "user": {
    "id": "...",
    "username": "testuser2fa",
    "role": "User"
  },
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

### Step 2: Setup 2FA
```http
POST http://localhost:8080/api/2fa/setup
Authorization: Bearer YOUR_JWT_TOKEN_FROM_STEP1
```

**Expected Response:**
```json
{
  "secretKey": "JBSWY3DPEHPK3PXP",
  "qrCodeUri": "otpauth://totp/HotelBookingAPI:testuser2fa?secret=JBSWY3DPEHPK3PXP&issuer=HotelBookingAPI",
  "recoveryCodes": ["ABCD1234", "EFGH5678", ...],
  "manualEntryCode": "JBSWY3DPEHPK3PXP"
}
```

📱 **Action Required:**
1. Open your authenticator app
2. Scan the QR code or manually enter the secret key
3. The app will show a 6-digit code that changes every 30 seconds

### Step 3: Enable 2FA
```http
POST http://localhost:8080/api/2fa/enable
Authorization: Bearer YOUR_JWT_TOKEN
Content-Type: application/json

{
  "verificationCode": "123456"
}
```
*Replace "123456" with the actual code from your authenticator app*

**Expected Response:**
```json
{
  "message": "2FA has been successfully enabled"
}
```

### Step 4: Test 2FA Login Flow

#### 4a. First Login Attempt (Will require 2FA)
```http
POST http://localhost:8080/api/Auth/login
Content-Type: application/json

{
  "username": "testuser2fa",
  "password": "TestPass123!"
}
```

**Expected Response:**
```json
{
  "requiresTwoFactor": true,
  "twoFactorToken": "abc123def456...",
  "message": "Please provide your 2FA code"
}
```

#### 4b. Complete 2FA Login
```http
POST http://localhost:8080/api/Auth/login-2fa
Content-Type: application/json

{
  "username": "testuser2fa",
  "password": "TestPass123!",
  "twoFactorCode": "654321",
  "isRecoveryCode": false,
  "twoFactorToken": "abc123def456..."
}
```
*Use the 2FA token from step 4a and current code from authenticator app*

**Expected Response:**
```json
{
  "message": "2FA login successful",
  "user": {
    "id": "...",
    "username": "testuser2fa",
    "role": "User"
  },
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

### Step 5: Test Recovery Codes

#### 5a. Get Recovery Codes
```http
GET http://localhost:8080/api/2fa/status
Authorization: Bearer YOUR_JWT_TOKEN
```

#### 5b. Login with Recovery Code
```http
POST http://localhost:8080/api/Auth/login-2fa
Content-Type: application/json

{
  "username": "testuser2fa",
  "password": "TestPass123!",
  "twoFactorCode": "ABCD1234",
  "isRecoveryCode": true,
  "twoFactorToken": "your_2fa_token"
}
```
*Use one of the recovery codes from step 2*

## 🧪 Additional Tests

### Check 2FA Status
```http
GET http://localhost:8080/api/2fa/status
Authorization: Bearer YOUR_JWT_TOKEN
```

### Regenerate Recovery Codes
```http
POST http://localhost:8080/api/2fa/recovery-codes/regenerate
Authorization: Bearer YOUR_JWT_TOKEN
Content-Type: application/json

{
  "code": "123456"
}
```

### Disable 2FA
```http
POST http://localhost:8080/api/2fa/disable
Authorization: Bearer YOUR_JWT_TOKEN
Content-Type: application/json

{
  "verificationCode": "123456"
}
```

## 🔍 Testing Different Scenarios

1. **Without 2FA**: Regular login works normally
2. **With 2FA**: Login requires 2-step process
3. **Wrong 2FA Code**: Returns "Invalid 2FA code"
4. **Expired 2FA Token**: Returns "Invalid or expired 2FA token"
5. **Recovery Codes**: Can be used once, then removed from list
6. **Account Without 2FA**: Cannot use 2FA login endpoint

## 📝 QR Code Testing

When you get the `qrCodeUri` from step 2, you can:

1. **Scan with phone**: Use your authenticator app's scan feature
2. **Manual entry**: Use the `secretKey` to manually add the account
3. **Online QR Generator**: Paste the URI into a QR code generator to see the actual QR code

## 🛡️ Security Features You Can Test

- ✅ **Time-based codes**: Codes change every 30 seconds
- ✅ **Clock drift tolerance**: ±30 seconds window for validation
- ✅ **Single-use recovery codes**: Each recovery code can only be used once
- ✅ **Token expiration**: 2FA tokens expire in 10 minutes
- ✅ **Secure storage**: Secret keys are stored securely
- ✅ **Authentication required**: 2FA endpoints require valid JWT

## 🚨 Common Issues

1. **Clock sync**: Make sure your phone's time is accurate
2. **Code timing**: Enter the code quickly before it expires
3. **Case sensitivity**: Recovery codes are case-sensitive
4. **Token expiry**: Get a new 2FA token if yours expires

The API is now running and ready for 2FA testing! 🎉
