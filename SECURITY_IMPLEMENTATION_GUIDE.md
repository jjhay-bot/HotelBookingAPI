# 🔒 Hotel Booking API - Security Implementation Guide

## 📋 Overview
This document outlines all security measures implemented in the Hotel Booking API, including explanations, configurations, and customization options.

---

## 🛡️ Security Middleware (`SecurityMiddleware.cs`)

### 🎯 Purpose
Custom middleware that provides protection against common web attacks and implements security best practices.

### 🚀 Features Implemented

#### 1. **HTTP Security Headers**
```csharp
// Location: SecurityMiddleware.cs -> AddSecurityHeaders()
```

| Header | Value | Purpose | Line |
|--------|-------|---------|------|
| `X-Frame-Options` | `DENY` | Prevents clickjacking attacks | 75 |
| `X-Content-Type-Options` | `nosniff` | Prevents MIME type sniffing | 78 |
| `X-XSS-Protection` | `1; mode=block` | Enables XSS protection | 81 |
| `Content-Security-Policy` | Restrictive policy | Controls resource loading | 94-95 |
| `Strict-Transport-Security` | `max-age=31536000; includeSubDomains` | Forces HTTPS (HTTPS only) | 99 |
| `Referrer-Policy` | `strict-origin-when-cross-origin` | Controls referrer information | 102 |
| `Permissions-Policy` | Disables geolocation, mic, camera | Restricts browser features | 105 |

#### 2. **Injection Attack Protection**
```csharp
// Location: SecurityMiddleware.cs -> CheckForSuspiciousActivity()
```

**SQL Injection Detection:**
- Patterns: `' OR`, `' AND`, `UNION SELECT`, `DROP TABLE`, `DELETE FROM`, `INSERT INTO`
- Action: Block request and log warning

**NoSQL Injection Detection:**
- Patterns: `$ne`, `$gt`, `$lt`, `$regex`, `$where`, `$eval`, `$or`, `$and`, `$not`, `$nor`, `$exists`
- Action: Block request and log warning

**Script Injection Detection:**
- Patterns: `<script`, `javascript:`, `onload=`, `onerror=`, `eval(`, `setTimeout(`
- Action: Block request and log warning

#### 3. **Rate Limiting**
```csharp
// Location: SecurityMiddleware.cs -> IsRateLimited()
```
- **Limit:** 100 requests per minute per IP address
- **Cleanup:** Automatic removal of old entries (2+ minutes)
- **Storage:** In-memory dictionary with thread-safe locking
- **Action:** Block request when limit exceeded

#### 4. **Request Size Validation**
```csharp
// Location: SecurityMiddleware.cs -> InvokeAsync()
```
- **Limit:** 1MB maximum request size
- **Purpose:** Prevent DoS attacks via large payloads

---

## 🔐 Password Security (`PasswordHasher.cs`)

### 🎯 Implementation
- **Algorithm:** PBKDF2 with SHA-256
- **Iterations:** 10,000 (computational cost)
- **Salt Size:** 128-bit (16 bytes) random salt
- **Hash Size:** 256-bit (32 bytes)
- **Timing Attack Protection:** Constant-time comparison using `CryptographicOperations.FixedTimeEquals`

### 🔧 Usage
```csharp
// Hash password
string hashedPassword = PasswordHasher.HashPassword("userPassword123");

// Verify password
bool isValid = PasswordHasher.VerifyPassword("userPassword123", hashedPassword);
```

---

## ✅ Input Validation (`InputValidator.cs`)

### 🎯 Validation Rules

#### **Username Validation**
- **Pattern:** `^[a-zA-Z0-9_-]{3,30}$`
- **Allowed:** Alphanumeric, underscore, hyphen
- **Length:** 3-30 characters

#### **MongoDB ObjectId Validation**
- **Pattern:** `^[0-9a-fA-F]{24}$`
- **Format:** 24 hexadecimal characters

#### **Password Strength Requirements**
- Minimum 8 characters, maximum 128
- Must contain uppercase letter
- Must contain lowercase letter
- Must contain number
- Must contain special character

#### **String Sanitization**
- Length limits to prevent buffer overflow
- HTML encoding to prevent XSS
- Whitespace trimming

---

## 🌐 CORS & Network Security (`SecurityConfiguration.cs`)

### 🎯 CORS Policy
```csharp
// Location: SecurityConfiguration.cs -> AddSecurityServices()
```
- **Default Origins:** `https://localhost:3000`
- **Configurable:** Via `appsettings.json` → `AllowedOrigins` array
- **Methods:** All methods allowed
- **Headers:** All headers allowed
- **Credentials:** Supported

### 🔧 Customization
```json
// appsettings.json
{
  "AllowedOrigins": [
    "https://localhost:3000",
    "https://myapp.com",
    "https://app.mydomain.com"
  ]
}
```

---

## 🔑 JWT Authentication

### 🎯 Configuration
```json
// appsettings.Development.json
{
  "Jwt": {
    "Key": "ThisIsASecretKeyForDevelopmentOnlyAndShouldBeAtLeast32CharactersLong123456",
    "Issuer": "HotelBookingAPI",
    "Audience": "HotelBookingAPIUsers"
  }
}
```

### 🔧 Security Features
- **Symmetric key encryption** with configurable key
- **Issuer and audience validation**
- **Lifetime validation** with zero clock skew
- **Signing key validation**

---

## 🎓 Educational Security Demo (`SecurityDemoController.cs`)

### 🎯 Purpose
Demonstrates vulnerable vs secure coding practices for educational purposes.

### 🚨 Endpoints

#### **Vulnerable Endpoints** (For Learning Only)
- `POST /api/security-demo/vulnerable/login` - NoSQL injection vulnerable
- `GET /api/security-demo/vulnerable/user` - String concatenation example

#### **Secure Endpoints** (Best Practices)
- `POST /api/security-demo/secure/login` - Proper validation & typed queries
- `GET /api/security-demo/secure/user` - Input sanitization & validation

#### **Educational Resources**
- `POST /api/security-demo/attack-examples` - Common attack patterns
- `GET /api/security-demo/secure-patterns` - Secure coding examples

---

## 🔧 Customization Guide

### 🎯 Allowing Specific Websites to Frame Your API

#### **Understanding "Same Origin"**
Same Origin = Same Protocol + Same Domain + Same Port

**✅ Same Origin Examples:**
- `https://myapp.com/frontend` ↔ `https://myapp.com/api`
- `https://localhost:3000/page` ↔ `https://localhost:3000/api`

**❌ Different Origin Examples:**
- `https://localhost:3000` ↔ `https://localhost:5268` (different ports)
- `https://app.mycompany.com` ↔ `https://api.mycompany.com` (different subdomains)
- `https://frontend.com` ↔ `https://backend.com` (different domains)

#### **Option 1: X-Frame-Options**
```csharp
// Location: SecurityMiddleware.cs line 75
// Change from:
response.Headers.Append("X-Frame-Options", "DENY");

// To allow same origin:
response.Headers.Append("X-Frame-Options", "SAMEORIGIN");
```

#### **Option 2: Content-Security-Policy (Recommended)**
```csharp
// Location: SecurityMiddleware.cs lines 94-95
// Add frame-ancestors directive:
response.Headers.Append("Content-Security-Policy", 
    "default-src 'self'; script-src 'self'; style-src 'self' 'unsafe-inline'; img-src 'self' data:; font-src 'self'; frame-ancestors 'self' https://myapp.com https://localhost:3000;");
```

### 🎯 Development Setup Examples

#### **Frontend on :3000, Backend on :5268**
```csharp
// Allow localhost:3000 to frame the API
response.Headers.Append("Content-Security-Policy", 
    "default-src 'self'; script-src 'self'; style-src 'self' 'unsafe-inline'; img-src 'self' data:; font-src 'self'; frame-ancestors 'self' http://localhost:3000;");
```

#### **Multiple Trusted Domains**
```csharp
response.Headers.Append("Content-Security-Policy", 
    "default-src 'self'; script-src 'self'; style-src 'self' 'unsafe-inline'; img-src 'self' data:; font-src 'self'; frame-ancestors 'self' https://app.mycompany.com https://dashboard.mycompany.com https://partner-site.com;");
```

### 🎯 Allowing External Resources

#### **CDN Scripts**
```csharp
// script-src 'self' https://cdn.trusted.com
```

#### **External Images**
```csharp
// img-src 'self' data: https://images.trusted.com
```

---

## 📊 Security Impact

### ✅ **Protections Against:**
- SQL/NoSQL Injection
- Cross-Site Scripting (XSS)
- Clickjacking
- MIME Sniffing
- Brute Force Attacks
- DoS/DDoS Attacks
- Password Cracking
- Timing Attacks
- MITM Attacks
- Data Exfiltration

### 🚨 **Security Events Logged:**
- Injection attempt detection with IP logging
- Rate limit violations tracking
- Failed authentication attempts monitoring
- Suspicious activity patterns alerting

---

## 🔍 Testing Security

### 🎯 Security Demo Tests
Use the provided `test-security-demo.http` file to test:
- NoSQL injection attempts
- Authentication bypass attempts
- Rate limiting
- Input validation
- Secure vs vulnerable endpoints

### 🎯 Security Headers Verification
Use browser developer tools or online tools to verify security headers:
- [Security Headers Checker](https://securityheaders.com/)
- Browser DevTools → Network → Response Headers

---

## 🚀 Production Recommendations

### 🎯 Environment-Specific Settings
- **Development:** More permissive CORS, detailed error messages
- **Production:** Strict CORS, generic error messages, HTTPS enforcement

### 🎯 Additional Security Measures
1. **Use HTTPS certificates** in production
2. **Implement proper logging and monitoring**
3. **Regular security audits and penetration testing**
4. **Keep dependencies updated**
5. **Use secrets management** for JWT keys
6. **Implement proper backup and disaster recovery**

---

## 📚 References
- [OWASP Top 10](https://owasp.org/www-project-top-ten/)
- [Content Security Policy Guide](https://developer.mozilla.org/en-US/docs/Web/HTTP/CSP)
- [ASP.NET Core Security](https://docs.microsoft.com/en-us/aspnet/core/security/)
- [MongoDB Security Guide](https://docs.mongodb.com/manual/security/)

---

**Last Updated:** September 13, 2025  
**Version:** 1.0  
**Status:** Production Ready 🔒
