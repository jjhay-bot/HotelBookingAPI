# 📱 Render Dashboard: Where to Enter Build & Start Commands

## Visual Step-by-Step Guide

When you're on the Render dashboard creating a new web service, here's exactly where to find and enter the build and start commands:

## Step 1: After Connecting Your Repository

Once you've connected your GitHub repository, you'll see a form with several fields:

```
┌─────────────────────────────────────────────────────────┐
│ Create a New Web Service                                │
├─────────────────────────────────────────────────────────┤
│                                                         │
│ Name: [hotel-booking-api                          ] │
│                                                         │
│ Region: [Oregon (US West)                        ▼] │
│                                                         │
│ Branch: [main                                    ▼] │
│                                                         │
│ Runtime: [Docker                                 ▼] │
│                                                         │
│ Build Command: [                                      ] │ ← ENTER HERE
│                                                         │
│ Start Command: [                                      ] │ ← ENTER HERE
│                                                         │
│ Instance Type: [Free                             ▼] │
│                                                         │
└─────────────────────────────────────────────────────────┘
```

## Step 2: Fill in the Exact Commands

### In the "Build Command" field, type:
```
dotnet publish -c Release -o out
```

### In the "Start Command" field, type:
```
dotnet out/HotelBookingAPI.dll
```

## Step 3: Complete Form Should Look Like This

```
┌─────────────────────────────────────────────────────────┐
│ Create a New Web Service                                │
├─────────────────────────────────────────────────────────┤
│                                                         │
│ Name: [hotel-booking-api                          ] │
│                                                         │
│ Region: [Oregon (US West)                        ▼] │
│                                                         │
│ Branch: [main                                    ▼] │
│                                                         │
│ Runtime: [Docker                                 ▼] │
│                                                         │
│ Build Command: [dotnet publish -c Release -o out      ] │ ✅
│                                                         │
│ Start Command: [dotnet out/HotelBookingAPI.dll        ] │ ✅
│                                                         │
│ Instance Type: [Free                             ▼] │ ✅
│                                                         │
└─────────────────────────────────────────────────────────┘
```

## Step 4: Add Environment Variables

After the main form, you'll see a section for Environment Variables:

```
┌─────────────────────────────────────────────────────────┐
│ Environment Variables                                   │
├─────────────────────────────────────────────────────────┤
│                                                         │
│ [+ Add Environment Variable]                           │
│                                                         │
│ Key: [MONGODB_CONNECTION_STRING]  Value: [mongodb+srv://...] │
│ Key: [DATABASE_NAME           ]  Value: [HotelBooking    ] │
│ Key: [JWT_SECRET             ]  Value: [YourSecretKey...] │
│ Key: [JWT_ISSUER             ]  Value: [HotelBookingAPI ] │
│ Key: [JWT_AUDIENCE           ]  Value: [HotelBookingAPI ] │
│ Key: [ASPNETCORE_ENVIRONMENT ]  Value: [Production      ] │
│ Key: [PORT                   ]  Value: [10000           ] │
│                                                         │
└─────────────────────────────────────────────────────────┘
```

## Alternative: If You Don't See Build/Start Command Fields

Sometimes Render auto-detects the runtime. If you don't see the Build Command and Start Command fields:

### Option 1: Change Runtime
1. In the "Runtime" dropdown, select "Docker" instead of auto-detected
2. This should show the Build Command and Start Command fields

### Option 2: Use Advanced Settings
1. Look for an "Advanced" button or link
2. Click it to expand more configuration options
3. You should see Build Command and Start Command fields there

### Option 3: After Initial Creation
If you miss these during creation, you can set them later:
1. Go to your service dashboard
2. Click "Settings" tab
3. Look for "Build & Deploy" section
4. Update the commands there

## What Each Command Does

### Build Command: `dotnet publish -c Release -o out`
- `dotnet publish` - Compiles your .NET app for production
- `-c Release` - Uses optimized release configuration
- `-o out` - Outputs compiled files to "out" directory

### Start Command: `dotnet out/HotelBookingAPI.dll`
- `dotnet` - .NET runtime
- `out/HotelBookingAPI.dll` - Your compiled application
- This starts your API server

## Troubleshooting

### If Build Command field is missing:
1. Make sure you selected "Docker" as runtime
2. Or look for "Advanced Settings" link
3. Or set it after creating the service in Settings

### If Start Command field is missing:
1. Same as above - check runtime setting
2. The field should appear right below Build Command

### If the form looks different:
Render sometimes updates their UI, but the concepts remain the same:
- Look for "Build Command" or "Build Script"
- Look for "Start Command" or "Start Script"
- The commands themselves remain exactly the same

## Quick Copy-Paste Values

**Build Command:**
```
dotnet publish -c Release -o out
```

**Start Command:**
```
dotnet out/HotelBookingAPI.dll
```

**Environment Variables** (add these one by one):
```
MONGODB_CONNECTION_STRING = your-mongodb-connection-string-here
DATABASE_NAME = HotelBooking
JWT_SECRET = YourSuperSecretKeyMustBeAtLeast32Characters123
JWT_ISSUER = HotelBookingAPI
JWT_AUDIENCE = HotelBookingAPI
ASPNETCORE_ENVIRONMENT = Production
PORT = 10000
```

## After Clicking "Create Web Service"

Render will:
1. ✅ Clone your repository
2. ✅ Run the build command (compile your .NET app)
3. ✅ Run the start command (start your API)
4. ✅ Give you a live URL

The whole process takes about 3-5 minutes!

---

**Need help finding these fields? The key is looking for "Build Command" and "Start Command" - they might be in the main form or under "Advanced Settings"!**
