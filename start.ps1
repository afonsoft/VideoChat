# VideoChat Project Startup Script for Windows
# This script starts all three projects: API, AdminWeb, and ClientWeb

Write-Host "==========================================" -ForegroundColor Blue
Write-Host "VideoChat Project Startup Script" -ForegroundColor Blue
Write-Host "==========================================" -ForegroundColor Blue
Write-Host ""

# Function to check if a port is in use
function Test-PortInUse {
    param([int]$Port)
    
    try {
        $connection = New-Object System.Net.Sockets.TcpClient
        $connection.Connect("localhost", $Port)
        $connection.Close()
        return $true
    }
    catch {
        return $false
    }
}

# Function to wait for a service to be available
function Wait-ForService {
    param([string]$Url, [string]$ServiceName, [int]$MaxAttempts = 30)
    
    Write-Host "Waiting for $ServiceName to be available..." -ForegroundColor Yellow
    
    $attempt = 1
    while ($attempt -le $MaxAttempts) {
        try {
            $response = Invoke-WebRequest -Uri $Url -UseBasicParsing -TimeoutSec 5 -ErrorAction Stop
            if ($response.StatusCode -eq 200 -or $response.StatusCode -eq 404) {
                Write-Host "$ServiceName is ready!" -ForegroundColor Green
                return $true
            }
        }
        catch {
            # Service not ready yet
        }
        
        Write-Host "Attempt $attempt/$MaxAttempts: $ServiceName not ready yet..." -ForegroundColor Yellow
        Start-Sleep -Seconds 2
        $attempt++
    }
    
    Write-Host "ERROR: $ServiceName failed to start within expected time" -ForegroundColor Red
    return $false
}

# Function to cleanup processes on exit
function Cleanup-Processes {
    Write-Host "`nShutting down all processes..." -ForegroundColor Yellow
    
    # Kill processes by port
    $ports = @(5001, 4200, 4201)
    foreach ($port in $ports) {
        if (Test-PortInUse -Port $port) {
            Write-Host "Killing process on port $port..." -ForegroundColor Yellow
            $process = Get-NetTCPConnection -LocalPort $port -ErrorAction SilentlyContinue
            if ($process) {
                $pid = $process.OwningProcess
                Stop-Process -Id $pid -Force -ErrorAction SilentlyContinue
            }
        }
    }
    
    # Kill all background jobs
    Get-Job | Stop-Job
    Get-Job | Remove-Job
    
    Write-Host "All processes stopped." -ForegroundColor Green
    exit 0
}

# Set up signal handlers
$originalErrorActionPreference = $ErrorActionPreference
$ErrorActionPreference = "SilentlyContinue"

# Check if required ports are available
Write-Host "Checking port availability..." -ForegroundColor Blue

$API_PORT = 5001
$ADMIN_PORT = 4200
$CLIENT_PORT = 4201

if (Test-PortInUse -Port $API_PORT) {
    Write-Host "ERROR: Port $API_PORT is already in use. Please stop the service using this port." -ForegroundColor Red
    exit 1
}

if (Test-PortInUse -Port $ADMIN_PORT) {
    Write-Host "ERROR: Port $ADMIN_PORT is already in use. Please stop the service using this port." -ForegroundColor Red
    exit 1
}

if (Test-PortInUse -Port $CLIENT_PORT) {
    Write-Host "ERROR: Port $CLIENT_PORT is already in use. Please stop the service using this port." -ForegroundColor Red
    exit 1
}

Write-Host "All ports are available. Starting services..." -ForegroundColor Green

# Start API Backend
Write-Host "Starting API Backend on port $API_PORT..." -ForegroundColor Blue
Set-Location "src/api"

if (-not (Test-Path "bin/Debug/net8.0")) {
    Write-Host "Building API..." -ForegroundColor Yellow
    dotnet build
}

$apiScript = {
    param($Port)
    Set-Location "src/api"
    dotnet run --project "src/FamilyMeet.Api.csproj" --urls "http://localhost:$Port"
}
Start-Job -ScriptBlock $apiScript -ArgumentList $API_PORT -Name "API" | Out-Null
Set-Location "../.."

# Wait for API to start
if (-not (Wait-ForService -Url "http://localhost:$API_PORT/health" -ServiceName "API Backend")) {
    Write-Host "Failed to start API Backend. Check logs for details." -ForegroundColor Red
    Cleanup-Processes
}

# Start AdminWeb
Write-Host "Starting AdminWeb on port $ADMIN_PORT..." -ForegroundColor Blue
Set-Location "src/adminWeb"

# Install dependencies if needed
if (-not (Test-Path "node_modules")) {
    Write-Host "Installing AdminWeb dependencies..." -ForegroundColor Yellow
    npm install
}

# Build if needed
if (-not (Test-Path "dist")) {
    Write-Host "Building AdminWeb..." -ForegroundColor Yellow
    npm run build
}

$adminScript = {
    param($Port)
    Set-Location "src/adminWeb"
    npm start
}
Start-Job -ScriptBlock $adminScript -ArgumentList $ADMIN_PORT -Name "AdminWeb" | Out-Null
Set-Location "../.."

# Wait for AdminWeb to start
if (-not (Wait-ForService -Url "http://localhost:$ADMIN_PORT" -ServiceName "AdminWeb")) {
    Write-Host "Failed to start AdminWeb. Check logs for details." -ForegroundColor Red
    Cleanup-Processes
}

# Start ClientWeb
Write-Host "Starting ClientWeb on port $CLIENT_PORT..." -ForegroundColor Blue
Set-Location "src/clientWeb"

# Install dependencies if needed
if (-not (Test-Path "node_modules")) {
    Write-Host "Installing ClientWeb dependencies..." -ForegroundColor Yellow
    npm install
}

# Build if needed
if (-not (Test-Path "dist")) {
    Write-Host "Building ClientWeb..." -ForegroundColor Yellow
    npm run build
}

$clientScript = {
    param($Port)
    Set-Location "src/clientWeb"
    npm start
}
Start-Job -ScriptBlock $clientScript -ArgumentList $CLIENT_PORT -Name "ClientWeb" | Out-Null
Set-Location "../.."

# Wait for ClientWeb to start
if (-not (Wait-ForService -Url "http://localhost:$CLIENT_PORT" -ServiceName "ClientWeb")) {
    Write-Host "Failed to start ClientWeb. Check logs for details." -ForegroundColor Red
    Cleanup-Processes
}

# All services started successfully
Write-Host ""
Write-Host "==========================================" -ForegroundColor Green
Write-Host "All services started successfully!" -ForegroundColor Green
Write-Host "==========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Service URLs:" -ForegroundColor Blue
Write-Host "  API Backend:     http://localhost:$API_PORT" -ForegroundColor Green
Write-Host "  AdminWeb:         http://localhost:$ADMIN_PORT" -ForegroundColor Green
Write-Host "  ClientWeb:        http://localhost:$CLIENT_PORT" -ForegroundColor Green
Write-Host ""
Write-Host "Test Credentials:" -ForegroundColor Blue
Write-Host "  Email:    user@familymeet.com" -ForegroundColor Yellow
Write-Host "  Password: password" -ForegroundColor Yellow
Write-Host ""
Write-Host "Press Ctrl+C to stop all services" -ForegroundColor Yellow
Write-Host ""

# Keep the script running and monitor services
try {
    while ($true) {
        Start-Sleep -Seconds 10
        
        # Check if services are still running
        if (-not (Test-PortInUse -Port $API_PORT)) {
            Write-Host "ERROR: API Backend stopped unexpectedly!" -ForegroundColor Red
            Cleanup-Processes
        }
        
        if (-not (Test-PortInUse -Port $ADMIN_PORT)) {
            Write-Host "ERROR: AdminWeb stopped unexpectedly!" -ForegroundColor Red
            Cleanup-Processes
        }
        
        if (-not (Test-PortInUse -Port $CLIENT_PORT)) {
            Write-Host "ERROR: ClientWeb stopped unexpectedly!" -ForegroundColor Red
            Cleanup-Processes
        }
    }
}
catch {
    Cleanup-Processes
}
finally {
    $ErrorActionPreference = $originalErrorActionPreference
}
