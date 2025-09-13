# Web API Security Guide: SQL Injection and Prevention

## What is SQL Injection?

SQL Injection is a code injection technique where malicious SQL statements are inserted into application entry points to manipulate database queries. Although your API uses MongoDB (NoSQL), similar injection attacks can occur with NoSQL databases (called NoSQL Injection).

## How SQL Injection Works

### Traditional SQL Injection Example

Consider this vulnerable SQL query:
```sql
SELECT * FROM users WHERE username = '" + userInput + "' AND password = '" + passwordInput + "'"
```

If an attacker enters: `admin'; DROP TABLE users; --`

The resulting query becomes:
```sql
SELECT * FROM users WHERE username = 'admin'; DROP TABLE users; --' AND password = ''
```

This would:
1. Query for admin user
2. Delete the entire users table
3. Comment out the rest of the query

### MongoDB Injection Example

MongoDB can also be vulnerable to injection attacks:

**Vulnerable Code:**
```javascript
// Dangerous - direct string concatenation
db.users.find({username: userInput, password: passwordInput})
```

**Attack:**
```json
{
  "username": {"$ne": null},
  "password": {"$ne": null}
}
```

This bypasses authentication by using MongoDB operators.

## Prevention Strategies

### 1. Parameterized Queries (For SQL)
```csharp
// Safe SQL approach
var query = "SELECT * FROM users WHERE username = @username AND password = @password";
var command = new SqlCommand(query, connection);
command.Parameters.AddWithValue("@username", username);
command.Parameters.AddWithValue("@password", password);
```

### 2. MongoDB Driver Protection (Your Current Setup)

Your MongoDB C# driver already provides protection through:
- Strongly typed queries
- BSON serialization
- Expression trees

**Current Safe Implementation:**
```csharp
await _usersCollection.Find(x => x.Username == username).FirstOrDefaultAsync();
```

### 3. Input Validation
- Validate all user inputs
- Use allowlists for expected values
- Sanitize inputs before processing

### 4. Least Privilege Principle
- Database users should have minimal required permissions
- Separate read/write access
- Never use admin accounts for application connections

## Security Improvements for Your API

### Current Security Status

✅ **Good Practices Already Implemented:**
- Using MongoDB C# Driver (prevents injection)
- JWT Authentication
- Route constraints (`{id:length(24)}`)
- Authorization attributes

⚠️ **Areas for Improvement:**
- Weak password hashing
- Missing input validation
- No rate limiting
- Insufficient error handling

## Recommended Security Enhancements

### 1. Strong Password Hashing
Replace the current weak hashing with BCrypt.

### 2. Input Validation
Add comprehensive input validation and sanitization.

### 3. Rate Limiting
Implement rate limiting to prevent brute force attacks.

### 4. Secure Headers
Add security headers to responses.

### 5. Request Validation
Validate and sanitize all incoming requests.

## Example Attack Scenarios

### Scenario 1: Authentication Bypass
**Attack:** Malicious JSON payload
```json
{
  "username": {"$ne": ""},
  "password": {"$ne": ""}
}
```

**Protection:** Strong typing and validation prevent this in your current setup.

### Scenario 2: Data Extraction
**Attack:** Manipulated query parameters
```
GET /api/users?filter={"$where": "function() { return true; }"}
```

**Protection:** Avoid dynamic query building, use predefined filters.

## Security Testing

### Tools for Security Testing:
1. **OWASP ZAP** - Web application security scanner
2. **Burp Suite** - Web vulnerability scanner
3. **SQLMap** - SQL injection testing tool
4. **NoSQLMap** - NoSQL injection testing tool

### Manual Testing:
1. Test with malicious payloads
2. Attempt authentication bypass
3. Test input validation boundaries
4. Verify error message information disclosure

## Monitoring and Logging

### Security Events to Log:
- Failed authentication attempts
- Unusual query patterns
- Rate limit violations
- Input validation failures
- Administrative actions

### Example Logging Implementation:
```csharp
_logger.LogWarning("Failed authentication attempt for user: {Username} from IP: {IpAddress}", 
    username, httpContext.Connection.RemoteIpAddress);
```

## Security Checklist

- [ ] Use parameterized queries/safe drivers
- [ ] Implement strong password hashing
- [ ] Add comprehensive input validation
- [ ] Use HTTPS everywhere
- [ ] Implement rate limiting
- [ ] Add security headers
- [ ] Log security events
- [ ] Regular security testing
- [ ] Keep dependencies updated
- [ ] Use principle of least privilege

## Next Steps

1. Implement BCrypt password hashing
2. Add input validation middleware
3. Implement rate limiting
4. Add security headers
5. Create comprehensive logging
6. Set up security monitoring
