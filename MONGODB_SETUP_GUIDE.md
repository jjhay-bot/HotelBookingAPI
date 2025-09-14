# Quick MongoDB Atlas Setup for Deployment

## Step 1: Create Free MongoDB Atlas Account

1. Go to [cloud.mongodb.com](https://cloud.mongodb.com)
2. Sign up with Google/GitHub (easiest)
3. Choose "Shared" (FREE tier)
4. Select AWS and closest region to you
5. Cluster name: `HotelBookingCluster`

## Step 2: Configure Database Access

1. **Create Database User**:
   - Username: `hotelapi`
   - Password: Generate a secure password (save it!)
   - Role: `Atlas admin`

2. **Set Network Access**:
   - Click "Add IP Address"
   - Choose "Allow access from anywhere" (0.0.0.0/0)
   - (For production, restrict to your deployment platform's IPs)

## Step 3: Get Connection String

1. Click "Connect" on your cluster
2. Choose "Connect your application"
3. Driver: Node.js (the connection string format is the same)
4. Copy the connection string:
   ```
   mongodb+srv://hotelapi:<password>@hotelbookingcluster.xxxxx.mongodb.net/?retryWrites=true&w=majority
   ```

## Step 4: Replace Password in Connection String

Replace `<password>` with your actual password:
```
mongodb+srv://hotelapi:YourActualPassword@hotelbookingcluster.xxxxx.mongodb.net/?retryWrites=true&w=majority
```

## Step 5: Environment Variables for Deployment

Use these in your deployment platform:

```bash
MONGODB_CONNECTION_STRING=mongodb+srv://hotelapi:YourActualPassword@hotelbookingcluster.xxxxx.mongodb.net/?retryWrites=true&w=majority
DATABASE_NAME=HotelBooking
JWT_SECRET=YourSuperSecretKeyAtLeast32CharactersLong123456
JWT_ISSUER=HotelBookingAPI
JWT_AUDIENCE=HotelBookingAPI
ASPNETCORE_ENVIRONMENT=Production
```

## Step 6: Test Connection

Once deployed, your API will automatically:
- Connect to MongoDB Atlas
- Create the database and collections
- Start accepting requests

## Free Tier Limits

MongoDB Atlas free tier includes:
- ✅ 512 MB storage
- ✅ Shared RAM and vCPU
- ✅ Clusters never sleep
- ✅ Perfect for testing and small projects

## Security Best Practices

For production:
1. **Restrict Network Access**: Only allow IPs from your deployment platform
2. **Use Specific User Roles**: Create read/write users instead of admin
3. **Enable Auditing**: Track database access
4. **Regular Backups**: Atlas provides automated backups

You're ready to deploy! 🚀
