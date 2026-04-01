@echo off
REM FamilyMeet Development Startup Script for Windows
REM This script starts the API, AdminWeb, and ClientWeb in development mode

setlocal enabledelayedexpansion

echo ========================================
echo        FamilyMeet Development
echo ========================================
echo.

REM Check if .NET is installed
dotnet --version >nul 2>&1
if %errorlevel% neq 0 (
    echo [ERROR] .NET CLI is not installed. Please install .NET 8 SDK.
    pause
    exit /b 1
)

REM Check if Node.js is installed
node --version >nul 2>&1
if %errorlevel% neq 0 (
    echo [ERROR] Node.js is not installed. Please install Node.js 18 or higher.
    pause
    exit /b 1
)

REM Check if npm is installed
npm --version >nul 2>&1
if %errorlevel% neq 0 (
    echo [ERROR] npm is not installed. Please install npm.
    pause
    exit /b 1
)

echo [INFO] All requirements are satisfied!
echo.

REM Create uploads directories
if not exist "src\api\uploads" mkdir "src\api\uploads"
if not exist "src\api\uploads\avatars" mkdir "src\api\uploads\avatars"
if not exist "src\api\uploads\chat-files" mkdir "src\api\uploads\chat-files"
if not exist "src\api\uploads\call-recordings" mkdir "src\api\uploads\call-recordings"

echo [INFO] Environment setup completed!
echo.

REM Build projects
echo [INFO] Building projects...

REM Build API
echo [INFO] Building API...
cd src\api
dotnet build FamilyMeet.HttpApi\FamilyMeet.HttpApi.csproj --configuration Development
cd ..\..

REM Build ClientWeb
echo [INFO] Building ClientWeb...
cd src\clientWeb
npm install
cd ..\..

echo [INFO] Build completed!
echo.

echo [INFO] Starting all services...

REM Start API
echo [INFO] Starting API on port 5000...
cd src\api
start "FamilyMeet API" cmd /k "dotnet run --project FamilyMeet.HttpApi\FamilyMeet.HttpApi.csproj --configuration Development"
cd ..\..

REM Wait for API to start
timeout /t 5 /nobreak >nul

REM Start ClientWeb
echo [INFO] Starting ClientWeb on port 4200...
cd src\clientWeb
start "FamilyMeet ClientWeb" cmd /k "npm start"
cd ..\..

REM Wait a moment for the client to start
timeout /t 3 /nobreak >nul

REM Start AdminWeb
echo [INFO] Starting AdminWeb on port 4201...
cd src\adminWeb\src
npm install
start "FamilyMeet AdminWeb" cmd /k "npm start -- --port 4201"
cd ..\..\..

REM Wait a moment for the admin to start
timeout /t 3 /nobreak >nul

echo.
echo ========================================
echo   FamilyMeet Development Environment Started!
echo ========================================
echo.
echo   ClientWeb (Chat):     http://localhost:4200
echo   AdminWeb (Admin):    http://localhost:4201
echo   API Backend:         http://localhost:5000
echo   API Documentation:    http://localhost:5000/swagger
echo.
echo   Press any key to continue...
echo ========================================

pause >nul
