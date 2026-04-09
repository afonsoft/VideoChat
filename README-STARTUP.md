# VideoChat Project Startup Guide

This guide explains how to run all three projects (API, AdminWeb, and ClientWeb) together for testing.

## Quick Start

### Windows (Recommended)
```powershell
# Run the PowerShell script
.\start.ps1
```

### Linux/macOS
```bash
# Make the script executable and run it
chmod +x start.sh
./start.sh
```

## Manual Startup

If you prefer to start each service manually:

### 1. API Backend (Port 5001)
```bash
cd src/api
dotnet build
dotnet run --project src/FamilyMeet.Api.csproj --urls "http://localhost:5001"
```

### 2. AdminWeb (Port 4200)
```bash
cd src/adminWeb
npm install
npm run build
npm start
```

### 3. ClientWeb (Port 4201)
```bash
cd src/clientWeb
npm install
npm run build
npm start
```

## Service URLs

Once started, the services will be available at:

- **API Backend**: http://localhost:5001
- **AdminWeb**: http://localhost:4200
- **ClientWeb**: http://localhost:4201

## Test Credentials

For testing the login functionality:

- **Email**: `user@familymeet.com`
- **Password**: `password`

## Features to Test

### AdminWeb
- Navigate to http://localhost:4200
- Login with admin credentials
- Access Audit Logs section
- Test filtering and pagination

### ClientWeb
- Navigate to http://localhost:4201
- Login with test credentials or Google OAuth
- Create chat groups
- Send and receive messages
- Test real-time messaging

### API Endpoints
The API provides the following endpoints:

#### Background Jobs
- `GET /api/background-jobs` - List all jobs
- `POST /api/background-jobs` - Create new job
- `PUT /api/background-jobs/{id}` - Update job
- `DELETE /api/background-jobs/{id}` - Delete job

#### Feature Management
- `GET /api/features` - List all features
- `POST /api/features` - Create new feature
- `PUT /api/features/{id}` - Update feature
- `DELETE /api/features/{id}` - Delete feature

#### Permission Management
- `GET /api/permissions` - List all permissions
- `POST /api/permissions` - Create new permission
- `PUT /api/permissions/{id}` - Update permission
- `DELETE /api/permissions/{id}` - Delete permission

#### Enhanced Audit Logging
- `GET /api/enhanced-audit-logs` - List audit logs with filtering
- `POST /api/enhanced-audit-logs` - Create audit log
- `GET /api/enhanced-audit-logs/statistics` - Get audit statistics
- `POST /api/enhanced-audit-logs/export` - Export audit logs

## Troubleshooting

### Port Conflicts
If you encounter port conflicts, make sure ports 5001, 4200, and 4201 are available:
```bash
# Check what's using the ports
netstat -ano | findstr :5001
netstat -ano | findstr :4200
netstat -ano | findstr :4201
```

### Build Issues
- Ensure you have .NET 8+ SDK installed
- Ensure you have Node.js 18+ installed
- Run `dotnet restore` in the API directory
- Run `npm install` in both frontend directories

### Database Issues
The API uses an in-memory database by default. If you want to persist data, update the connection string in `appsettings.json`.

## Development Notes

- The API runs on ASP.NET Core 8
- Both frontend apps use Angular 21+ with standalone components
- Real-time chat functionality uses SignalR (mock implementation)
- All projects include comprehensive unit tests
- The build processes are optimized for production deployment

## Stopping Services

To stop all running services:
- Press `Ctrl+C` in the terminal where you ran the startup script
- Or manually stop each process/task

## Logs

Log files are created in the project root:
- `api.log` - API backend logs
- `admin.log` - AdminWeb logs  
- `client.log` - ClientWeb logs

Check these files if you encounter any issues during startup or runtime.
