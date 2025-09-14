#!/bin/bash

# 🧪 Test Your Build Commands Locally Before Deploying

echo "🧪 Testing Render Build Commands Locally"
echo "======================================="
echo ""

# Test the exact build command that Render will use
echo "📦 Testing Build Command: dotnet publish -c Release -o out"
echo "--------------------------------------------------------"

if dotnet publish -c Release -o out; then
    echo "✅ Build command successful!"
    echo ""
    
    # Check if the output files exist
    if [ -f "out/HotelBookingAPI.dll" ]; then
        echo "✅ HotelBookingAPI.dll created successfully"
        echo "📁 Build output files:"
        ls -la out/ | head -10
        echo ""
        
        # Test the start command
        echo "🚀 Testing Start Command: dotnet out/HotelBookingAPI.dll"
        echo "--------------------------------------------------------"
        echo "💡 This will start your API locally using the same command Render uses"
        echo "💡 Press Ctrl+C to stop the server when you're done testing"
        echo ""
        
        # Set environment variables for local testing
        export ASPNETCORE_ENVIRONMENT=Development
        export MONGODB_CONNECTION_STRING="mongodb://localhost:27017"
        export DATABASE_NAME="HotelBooking"
        export JWT_SECRET="ThisIsATestSecretKeyForLocalDevelopmentOnly123456"
        export JWT_ISSUER="HotelBookingAPI"
        export JWT_AUDIENCE="HotelBookingAPI"
        export PORT="5000"
        
        echo "🔧 Environment variables set for local testing:"
        echo "   ASPNETCORE_ENVIRONMENT=Development"
        echo "   PORT=5000"
        echo ""
        
        # Start the application
        dotnet out/HotelBookingAPI.dll
        
    else
        echo "❌ HotelBookingAPI.dll not found in out/ directory"
        echo "🔍 Contents of out/ directory:"
        ls -la out/
    fi
    
else
    echo "❌ Build command failed!"
    echo ""
    echo "🔧 Troubleshooting steps:"
    echo "1. Make sure you're in the project root directory"
    echo "2. Check that HotelBookingAPI.csproj exists"
    echo "3. Try running: dotnet restore"
    echo "4. Try running: dotnet build"
fi

echo ""
echo "📝 Summary:"
echo "✅ If both commands worked, your project is ready for Render!"
echo "❌ If any command failed, fix the issues before deploying"
echo ""
echo "🚀 Next steps for Render deployment:"
echo "1. Go to render.com and create a new web service"
echo "2. Use exactly these commands:"
echo "   Build Command: dotnet publish -c Release -o out"
echo "   Start Command: dotnet out/HotelBookingAPI.dll"
echo "3. Add your environment variables (MongoDB connection, JWT secret, etc.)"
