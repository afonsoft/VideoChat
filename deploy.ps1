# FamilyChat Deployment Script for Windows
# This script builds and deploys the FamilyChat application

Write-Host "🚀 Starting FamilyChat deployment..." -ForegroundColor Green

# Function to print colored output
function Print-Status($message) {
    Write-Host "[INFO] $message" -ForegroundColor Green
}

function Print-Warning($message) {
    Write-Host "[WARNING] $message" -ForegroundColor Yellow
}

function Print-Error($message) {
    Write-Host "[ERROR] $message" -ForegroundColor Red
}

# Check if Docker is installed
try {
    docker --version | Out-Null
} catch {
    Print-Error "Docker is not installed. Please install Docker first."
    exit 1
}

# Check if Docker Compose is installed
try {
    docker-compose --version | Out-Null
} catch {
    Print-Error "Docker Compose is not installed. Please install Docker Compose first."
    exit 1
}

# Create necessary directories
Print-Status "Creating necessary directories..."
New-Item -ItemType Directory -Force -Path logs | Out-Null
New-Item -ItemType Directory -Force -Path ssl | Out-Null

# Stop existing containers
Print-Status "Stopping existing containers..."
try {
    docker-compose -f docker-compose.familychat.yml down --remove-orphans | Out-Null
} catch {
    Print-Warning "No existing containers to stop"
}

# Build and start services
Print-Status "Building and starting FamilyChat services..."
docker-compose -f docker-compose.familychat.yml up --build -d

# Wait for services to be healthy
Print-Status "Waiting for services to be healthy..."
Start-Sleep -Seconds 30

# Check service health
Print-Status "Checking service health..."

# Check API health
try {
    $response = Invoke-WebRequest -Uri "http://localhost:5000/health" -UseBasicParsing -TimeoutSec 5
    if ($response.StatusCode -eq 200) {
        Print-Status "✅ API is healthy"
    }
} catch {
    Print-Warning "⚠️  API might not be ready yet"
}

# Check Frontend health
try {
    $response = Invoke-WebRequest -Uri "http://localhost/health" -UseBasicParsing -TimeoutSec 5
    if ($response.StatusCode -eq 200) {
        Print-Status "✅ Frontend is healthy"
    }
} catch {
    Print-Warning "⚠️  Frontend might not be ready yet"
}

# Show running containers
Print-Status "Running containers:"
docker-compose -f docker-compose.familychat.yml ps

# Show logs
Print-Status "Recent logs:"
docker-compose -f docker-compose.familychat.yml logs --tail=20

Print-Status "🎉 FamilyChat deployment completed!"
Print-Status "📱 Frontend: http://localhost"
Print-Status "🔧 API: http://localhost:5000"
Print-Status "📊 Redis: localhost:6379"

# Show useful commands
Write-Host ""
Write-Host "📋 Useful Commands:" -ForegroundColor Cyan
Write-Host "  View logs: docker-compose -f docker-compose.familychat.yml logs -f" -ForegroundColor White
Write-Host "  Stop services: docker-compose -f docker-compose.familychat.yml down" -ForegroundColor White
Write-Host "  Restart API: docker-compose -f docker-compose.familychat.yml restart familychat-api" -ForegroundColor White
Write-Host "  Restart Frontend: docker-compose -f docker-compose.familychat.yml restart familychat-frontend" -ForegroundColor White
Write-Host "  Access Redis: docker exec -it familychat-redis redis-cli" -ForegroundColor White
