# Testing Guide for Hotel Booking API

This guide explains how to test the role-based authorization system implemented in the Hotel Booking API.

## Prerequisites

1. **Start the API Server**
   ```bash
   cd /Users/jhayjhayalcorcon/-jjhay.nosync/personal/CODESMITH/HotelBookingAPI
   dotnet run
   ```
   The API will be available at `http://localhost:5268`

2. **Install REST Client Extension**
   - Install the "REST Client" extension in VS Code to run `.http` files
   - Or use tools like Postman, Insomnia, or curl

## Test Files Overview

### 1. `test-security-demo.http`
Tests basic security features:
- Security headers validation
- Input validation and injection protection
- Rate limiting
- Request size validation
- HTTPS enforcement

### 2. `test-role-authorization.http`
Comprehensive tests for role-based authorization:
- User registration and authentication
- Role-specific endpoint access
- Permission enforcement
- CRUD operations with different roles

## How to Use the Role Authorization Tests

### Step 1: Initialize Admin User

Before running the tests, you need at least one admin user in the database. You can either:

**Option A: Use MongoDB Compass or Shell to insert directly:**
```javascript
db.users.insertOne({
  "username": "admin",
  "passwordHash": "PBKDF2:100000:base64EncodedSalt:base64EncodedHash",
  "role": 2,
  "isActive": true,
  "createdAt": new Date()
})
```

**Option B: Create a temporary endpoint to register the first admin (not recommended for production):**

### Step 2: Run Tests Sequentially

The tests in `test-role-authorization.http` are designed to be run sequentially. Follow these steps:

1. **Run tests 1-5** to register regular users and log them in
2. **Run test 6** to log in as the admin user
3. **Copy the tokens** from the login responses and replace the variables:
   - `{{userToken}}` - token from regular user login
   - `{{managerToken}}` - token from manager login (after creating manager)
   - `{{adminToken}}` - token from admin login

4. **Copy IDs** from responses and replace:
   - `{{userId}}` - ID of a regular user
   - `{{managerId}}` - ID of a manager user
   - `{{roomId}}` - ID of a created room

5. **Continue with remaining tests** using the actual tokens and IDs

### Step 3: Expected Results

Each test includes expected HTTP status codes:

- **200** - Success
- **201** - Created
- **204** - No Content (successful update/delete)
- **401** - Unauthorized (no token or invalid token)
- **403** - Forbidden (insufficient role permissions)
- **404** - Not Found
- **409** - Conflict (username already exists)

## Role Permission Matrix

| Action | User (0) | Manager (1) | Admin (2) |
|--------|----------|-------------|-----------|
| Register Account | ✅ | ✅ | ✅ |
| Login | ✅ | ✅ | ✅ |
| View Rooms | ✅ | ✅ | ✅ |
| View Own Profile | ✅ | ✅ | ✅ |
| View All Users | ❌ | ✅ | ✅ |
| Create Rooms | ❌ | ✅ | ✅ |
| Update Rooms | ❌ | ✅ | ✅ |
| Delete Rooms | ❌ | ❌ | ✅ |
| Update Users | ❌ | ❌ | ✅ |
| Delete Users | ❌ | ❌ | ✅ |
| Manage Roles | ❌ | ❌ | ✅ |
| Create Admin/Manager | ❌ | ❌ | ✅ |
| Deactivate Users | ❌ | ❌ | ✅ |

## Security Features Tested

### 1. Authentication
- JWT token-based authentication
- Token validation on protected endpoints
- Token expiration handling

### 2. Authorization
- Role-based access control
- Endpoint-specific permissions
- Hierarchical role system

### 3. Input Security
- SQL injection protection
- NoSQL injection protection
- XSS prevention
- Script injection blocking

### 4. Rate Limiting
- 100 requests per minute per IP
- Automatic blocking of excessive requests

### 5. Security Headers
- X-Frame-Options: DENY
- X-Content-Type-Options: nosniff
- X-XSS-Protection: 1; mode=block
- Content Security Policy
- Referrer Policy

### 6. Password Security
- PBKDF2 hashing with SHA-256
- Random salt generation
- Strong password requirements
- Constant-time comparison

## Troubleshooting

### Common Issues

1. **401 Unauthorized**
   - Check if token is valid and not expired
   - Ensure Bearer prefix is included
   - Verify user account is active

2. **403 Forbidden**
   - Check if user has required role for the endpoint
   - Verify role hierarchy (Admin > Manager > User)

3. **Rate Limiting**
   - Wait for rate limit window to reset (1 minute)
   - Use different IP if testing extensively

4. **MongoDB Connection**
   - Ensure MongoDB is running
   - Check connection string in appsettings.json
   - Verify database permissions

### Test Debugging

1. **Enable Detailed Logging**
   ```json
   // In appsettings.Development.json
   {
     "Logging": {
       "LogLevel": {
         "Default": "Debug",
         "Microsoft.AspNetCore": "Information"
       }
     }
   }
   ```

2. **Check HTTP Response Headers**
   - Look for security headers in responses
   - Verify CORS headers if testing from browser

3. **Monitor MongoDB**
   - Use MongoDB Compass to view database changes
   - Check user collection for role updates

## Production Considerations

When deploying to production:

1. **Change Default Passwords** - Update all test passwords
2. **Enable HTTPS** - Force HTTPS for all endpoints
3. **Configure CORS** - Restrict to known origins
4. **Set Rate Limits** - Adjust based on expected traffic
5. **Monitor Logs** - Set up proper logging and monitoring
6. **Environment Variables** - Use secure configuration management

## Sample Workflow

Here's a typical testing workflow:

```
1. Start API server
2. Run security demo tests to verify basic protection
3. Create/login admin user
4. Register manager and regular users via admin
5. Test each role's permissions systematically
6. Verify forbidden actions return 403
7. Test edge cases (invalid tokens, deactivated users)
8. Verify security headers and rate limiting
```

This comprehensive testing approach ensures that all security and authorization features are working correctly before deployment.
