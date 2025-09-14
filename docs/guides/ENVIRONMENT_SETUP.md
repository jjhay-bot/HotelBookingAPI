# Environment Variables Setup

This project uses environment variables to securely manage sensitive configuration like database connection strings.

## Quick Setup (Development)

1. **Copy the template:**
   ```bash
   cp set-env.template.sh set-env.sh
   ```

2. **Edit `set-env.sh` with your actual MongoDB credentials:**
   ```bash
   nano set-env.sh  # or use your preferred editor
   ```

3. **Load environment variables:**
   ```bash
   source ./set-env.sh
   ```

4. **Run the application:**
   ```bash
   dotnet run
   ```

## Required Environment Variables

| Variable | Description | Example |
|----------|-------------|---------|
| `MongoDbSettings__ConnectionString` | MongoDB connection string | `mongodb+srv://user:pass@cluster.mongodb.net/` |
| `Jwt__Key` | JWT signing key (32+ characters) | `YourSecretKeyThatShouldBeAtLeast32Chars...` |

## Optional Environment Variables

| Variable | Description | Default |
|----------|-------------|---------|
| `MongoDbSettings__DatabaseName` | Database name | `HotelBookingDb` |
| `MongoDbSettings__UsersCollectionName` | Users collection | `Users` |
| `MongoDbSettings__RoomsCollectionName` | Rooms collection | `Rooms` |
| `Jwt__Issuer` | JWT issuer | `HotelBookingAPI` |
| `Jwt__Audience` | JWT audience | `HotelBookingAPIUsers` |

## Production Deployment

### Option 1: Set directly on server
```bash
export MongoDbSettings__ConnectionString="your-connection-string"
export MongoDbSettings__DatabaseName="HotelBookingDb"
# ... other variables
```

### Option 2: Use cloud provider environment variables
- **Azure**: App Service Configuration
- **AWS**: Elastic Beanstalk Environment Properties
- **Heroku**: Config Vars
- **Docker**: Environment variables in compose or runtime

## Security Notes

- ✅ **DO**: Use environment variables for sensitive data
- ✅ **DO**: Keep `set-env.sh` in `.gitignore`
- ✅ **DO**: Use `set-env.template.sh` for team sharing
- ❌ **DON'T**: Commit actual credentials to Git
- ❌ **DON'T**: Put secrets in `appsettings.json`

## Troubleshooting

**Connection issues?**
1. Verify environment variables are set: `echo $MongoDbSettings__ConnectionString`
2. Check MongoDB Atlas IP whitelist
3. Verify credentials in MongoDB Atlas

**Application not reading environment variables?**
1. Make sure you ran `source ./set-env.sh` in the same terminal
2. Restart your application after setting variables
3. Check that `builder.Configuration.AddEnvironmentVariables()` is in Program.cs
