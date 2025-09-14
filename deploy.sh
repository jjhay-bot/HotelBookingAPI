#!/bin/bash

# 🚀 Hotel Booking API Deployment Helper
# This script helps you deploy your API to various platforms

echo "🏨 Hotel Booking API Deployment Helper"
echo "======================================"
echo ""

# Check if we're in the right directory
if [ ! -f "HotelBookingAPI.csproj" ]; then
    echo "❌ Error: HotelBookingAPI.csproj not found!"
    echo "Please run this script from the project root directory."
    exit 1
fi

echo "📋 Available deployment platforms:"
echo "1. Render (Recommended - Always Free)"
echo "2. Azure App Service (Microsoft Free Tier)"
echo "3. Fly.io (Generous Free Tier)"
echo "4. GitHub Codespaces (Quick Testing)"
echo "5. Local Docker test"
echo ""

read -p "Choose a platform (1-5): " choice

case $choice in
    1)
        echo ""
        echo "🎨 Render Deployment (Always Free)"
        echo "================================="
        echo ""
        echo "📝 Steps to deploy to Render:"
        echo "1. Go to https://render.com and sign up with GitHub"
        echo "2. Click 'New' → 'Web Service'"
        echo "3. Connect your GitHub repository"
        echo "4. Use these exact settings:"
        echo "   Build Command: dotnet publish -c Release -o out"
        echo "   Start Command: dotnet out/HotelBookingAPI.dll"
        echo ""
        echo "🔧 Required Environment Variables (set in Render dashboard):"
        echo "MONGODB_CONNECTION_STRING=mongodb+srv://username:password@cluster.mongodb.net/"
        echo "DATABASE_NAME=HotelBooking"
        echo "JWT_SECRET=$(openssl rand -base64 32)"
        echo "JWT_ISSUER=HotelBookingAPI"
        echo "JWT_AUDIENCE=HotelBookingAPI"
        echo "ASPNETCORE_ENVIRONMENT=Production"
        echo "PORT=10000"
        echo ""
        echo "💡 Advantages:"
        echo "✅ Always free (no trial period)"
        echo "✅ 750 hours/month free"
        echo "✅ Automatic HTTPS and SSL"
        echo "✅ Easy GitHub integration"
        echo ""
        echo "� Your API will be live at: https://your-app-name.onrender.com"
        ;;
    
    2)
        echo ""
        echo "☁️ Azure App Service (Microsoft Free Tier)"
        echo "=========================================="
        echo ""
        echo "🔧 Prerequisites:"
        echo "1. Install Azure CLI: brew install azure-cli"
        echo "2. Login: az login"
        echo ""
        echo "📝 Deployment commands:"
        echo "az group create --name HotelBookingAPI-rg --location 'East US'"
        echo "az appservice plan create --name HotelBookingAPI-plan --resource-group HotelBookingAPI-rg --sku F1 --is-linux"
        echo "az webapp create --resource-group HotelBookingAPI-rg --plan HotelBookingAPI-plan --name your-unique-app-name --runtime 'DOTNETCORE:9.0'"
        echo ""
        echo "💡 Advantages:"
        echo "✅ F1 Free tier (permanent)"
        echo "✅ Microsoft ecosystem"
        echo "✅ Professional platform"
        echo "✅ No credit card required"
        ;;
    
    3)
        echo ""
        echo "🚀 Fly.io Deployment"
        echo "==================="
        echo ""
        echo "📝 Steps to deploy to Fly.io:"
        echo "1. Install flyctl: curl -L https://fly.io/install.sh | sh"
        echo "2. Login: flyctl auth login"
        echo "3. Initialize: flyctl launch"
        echo "4. Deploy: flyctl deploy"
        echo ""
        echo "💡 Advantages:"
        echo "✅ 3 shared VMs free"
        echo "✅ Excellent performance"
        echo "✅ Global deployment"
        echo "✅ Production-grade features"
        ;;
    
    4)
        echo ""
        echo "🐙 GitHub Codespaces (Quick Testing)"
        echo "==================================="
        echo ""
        echo "📝 Steps for immediate testing:"
        echo "1. Push your code to GitHub"
        echo "2. Open repository on github.com"
        echo "3. Click 'Code' → 'Codespaces' → 'Create codespace'"
        echo "4. In the codespace terminal:"
        echo "   dotnet run --urls='http://0.0.0.0:5000'"
        echo "5. Go to 'Ports' tab → right-click port 5000 → 'Make Public'"
        echo "6. Share the generated URL for testing"
        echo ""
        echo "💡 Advantages:"
        echo "✅ Completely free (60 hours/month)"
        echo "✅ No deployment setup needed"
        echo "✅ Instant testing"
        echo "✅ Share with others immediately"
        ;;
    
    5)
        echo ""
        echo "🐳 Local Docker Test"
        echo "==================="
        echo ""
        echo "Building Docker image..."
        docker build -t hotel-booking-api .
        
        if [ $? -eq 0 ]; then
            echo "✅ Docker image built successfully!"
            echo ""
            echo "🚀 Starting container..."
            echo "API will be available at: http://localhost:8080"
            echo ""
            echo "Press Ctrl+C to stop the container"
            echo ""
            docker run -p 8080:8080 \
                -e MONGODB_CONNECTION_STRING="mongodb://localhost:27017" \
                -e DATABASE_NAME="HotelBooking" \
                -e JWT_SECRET="ThisIsASecretKeyForDevelopmentOnly12345" \
                -e JWT_ISSUER="HotelBookingAPI" \
                -e JWT_AUDIENCE="HotelBookingAPI" \
                -e ASPNETCORE_ENVIRONMENT="Development" \
                hotel-booking-api
        else
            echo "❌ Docker build failed!"
            exit 1
        fi
        ;;
    
    *)
        echo "❌ Invalid choice. Please run the script again and choose 1-5."
        exit 1
        ;;
esac

echo ""
echo "🧪 Testing Your Deployed API"
echo "============================"
echo ""
echo "Once deployed, test with these commands:"
echo ""
echo "# Test health endpoint"
echo "curl https://your-app-url.railway.app/health"
echo ""
echo "# Test rooms endpoint"
echo "curl https://your-app-url.railway.app/api/rooms"
echo ""
echo "# Test authentication"
echo "curl -X POST https://your-app-url.railway.app/api/auth/login \\"
echo "  -H 'Content-Type: application/json' \\"
echo "  -d '{\"username\":\"testuser\",\"password\":\"TestPassword123!\"}'"
echo ""
echo "# Test rate limiting (run multiple times)"
echo "for i in {1..10}; do"
echo "  curl -s -o /dev/null -w \"Request \$i: %{http_code}\\n\" \\"
echo "    https://your-app-url.railway.app/api/rooms"
echo "done"
echo ""
echo "🎉 Your secure API with rate limiting is ready for testing!"
echo "📖 Check the DEPLOYMENT_GUIDE.md for detailed instructions."
