# Deployment Options for Your .NET Hotel Booking API

## Quick Overview of Options

| Platform | Difficulty | Cost | Setup Time | Best For |
|----------|------------|------|------------|----------|
| **Railway** | ⭐ Easy | Free tier | 5 minutes | Quick testing |
| **Render** | ⭐ Easy | Free tier | 10 minutes | Simple deployment |
| **Azure App Service** | ⭐⭐ Medium | Free tier | 15 minutes | Microsoft ecosystem |
| **DigitalOcean** | ⭐⭐ Medium | $5/month | 20 minutes | Best value |
| **AWS** | ⭐⭐⭐ Advanced | Free tier | 30 minutes | Enterprise scale |

## 🚀 **Option 1: Railway (Recommended for Quick Testing)**

**Why Railway is perfect for you:**
- ✅ Extremely easy deployment
- ✅ Free tier with good limits
- ✅ Automatic HTTPS
- ✅ GitHub integration
- ✅ Built-in database options

### Step 1: Prepare Your Project

First, let's prepare your API for deployment:

```bash
# Add a Dockerfile
touch Dockerfile
```

### Step 2: Create Dockerfile

```dockerfile
# Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["HotelBookingAPI.csproj", "."]
RUN dotnet restore "./HotelBookingAPI.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "HotelBookingAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "HotelBookingAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HotelBookingAPI.dll"]
```

### Step 3: Update appsettings.json for Production

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "MongoDbSettings": {
    "ConnectionString": "${MONGODB_CONNECTION_STRING}",
    "DatabaseName": "${DATABASE_NAME}"
  },
  "Jwt": {
    "Key": "${JWT_SECRET}",
    "Issuer": "${JWT_ISSUER}",
    "Audience": "${JWT_AUDIENCE}",
    "ExpiryMinutes": 60
  }
}
```

### Step 4: Deploy to Railway

1. **Sign up**: Go to [railway.app](https://railway.app) and sign up with GitHub
2. **Create project**: Click "New Project" → "Deploy from GitHub repo"
3. **Select your repo**: Choose your HotelBookingAPI repository
4. **Set environment variables**:
   ```
   MONGODB_CONNECTION_STRING=mongodb+srv://username:password@cluster.mongodb.net/
   DATABASE_NAME=HotelBooking
   JWT_SECRET=YourSuperSecretKeyHere123456789012345678901234567890
   JWT_ISSUER=HotelBookingAPI
   JWT_AUDIENCE=HotelBookingAPI
   ASPNETCORE_ENVIRONMENT=Production
   ```

**Total time: 5-10 minutes** ⚡

## 🌐 **Option 2: Render (Great Alternative)**

Similar to Railway but with different features:

### Deploy Steps:
1. Go to [render.com](https://render.com)
2. Connect GitHub account
3. Create "New Web Service"
4. Select your repository
5. Configure:
   ```
   Build Command: dotnet publish -c Release -o out
   Start Command: dotnet out/HotelBookingAPI.dll
   ```

## ☁️ **Option 3: Azure App Service (Microsoft's Platform)**

Perfect if you want to stay in the Microsoft ecosystem:

### Quick Deploy with Azure CLI:

```bash
# Install Azure CLI (if not installed)
brew install azure-cli  # macOS
# or download from: https://aka.ms/installazurecli

# Login to Azure
az login

# Create resource group
az group create --name HotelBookingAPI-rg --location "East US"

# Create App Service plan (Free tier)
az appservice plan create --name HotelBookingAPI-plan --resource-group HotelBookingAPI-rg --sku FREE --is-linux

# Create web app
az webapp create --resource-group HotelBookingAPI-rg --plan HotelBookingAPI-plan --name your-unique-app-name --runtime "DOTNETCORE:9.0"

# Deploy your code
dotnet publish -c Release
cd bin/Release/net9.0/publish
zip -r ../../../../../deploy.zip .
cd ../../../../../

az webapp deployment source config-zip --resource-group HotelBookingAPI-rg --name your-unique-app-name --src deploy.zip
```

## 🐳 **Option 4: DigitalOcean App Platform**

Great balance of simplicity and features:

1. Go to [cloud.digitalocean.com](https://cloud.digitalocean.com)
2. Create "App" → Connect GitHub
3. Select repository and branch
4. DigitalOcean auto-detects .NET and builds for you

## 📊 **Database Options**

You'll need a database for production. Here are your options:

### **Option A: MongoDB Atlas (Recommended)**
```bash
# Free tier: 512MB storage
# Sign up at: https://cloud.mongodb.com
# Get connection string like: mongodb+srv://username:password@cluster.mongodb.net/
```

### **Option B: Railway's Built-in MongoDB**
```bash
# In Railway dashboard:
# Add "MongoDB" service to your project
# Railway will provide connection string automatically
```

### **Option C: DigitalOcean Managed Database**
```bash
# $15/month for managed MongoDB
# Good for production workloads
```

## 🛠️ **Let's Deploy to Railway Right Now**

I'll walk you through the fastest deployment:

### Step 1: Prepare the Dockerfile

```dockerfile
# Save this as Dockerfile in your project root
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["HotelBookingAPI.csproj", "."]
RUN dotnet restore "./HotelBookingAPI.csproj"
COPY . .
RUN dotnet build "HotelBookingAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "HotelBookingAPI.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HotelBookingAPI.dll"]
```

### Step 2: Update Program.cs for Production

```csharp
// Add this to Program.cs for Railway deployment
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(int.Parse(port));
});
```

### Step 3: Create railway.json

```json
{
  "$schema": "https://railway.app/railway.schema.json",
  "build": {
    "builder": "dockerfile"
  },
  "deploy": {
    "startCommand": "dotnet HotelBookingAPI.dll",
    "healthcheckPath": "/health"
  }
}
```

### Step 4: Environment Variables for Railway

```bash
# These will be set in Railway dashboard
ASPNETCORE_ENVIRONMENT=Production
MONGODB_CONNECTION_STRING=mongodb+srv://...
DATABASE_NAME=HotelBooking
JWT_SECRET=YourSuperSecretKeyAtLeast32Characters
JWT_ISSUER=HotelBookingAPI
JWT_AUDIENCE=HotelBookingAPI
PORT=8080
```

## 🧪 **Testing Your Deployed API**

Once deployed, test your endpoints:

```bash
# Test health endpoint
curl https://your-app-name.railway.app/health

# Test rooms endpoint
curl https://your-app-name.railway.app/api/rooms

# Test authentication
curl -X POST https://your-app-name.railway.app/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"testuser","password":"TestPassword123!"}'

# Test rate limiting (run multiple times quickly)
for i in {1..20}; do
  curl -s -o /dev/null -w "Request $i: %{http_code}\n" \
    https://your-app-name.railway.app/api/rooms
done
```

## 💰 **Cost Breakdown**

### **Free Tiers:**
- **Railway**: 500 hours/month, $5 credit
- **Render**: 750 hours/month  
- **Azure**: 1 million requests/month
- **MongoDB Atlas**: 512MB storage

### **Paid Options:**
- **Railway Pro**: $5/month
- **DigitalOcean**: $5/month
- **Azure**: Pay-as-you-go
- **AWS**: Pay-as-you-go

## 🚀 **My Recommendation**

For testing your API right now:

1. **Start with Railway** (5 minutes to deploy)
2. **Use MongoDB Atlas free tier** for database
3. **Test all your endpoints** 
4. **Share the URL** with others for feedback

For production later:
1. **DigitalOcean** for best value ($5/month)
2. **Azure** if you want Microsoft ecosystem
3. **AWS** if you need enterprise scale

## 🎯 **Next Steps**

1. Choose Railway for quick testing
2. Set up MongoDB Atlas (free)
3. Deploy in 10 minutes
4. Test your secure API live
5. Get feedback and iterate

Want me to help you deploy to Railway right now? I can walk you through each step!
