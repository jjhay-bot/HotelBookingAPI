#!/bin/bash

# MongoDB Environment Variables Template for Hotel Booking API
# Copy this file to set-env.sh and replace with your actual values
# Usage: source ./set-env.sh

# MongoDB Settings - REPLACE WITH YOUR VALUES
export MongoDbSettings__ConnectionString="mongodb+srv://USERNAME:PASSWORD@CLUSTER.mongodb.net/"

# JWT Settings - REPLACE WITH YOUR VALUES
export Jwt__Key="YourSecretKeyThatShouldBeAtLeast32CharactersLongAndRandomlyGenerated"

# Optional: Only set these if you need different values per environment
# export MongoDbSettings__DatabaseName="HotelBookingDb"
# export MongoDbSettings__UsersCollectionName="Users" 
# export MongoDbSettings__RoomsCollectionName="Rooms"

# JWT Settings (optional - usually same across environments)
# export Jwt__Issuer="HotelBookingAPI"
# export Jwt__Audience="HotelBookingAPIUsers"

echo "Environment variables set successfully!"
echo "MongoDB Connection: $MongoDbSettings__ConnectionString"
echo "Database: $MongoDbSettings__DatabaseName"

# Instructions:
# 1. Copy this file: cp set-env.template.sh set-env.sh
# 2. Edit set-env.sh with your actual MongoDB credentials
# 3. Run: source ./set-env.sh
# 4. Start your application: dotnet run
