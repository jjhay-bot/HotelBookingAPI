# 🏨 Hotel Booking API

A secure, production-ready .NET 9.0 Web API for hotel room booking with JWT authentication, MongoDB integration, and comprehensive security features.

## 🚀 Quick Start

1. **Prerequisites**: .NET 9.0 SDK, MongoDB Atlas account
2. **Environment**: Copy `.env.example` to `.env` and configure
3. **Run**: `dotnet run` or use Docker

## 📁 Project Structure

```
HotelBookingAPI/
├── 📂 Controllers/          # API endpoints
├── 📂 Models/              # Data models and DTOs  
├── 📂 Services/            # Business logic
├── 📂 Security/            # Security middleware and helpers
├── 📂 Configuration/       # App configuration classes
├── 📂 docs/               # 📚 Documentation
│   ├── security/          # Security guides and best practices
│   ├── deployment/        # Cloud deployment guides
│   ├── guides/           # Implementation guides
│   ├── performance/      # Performance optimization
│   └── architecture/     # System design docs
├── 📂 scripts/           # 🔧 Utility scripts
├── 📂 tests/            # 🧪 HTTP test files
├── 🐳 Dockerfile        # Container configuration
├── ⚙️ appsettings.json   # App configuration
└── 🔑 .env              # Environment variables (create from .env.example)
```

## 🔧 Core Features

- **🔐 JWT Authentication** with role-based authorization
- **🛡️ Security Middleware** - Rate limiting, input validation, injection protection
- **📊 MongoDB Integration** with optimized queries
- **⚡ Performance** - Caching, query protection, memory optimization
- **🚀 Cloud Ready** - Docker, Railway, Render, Azure deployment

## 📖 Documentation

### 🎯 Getting Started
- [Environment Setup](docs/guides/ENVIRONMENT_SETUP.md)
- [Implementation Summary](docs/guides/IMPLEMENTATION_SUMMARY.md)
- [Configuration Guide](docs/guides/CONFIGURATION_QUICK_REFERENCE.md)

### 🔒 Security
- [Security Implementation](docs/security/SECURITY_IMPLEMENTATION_GUIDE.md)
- [Security Checklist](docs/security/SECURITY_CHECKLIST.md)
- [Package Security](docs/security/PACKAGE_RESEARCH_GUIDE.md)

### 🚀 Deployment
- [MongoDB Setup](docs/deployment/MONGODB_SETUP_GUIDE.md)
- [Render Deployment](docs/deployment/RENDER_DEPLOYMENT_GUIDE.md)
- [Alternative Platforms](docs/deployment/ALTERNATIVE_DEPLOYMENT_OPTIONS.md)

### ⚡ Performance
- [Caching Guide](docs/performance/CACHE_CONFIGURATION_GUIDE.md)
- [Database Optimization](docs/performance/DATABASE_PERFORMANCE_ANALYSIS.md)
- [Memory Management](docs/performance/MEMORY_CACHE_VS_REDIS_GUIDE.md)

## 🧪 Testing

Run HTTP tests from the `tests/` folder using VS Code REST Client or any HTTP client:

```bash
# Security tests
tests/test-security-demo.http
tests/test-role-authorization.http

# Feature tests  
tests/test-user-update-fix.http
tests/test-query-protection.http
```

## 🔧 Scripts

```bash
# Security audit
./scripts/security-audit.sh

# Deployment helpers
./scripts/deploy.sh

# Environment setup
./scripts/set-env.sh
```

## 🌐 API Endpoints

- `GET /Health` - Health check
- `POST /api/auth/register` - User registration
- `POST /api/auth/login` - User login
- `GET /api/users` - Get users (Admin only)
- `GET /api/rooms` - Get available rooms
- `POST /api/rooms` - Create room (Admin only)

## 🔑 Environment Variables

Create `.env` file with:

```env
MONGODB_CONNECTION_STRING=your_mongodb_connection
DATABASE_NAME=HotelBookingDb
JWT_SECRET=your_jwt_secret_key
JWT_ISSUER=HotelBookingAPI
JWT_AUDIENCE=HotelBookingAPIUsers
ASPNETCORE_ENVIRONMENT=Development
```

## 🐳 Docker Deployment

```bash
docker build -t hotel-booking-api .
docker run -p 8080:8080 --env-file .env hotel-booking-api
```

## 📊 Tech Stack

- **.NET 9.0** - Modern C# web framework
- **MongoDB** - NoSQL database with Atlas cloud hosting
- **JWT** - Secure token-based authentication
- **Docker** - Containerization for consistent deployment
- **Security Middleware** - Custom protection layers

## 🤝 Contributing

1. Check [Security Guide](docs/security/SECURITY_GUIDE.md) for security requirements
2. Follow [Testing Guide](docs/guides/TESTING_GUIDE.md) for test procedures
3. Review [Architecture Guide](docs/architecture/API_Structure_Guide.md) for design patterns

## 📄 License

This project is open source. Please review security guidelines before production use.

---

**🎯 Ready for Production** | **🔒 Security First** | **⚡ High Performance** | **🚀 Cloud Native**
