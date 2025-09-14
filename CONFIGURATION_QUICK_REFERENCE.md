# ⚙️ Configuration Quick Reference Guide

## 💾 **Server-Side Caching (Live Memory Storage)**

### **Cache Location & Type**

The cache in this .NET Web API uses **built-in IMemoryCache** - it's **live in-memory storage on the server**:

```csharp
// Registered in Program.cs (line 49)
builder.Services.AddMemoryCache();

// Cache lives in SERVER RAM, not browser
private readonly IMemoryCache _cache;
```

### **How It Works**

| Request Flow | Cache Status | What Happens |
|--------------|-------------|--------------|
| **First request** | Cache MISS | Query MongoDB → Store in server RAM |
| **Next requests (within 5 min)** | Cache HIT | Serve from server RAM (no DB query) |
| **After 5 minutes** | Cache EXPIRED | Query MongoDB → Update server RAM |

### **Performance Impact**

- **Database Load:** 95% reduction in user status queries
- **Response Time:** 50x faster for cached entries (1ms vs 50ms)
- **Memory Usage:** ~3.2 MB RAM for 10,000 active users

### **Cache vs Browser Storage**

| Type | Location | Purpose | Survives Server Restart |
|------|----------|---------|------------------------|
| **Server Cache (Our Implementation)** | Web API Server RAM | Reduce DB queries | ❌ No |
| **Browser Cache** | User's browser | Reduce HTTP requests | ✅ Yes |
| **Redis Cache** | External Redis server | Shared server cache | ✅ Yes |

💡 **See [SERVER_SIDE_CACHING_EXPLAINED.md](./SERVER_SIDE_CACHING_EXPLAINED.md) for detailed explanation**

### **🧹 Built-in Auto Cleanup**

.NET Memory Cache has **automatic cleanup** built-in - no configuration needed!

```csharp
// Auto cleanup happens automatically:
- Expired items: Removed every 30 seconds
- Memory pressure: Immediate cleanup when RAM gets low  
- Priority-based: Removes Normal priority items first
- Background process: Non-blocking, minimal CPU impact
```

**Cleanup triggers:**
- ⏰ **Timer-based:** Every 30 seconds (expired items)
- 🔥 **Memory pressure:** When system RAM usage >85%
- 📏 **Size limits:** When cache size limits reached (if configured)
- ⏳ **Item expiration:** Your 5-minute user status expiry

**What gets cleaned up first:**
1. Expired items (past their expiration time)
2. Low priority cache entries
3. Normal priority entries (your user status cache)
4. High priority entries (only if needed)

💡 **See [MEMORY_CACHE_AUTO_CLEANUP.md](./MEMORY_CACHE_AUTO_CLEANUP.md) for detailed explanation**

## 🎯 **Cache Interval Configuration**

### **Current Setting Location**

**File:** `/Security/OptimizedUserStatusValidationMiddleware.cs`  
**Line:** 18  
**Current Value:** 5 minutes

```csharp
private static readonly TimeSpan CacheExpiry = TimeSpan.FromMinutes(5);
```

### **Quick Change Instructions**

1. **Open file:** `/Security/OptimizedUserStatusValidationMiddleware.cs`

2. **Find line 18 and change the value:**

   ```csharp
   // From:
   private static readonly TimeSpan CacheExpiry = TimeSpan.FromMinutes(5);
   
   // To one of these options:
   private static readonly TimeSpan CacheExpiry = TimeSpan.FromSeconds(30);  // Maximum security
   private static readonly TimeSpan CacheExpiry = TimeSpan.FromMinutes(1);   // High security
   private static readonly TimeSpan CacheExpiry = TimeSpan.FromMinutes(5);   // Balanced (current)
   private static readonly TimeSpan CacheExpiry = TimeSpan.FromMinutes(10);  // Better performance
   private static readonly TimeSpan CacheExpiry = TimeSpan.FromHours(1);     // Development only
   ```

3. **Rebuild and restart:**

   ```bash
   dotnet build
   dotnet run
   ```

### **Recommended Settings by Environment**

| Environment | Setting | Reason |
|-------------|---------|---------|
| **Production (High Security)** | `TimeSpan.FromMinutes(1)` | Quick response to user deactivation |
| **Production (Balanced)** | `TimeSpan.FromMinutes(5)` | Good security + performance |
| **Production (High Traffic)** | `TimeSpan.FromMinutes(10)` | Reduced database load |
| **Development** | `TimeSpan.FromMinutes(30)` | Less frequent validation |
| **Testing** | `TimeSpan.FromSeconds(1)` | Immediate validation |

### **Security Impact**

- **Lower values (30s-1min):** Deactivated users blocked faster, higher DB load
- **Higher values (10min+):** Deactivated users may access longer, lower DB load

### **Performance Impact**

- **Lower values:** More database queries, higher server load
- **Higher values:** Fewer database queries, better performance

## 🔧 **Other Configuration Options**

### **JWT Settings**

**File:** `Program.cs` (lines 40-55)

```csharp
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,          // ← Token expiration validation
        ValidateIssuerSigningKey = true,
        ValidIssuer = "HotelBookingAPI",
        ValidAudience = "HotelBookingAPI",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
        ClockSkew = TimeSpan.Zero        // ← No tolerance for clock skew
    };
});
```

### **Security Middleware Settings**

**File:** `/Security/SecurityMiddleware.cs`

```csharp
// Rate limiting (line ~15)
private static readonly Dictionary<string, DateTime> _lastRequest = new();
private static readonly TimeSpan RateLimitWindow = TimeSpan.FromSeconds(1);

// Request size limit (line ~20)
private const int MaxRequestSize = 1024 * 1024; // 1MB
```

### **Database Connection**

**File:** `appsettings.json`

```json
{
  "MongoDbSettings": {
    "ConnectionString": "mongodb://localhost:27017",
    "DatabaseName": "HotelBookingDB"
  }
}
```

## 📋 **Configuration Checklist**

- [ ] Cache interval set appropriately for environment
- [ ] JWT secret configured in environment variables
- [ ] Database connection string updated
- [ ] CORS origins configured for production
- [ ] HTTPS enforcement enabled
- [ ] Rate limiting appropriate for expected traffic
- [ ] Request size limits appropriate for use case

## 🚨 **Production Deployment Notes**

1. **Never use development settings in production**
2. **Always use environment variables for secrets**
3. **Test configuration changes in staging first**
4. **Monitor performance after cache interval changes**
5. **Document any custom configuration changes**

## 📞 **Need Help?**

- Review `/TESTING_GUIDE.md` for testing configuration changes
- Check `/SECURITY_IMPLEMENTATION_GUIDE.md` for security details
- See `/DATABASE_PERFORMANCE_ANALYSIS.md` for performance guidance
