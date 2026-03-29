#!/bin/bash

# FamilyChat Deployment Script
# This script builds and deploys the FamilyChat application

set -e

echo "🚀 Starting FamilyChat deployment..."

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Function to print colored output
print_status() {
    echo -e "${GREEN}[INFO]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Check if Docker is installed
if ! command -v docker &> /dev/null; then
    print_error "Docker is not installed. Please install Docker first."
    exit 1
fi

# Check if Docker Compose is installed
if ! command -v docker-compose &> /dev/null; then
    print_error "Docker Compose is not installed. Please install Docker Compose first."
    exit 1
fi

# Create necessary directories
print_status "Creating necessary directories..."
mkdir -p logs ssl

# Stop existing containers
print_status "Stopping existing containers..."
docker-compose -f docker-compose.familychat.yml down --remove-orphans || true

# Build and start services
print_status "Building and starting FamilyChat services..."
docker-compose -f docker-compose.familychat.yml up --build -d

# Wait for services to be healthy
print_status "Waiting for services to be healthy..."
sleep 30

# Check service health
print_status "Checking service health..."

# Check API health
if curl -f http://localhost:5000/health > /dev/null 2>&1; then
    print_status "✅ API is healthy"
else
    print_warning "⚠️  API might not be ready yet"
fi

# Check Frontend health
if curl -f http://localhost/health > /dev/null 2>&1; then
    print_status "✅ Frontend is healthy"
else
    print_warning "⚠️  Frontend might not be ready yet"
fi

# Show running containers
print_status "Running containers:"
docker-compose -f docker-compose.familychat.yml ps

# Show logs
print_status "Recent logs:"
docker-compose -f docker-compose.familychat.yml logs --tail=20

print_status "🎉 FamilyChat deployment completed!"
print_status "📱 Frontend: http://localhost"
print_status "🔧 API: http://localhost:5000"
print_status "📊 Redis: localhost:6379"

# Show useful commands
echo ""
echo "📋 Useful Commands:"
echo "  View logs: docker-compose -f docker-compose.familychat.yml logs -f"
echo "  Stop services: docker-compose -f docker-compose.familychat.yml down"
echo "  Restart API: docker-compose -f docker-compose.familychat.yml restart familychat-api"
echo "  Restart Frontend: docker-compose -f docker-compose.familychat.yml restart familychat-frontend"
echo "  Access Redis: docker exec -it familychat-redis redis-cli"
