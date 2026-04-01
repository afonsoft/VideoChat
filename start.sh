#!/bin/bash

# FamilyMeet Development Startup Script
# This script starts the API, AdminWeb, and ClientWeb in development mode

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
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

print_header() {
    echo -e "${BLUE}========================================${NC}"
    echo -e "${BLUE}       FamilyMeet Development         ${NC}"
    echo -e "${BLUE}========================================${NC}"
}

# Check if required tools are installed
check_requirements() {
    print_status "Checking requirements..."
    
    # Check .NET
    if ! command -v dotnet &> /dev/null; then
        print_error ".NET CLI is not installed. Please install .NET 8 SDK."
        exit 1
    fi
    
    # Check Node.js
    if ! command -v node &> /dev/null; then
        print_error "Node.js is not installed. Please install Node.js 18 or higher."
        exit 1
    fi
    
    # Check npm
    if ! command -v npm &> /dev/null; then
        print_error "npm is not installed. Please install npm."
        exit 1
    fi
    
    print_status "All requirements are satisfied!"
}

# Setup development environment
setup_environment() {
    
    # Create uploads directory if it doesn't exist
    mkdir -p src/api/uploads
    mkdir -p src/api/uploads/avatars
    mkdir -p src/api/uploads/chat-files
    mkdir -p src/api/uploads/call-recordings
    
    print_status "Environment setup completed!"
}

# Build projects
build_projects() {
    print_status "Building projects..."
    
    # Build API
    print_status "Building API..."
    cd src/api
    dotnet build FamilyMeet.HttpApi/FamilyMeet.HttpApi.csproj --configuration Development
    cd ../..
    
    # Build ClientWeb
    print_status "Building ClientWeb..."
    cd src/clientWeb
    npm install
    cd ../..
    
    print_status "Build completed!"
}

# Start API
start_api() {
    print_status "Starting API on port 5000..."
    cd src/api
    dotnet run --project FamilyMeet.HttpApi/FamilyMeet.HttpApi.csproj --configuration Development &
    API_PID=$!
    cd ../..
    
    # Wait for API to start
    sleep 5
    
    # Check if API is running
    if curl -s http://localhost:5000/health > /dev/null; then
        print_status "API is running on http://localhost:5000"
    else
        print_warning "API might not be fully started yet. Continuing..."
    fi
}

# Start ClientWeb
start_clientweb() {
    print_status "Starting ClientWeb on port 4200..."
    cd src/clientWeb
    npm start &
    CLIENTWEB_PID=$!
    cd ../..
    
    # Wait a moment for the client to start
    sleep 3
    print_status "ClientWeb is starting on http://localhost:4200"
}

# Start AdminWeb
start_adminweb() {
    print_status "Starting AdminWeb on port 4201..."
    cd src/adminWeb/angular
    npm install
    npm start &
    ADMINWEB_PID=$!
    cd ../../..
    
    # Wait a moment for the admin to start
    sleep 3
    print_status "AdminWeb is starting on http://localhost:4201"
}

# Cleanup function
cleanup() {
    print_status "Shutting down services..."
    
    if [ ! -z "$API_PID" ]; then
        kill $API_PID 2>/dev/null || true
    fi
    
    if [ ! -z "$CLIENTWEB_PID" ]; then
        kill $CLIENTWEB_PID 2>/dev/null || true
    fi
    
    if [ ! -z "$ADMINWEB_PID" ]; then
        kill $ADMINWEB_PID 2>/dev/null || true
    fi
    
    print_status "All services stopped."
}

# Setup signal handlers
trap cleanup SIGINT SIGTERM

# Main execution
main() {
    print_header
    
    check_requirements
    setup_environment
    build_projects
    
    print_status "Starting all services..."
    
    start_api
    start_clientweb
    start_adminweb
    
    print_header
    print_status "🚀 FamilyMeet Development Environment Started!"
    print_status ""
    print_status "📱 ClientWeb (Chat):     http://localhost:4200"
    print_status "🛠️  AdminWeb (Admin):    http://localhost:4201"
    print_status "🔧 API Backend:         http://localhost:5000"
    print_status "📚 API Documentation:    http://localhost:5000/swagger"
    print_status ""
    print_status "Press Ctrl+C to stop all services"
    print_header
    
    # Wait for user to stop
    wait
}

# Check if script is being sourced or executed
if [[ "${BASH_SOURCE[0]}" == "${0}" ]]; then
    main "$@"
fi
