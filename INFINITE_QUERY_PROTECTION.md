# 🛡️ Infinite Query Protection - Stop React from Crashing Your Server!

## 🚨 **The Problem: React Infinite Queries**

You know the pain! Sometimes React components can trigger infinite queries:

```javascript
// 😱 BAD: Can cause infinite requests
useEffect(() => {
  fetchData();
}, [data]); // data changes on every fetch!

// 😱 BAD: Infinite loop in React Query  
const { data } = useQuery('rooms', fetchRooms, {
  refetchOnMount: true,
  refetchOnWindowFocus: true,
  refetchInterval: 1000 // Every second!
});

// 😱 BAD: Recursive API calls
const fetchUser = async () => {
  const user = await api.getUser();
  if (!user.complete) {
    await fetchUser(); // Recursion without delay!
  }
};
```

**Result: Your server gets hammered and crashes!** 💥

## ✅ **The Solution: Multi-Layer Protection**

I've implemented comprehensive protection against infinite queries:

### **🎯 Smart Rate Limiting per Endpoint Type**

```csharp
// Different limits for different endpoints:
Authentication (/api/auth/*):
├── Burst: 5 requests per 10 seconds
├── Per minute: 20 requests  
└── Daily: 100 requests

Room browsing (/api/room/*):
├── Burst: 60 requests per 10 seconds  
├── Per minute: 200 requests
└── Daily: 1000 requests (heavy browsing expected)

User operations (/api/user/*):
├── Burst: 30 requests per 10 seconds
├── Per minute: 100 requests
└── Daily: 500 requests
```

### **🔍 Per-User AND Per-IP Protection**

```csharp
// Tracks limits separately:
Authenticated users: By user ID from JWT token
├── user:507f1f77bcf86cd799439011
├── user:507f1f77bcf86cd799439012
└── More precise tracking

Unauthenticated users: By IP address  
├── ip:192.168.1.100
├── ip:10.0.0.50
└── Prevents anonymous abuse
```

## 🚀 **How It Works**

### **1. Request Tracking**
```csharp
Every request is tracked:
├── Timestamp when request happened
├── Which endpoint was called
├── Who made the request (user/IP)
└── Rolling window analysis
```

### **2. Multi-Level Limits**
```csharp
// Checks happen in this order:
1. Burst check (10 seconds): Stops rapid-fire requests
2. Per-minute check: Prevents sustained flooding  
3. Daily check: Prevents all-day abuse
4. Allow request if all checks pass
```

### **3. Smart Response Headers**
```http
// Your React app gets helpful headers:
X-RateLimit-Remaining: 45          // Requests left this minute
X-RateLimit-Burst-Remaining: 8     // Burst requests left
X-RateLimit-Daily-Remaining: 850   // Daily requests left
X-RateLimit-Reset: 1726315200      // When limits reset
```

### **4. Graceful Error Responses**
```json
// When limits exceeded:
{
  "error": {
    "code": 429,
    "message": "Too many requests", 
    "details": "Burst limit exceeded",
    "retryAfter": 10
  }
}
// Plus: Retry-After: 10 header
```

## 🧪 **Testing the Protection**

### **Simulate React Infinite Loop**
```bash
# Test burst protection (60 requests in 10 seconds):
for i in {1..70}; do
  curl -X GET https://localhost:7070/api/room & 
done
wait

# Result: First 60 succeed, rest get 429 errors
```

### **Test Different Endpoints**
```bash
# Login protection (stricter):
for i in {1..10}; do
  curl -X POST https://localhost:7070/api/auth/login \
    -H "Content-Type: application/json" \
    -d '{"username":"test","password":"wrong"}' &
done

# Result: First 5 succeed, rest blocked
```

## 📊 **Rate Limit Configuration**

### **Current Limits (Production-Ready)**

| Endpoint | Burst (10s) | Per Minute | Daily | Use Case |
|----------|-------------|------------|-------|----------|
| **Auth** | 5 | 20 | 100 | Login attempts |
| **Rooms** | 60 | 200 | 1000 | Heavy browsing |
| **Users** | 30 | 100 | 500 | Profile management |
| **Default** | 20 | 80 | 400 | Other endpoints |

### **Why These Limits?**

```
Authentication (Strict):
├── Login brute force protection
├── Account enumeration prevention  
└── Credential stuffing protection

Room browsing (Generous):
├── Users browse lots of rooms
├── Search, filter, pagination
└── Image loading, details viewing

User operations (Moderate):
├── Profile updates less frequent
├── Settings changes
└── Account management
```

## 🔧 **Customization Options**

### **Modify Limits per Endpoint**
```csharp
// In QueryProtectionMiddleware.cs:
private static readonly Dictionary<string, RateLimitConfig> EndpointLimits = new()
{
    // Make auth even stricter:
    { "/api/auth/login", new RateLimitConfig(3, 10, 50) },
    
    // Make room browsing more generous:
    { "/api/room", new RateLimitConfig(100, 300, 2000) },
    
    // Add specific endpoint:
    { "/api/booking", new RateLimitConfig(10, 50, 200) }
};
```

### **Add New Protection Rules**
```csharp
// Add geographic restrictions:
if (clientId.StartsWith("ip:") && IsFromSuspiciousCountry(clientId))
{
    return new RateLimitResult { IsAllowed = false, Reason = "Geographic restriction" };
}

// Add time-based restrictions:
if (DateTime.UtcNow.Hour >= 2 && DateTime.UtcNow.Hour <= 6) // 2-6 AM
{
    config = config with { BurstLimit = config.BurstLimit / 2 }; // Reduce limits at night
}
```

## 🎯 **React Best Practices** 

### **How to Handle Rate Limits in React**

```javascript
// ✅ GOOD: Handle 429 responses gracefully
const fetchWithRetry = async (url, options = {}) => {
  try {
    const response = await fetch(url, options);
    
    if (response.status === 429) {
      const retryAfter = response.headers.get('Retry-After');
      console.warn(`Rate limited. Retry after ${retryAfter}s`);
      
      // Show user-friendly message
      toast.warning(`Too many requests. Please wait ${retryAfter} seconds.`);
      
      // Auto-retry after delay
      await new Promise(resolve => setTimeout(resolve, retryAfter * 1000));
      return fetchWithRetry(url, options);
    }
    
    return response;
  } catch (error) {
    console.error('Request failed:', error);
    throw error;
  }
};

// ✅ GOOD: Respect rate limit headers
const ApiClient = {
  async request(endpoint) {
    const response = await fetch(endpoint);
    
    // Check remaining requests
    const remaining = response.headers.get('X-RateLimit-Remaining');
    if (remaining && parseInt(remaining) < 10) {
      console.warn('Approaching rate limit!');
      // Maybe slow down requests or warn user
    }
    
    return response;
  }
};

// ✅ GOOD: Debounce search queries
const useRoomSearch = (query) => {
  const [debouncedQuery] = useDebounce(query, 500); // 500ms delay
  
  return useQuery(
    ['rooms', debouncedQuery],
    () => fetchRooms(debouncedQuery),
    {
      enabled: debouncedQuery.length > 2, // Only search with 3+ chars
      staleTime: 30000, // Cache for 30 seconds
      refetchOnWindowFocus: false // Don't refetch on focus
    }
  );
};
```

## 📈 **Monitoring & Alerts**

### **Log Analysis**
```bash
# Check for rate limit violations:
grep "Rate limit exceeded" logs/app.log | wc -l

# Check for burst violations:  
grep "Burst limit exceeded" logs/app.log

# Most blocked IPs:
grep "Rate limit exceeded" logs/app.log | cut -d' ' -f6 | sort | uniq -c | sort -nr
```

### **Health Check Endpoint**
```csharp
// Add to a controller:
[HttpGet("rate-limit-stats")]
public async Task<IActionResult> GetRateLimitStats()
{
    return Ok(new {
        ActiveUsers = _userRequests.Count,
        MostActiveEndpoint = GetMostActiveEndpoint(),
        RecentBlocks = GetRecentBlocks()
    });
}
```

## ✅ **Benefits of This Protection**

### **Server Stability**
- ✅ **Prevents server crashes** from infinite queries
- ✅ **Maintains performance** under heavy load  
- ✅ **Automatic cleanup** of old tracking data
- ✅ **Memory efficient** with concurrent dictionaries

### **Security Benefits**
- ✅ **Brute force protection** on auth endpoints
- ✅ **DoS attack prevention** 
- ✅ **Resource abuse prevention**
- ✅ **Fair usage enforcement**

### **Developer Experience**
- ✅ **Clear error messages** for debugging
- ✅ **Rate limit headers** for client optimization
- ✅ **Configurable limits** per endpoint type
- ✅ **No breaking changes** to existing API

## 🎉 **Summary**

Your Hotel Booking API now has **enterprise-grade protection** against infinite queries:

✅ **Multi-layer rate limiting** (burst, per-minute, daily)  
✅ **Per-endpoint configuration** (auth stricter, browsing generous)  
✅ **Smart user tracking** (JWT users + IP fallback)  
✅ **Graceful error handling** with retry instructions  
✅ **Automatic cleanup** and memory management  
✅ **Production-ready defaults** with easy customization  

**Your server is now bulletproof against React infinite query bugs!** 🚀

No more server crashes from accidental infinite loops - the protection kicks in automatically and guides users/developers on how to fix their code.

Test it with the provided test files and watch your server stay stable even under heavy request loads!
