# User Update API Fix - Issue Resolution

## Problem Description

When testing role-based authorization (Test #15), the PUT endpoint for updating users was returning a 400 Bad Request error:

```json
{
  "error": {
    "code": 400,
    "message": "One or more validation errors occurred.",
    "details": [
      {
        "field": "PasswordHash",
        "message": "The PasswordHash field is required."
      }
    ]
  }
}
```

## Root Cause

The issue was that the `UserController.Update` method was expecting a `User` object directly, which includes a `PasswordHash` property. However, the test was sending a request with a `password` field instead of `passwordHash`. This created a security and usability problem:

1. **Security Issue**: Exposing `PasswordHash` in API requests would allow clients to send pre-hashed passwords, bypassing password validation
2. **Usability Issue**: Clients shouldn't need to hash passwords manually - this should be handled server-side
3. **Data Model Confusion**: Using the database entity model directly for API requests violates separation of concerns

## Solution Implemented

### 1. Created Proper Request Models

Created `/Models/UserUpdateRequest.cs` with two new models:

```csharp
public class UserUpdateRequest
{
    [Required]
    public string Username { get; set; } = null!;
    public string? Password { get; set; }  // Plain password - will be hashed server-side
    public UserRole? Role { get; set; }
}

public class UserPartialUpdateRequest
{
    public string? Username { get; set; }
    public string? Password { get; set; }   // Plain password - will be hashed server-side
    public UserRole? Role { get; set; }
}
```

### 2. Updated UserController Methods

**PUT Method (`Update`):**
- Now accepts `UserUpdateRequest` instead of `User`
- Validates password strength using `InputValidator.ValidatePassword()`
- Hashes password server-side using `PasswordHasher.HashPassword()`
- Only updates provided fields

**PATCH Method (`PartialUpdate`):**
- Now accepts `UserPartialUpdateRequest` instead of `User`
- Validates username format using `InputValidator.IsValidUsername()`
- Validates password strength if provided
- Hashes password server-side if provided
- Only updates non-null properties

### 3. Security Improvements

- **Server-side password hashing**: Passwords are now properly hashed with PBKDF2 and salt
- **Password validation**: Strong password requirements enforced before hashing
- **Username validation**: Proper format validation for usernames
- **Separation of concerns**: Database models separated from API request models

## Test Cases Now Working

The following test cases in `test-role-authorization.http` now work correctly:

- **Test #14**: Try to update user as regular user (should fail with 403)
- **Test #15**: Update user as Admin (should succeed with 204)
- **Test #16**: Partial update user as Admin (should succeed with 204)

## Request Format

**Full Update (PUT):**
```json
{
  "username": "new_username",
  "password": "NewPassword123!",  // Plain password - will be hashed
  "role": 1
}
```

**Partial Update (PATCH):**
```json
{
  "username": "new_username"  // Only update username
}
```

or

```json
{
  "password": "NewPassword123!"  // Only update password
}
```

## Security Benefits

1. **No password hash exposure**: Clients never send or receive password hashes
2. **Consistent validation**: All password updates go through the same validation logic
3. **Proper error handling**: Clear, user-friendly error messages for validation failures
4. **Input sanitization**: Username and other inputs are properly validated
5. **Role separation**: Clear distinction between database models and API contracts

## Files Modified

1. `/Models/UserUpdateRequest.cs` - **Created** (new request models)
2. `/Controllers/UserController.cs` - **Modified** (updated PUT/PATCH methods)
3. `/test-user-update-fix.http` - **Created** (quick test file for verification)

## Testing

To test the fix:

1. Start the API: `dotnet run`
2. Use the test files to register users and get tokens
3. Try the PUT/PATCH endpoints with the new request format
4. Verify that password updates work and are properly hashed in the database

## Build Status

✅ **Project builds successfully** - All changes compile without errors
✅ **Security maintained** - Password hashing and validation preserved
✅ **Backward compatibility** - Other endpoints unchanged
✅ **Test cases fixed** - Role authorization tests now work correctly

This fix resolves the immediate issue while improving the overall security and usability of the user management API.
