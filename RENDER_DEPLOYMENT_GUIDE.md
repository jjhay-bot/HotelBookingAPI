# 🚀 Deploy to Render in 5 Minutes (Free Forever)

Since Railway's trial is used up, Render is your best alternative with a **permanent free tier**.

## Step 1: Prepare MongoDB Atlas (2 minutes)

1. **Go to [cloud.mongodb.com](https://cloud.mongodb.com)**
2. **Sign up** with Google/GitHub (fastest)
3. **Create cluster**:
   - Choose "M0 Sandbox" (FREE)
   - Cloud Provider: AWS
   - Region: Closest to you
   - Cluster Name: `HotelBookingCluster`
4. **Create database user**:
   - Username: `hotelapi`  
   - Password: `SecurePassword123!` (save this!)
5. **Network Access**:
   - Add IP: `0.0.0.0/0` (allow all)
6. **Get connection string**:
   - Click "Connect" → "Connect your application"
   - Copy the connection string
   - Replace `<password>` with your actual password

**Your connection string will look like:**
```
mongodb+srv://hotelapi:SecurePassword123!@hotelbookingcluster.xxxxx.mongodb.net/?retryWrites=true&w=majority
```

## Step 2: Deploy to Render (3 minutes)

1. **Go to [render.com](https://render.com)**
2. **Sign up** with GitHub (free account)
3. **Click "New" → "Web Service"**
4. **Connect your GitHub repository**
   - Authorize Render to access your repos
   - Select your `HotelBookingAPI` repository
5. **Configure service**:
   - **Name**: `hotel-booking-api` (or your choice)
   - **Build Command**: `dotnet publish -c Release -o out`
   - **Start Command**: `dotnet out/HotelBookingAPI.dll`
   - **Instance Type**: Free
6. **Add Environment Variables** (click "Advanced" then "Add Environment Variable"):
   ```
   MONGODB_CONNECTION_STRING = mongodb+srv://hotelapi:SecurePassword123!@hotelbookingcluster.xxxxx.mongodb.net/?retryWrites=true&w=majority
   DATABASE_NAME = HotelBooking
   JWT_SECRET = YourSuperSecretKeyMustBeAtLeast32Characters123
   JWT_ISSUER = HotelBookingAPI
   JWT_AUDIENCE = HotelBookingAPI
   ASPNETCORE_ENVIRONMENT = Production
   PORT = 10000
   ```
7. **Click "Create Web Service"**

## Step 3: Monitor Deployment

Render will:
1. ✅ Clone your repository
2. ✅ Build your .NET application
3. ✅ Deploy to their servers
4. ✅ Provide you with a live URL

**Build time**: ~3-5 minutes

## Step 4: Test Your Live API

Once deployed, your API will be available at:
```
https://hotel-booking-api.onrender.com
```

**Test it immediately:**

```bash
# Replace with your actual Render URL
export API_URL="https://hotel-booking-api.onrender.com"

# Test health endpoint
curl $API_URL/health

# Should return:
# {"status":"healthy","timestamp":"2025-09-14T...","version":"1.0.0","environment":"Production"}

# Test rooms endpoint
curl $API_URL/api/rooms

# Test authentication
curl -X POST $API_URL/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"testuser","password":"TestPassword123!"}'

# Test rate limiting (watch it kick in!)
for i in {1..20}; do
  echo "Request $i: $(curl -s -o /dev/null -w "%{http_code}" $API_URL/api/rooms)"
  sleep 0.1
done
```

## What You Get (All Free!)

✅ **Live API URL** - Share with anyone  
✅ **Automatic HTTPS** - Secure by default  
✅ **All your security features working**:
- JWT authentication
- Rate limiting & DDoS protection  
- Input validation
- Security headers
- Role-based access control

✅ **Professional endpoint** for your portfolio  
✅ **MongoDB database** in the cloud  
✅ **750 hours/month** free (enough for continuous testing)

## Render Free Tier Details

- **Hours**: 750/month (plenty for testing)
- **Sleep**: Apps sleep after 15 minutes of inactivity
- **Wake-up**: First request after sleep takes ~30 seconds
- **Storage**: Persistent disk storage
- **HTTPS**: Automatic SSL certificates
- **Custom domains**: Supported on free tier

## Troubleshooting

### If build fails:
1. Check the build logs in Render dashboard
2. Ensure your repository has all files committed
3. Verify environment variables are set correctly

### If app won't start:
1. Check the service logs
2. Verify MongoDB connection string is correct
3. Ensure JWT_SECRET is at least 32 characters

### If MongoDB connection fails:
1. Check connection string format
2. Verify password is correct (no special URL encoding needed)
3. Ensure IP whitelist includes 0.0.0.0/0

## Portfolio Benefits

Having your API deployed gives you:
- **Live demo** for interviews
- **Professional experience** with cloud deployment
- **Real-world testing** environment
- **Shareable URL** for feedback
- **Production skills** on your resume

## Next Steps

Once deployed:
1. **Test all endpoints** thoroughly
2. **Share the URL** with friends/colleagues for feedback
3. **Monitor performance** in Render dashboard
4. **Consider upgrading** to paid tier for production use
5. **Add more features** to your API

## Cost to Scale

When you're ready for production:
- **Starter Plan**: $7/month (always-on, no sleep)
- **Standard Plan**: $25/month (more resources)
- **Pro Plan**: $85/month (enterprise features)

**You're now live on the internet! 🌐**

Your secure, rate-limited, production-ready API is accessible worldwide. Perfect for your portfolio and real-world testing!
