#!/bin/bash

# VideoChat Project Startup Script
# This script starts all three projects: API, AdminWeb, and ClientWeb

echo "=========================================="
echo "VideoChat Project Startup Script"
echo "=========================================="
echo ""

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Function to check if a port is in use
check_port() {
    local port=$1
    if lsof -Pi :$port -sTCP:LISTEN -t >/dev/null 2>&1; then
        return 0
    else
        return 1
    fi
}

# Function to wait for a service to be available
wait_for_service() {
    local url=$1
    local service_name=$2
    local max_attempts=30
    local attempt=1
    
    echo -e "${YELLOW}Waiting for $service_name to be available...${NC}"
    
    while [ $attempt -le $max_attempts ]; do
        if curl -s -o /dev/null -w "%{http_code}" "$url" | grep -q "200\|404"; then
            echo -e "${GREEN} $service_name is ready!${NC}"
            return 0
        fi
        
        echo -e "${YELLOW}Attempt $attempt/$max_attempts: $service_name not ready yet...${NC}"
        sleep 2
        ((attempt++))
    done
    
    echo -e "${RED}ERROR: $service_name failed to start within expected time${NC}"
    return 1
}

# Function to cleanup processes on exit
cleanup() {
    echo -e "\n${YELLOW}Shutting down all processes...${NC}"
    
    # Kill all background jobs
    jobs -p | xargs -r kill
    
    # Kill processes on specific ports
    for port in 5001 4200 4201; do
        if check_port $port; then
            echo -e "${YELLOW}Killing process on port $port...${NC}"
            lsof -ti:$port | xargs -r kill
        fi
    done
    
    echo -e "${GREEN}All processes stopped.${NC}"
    exit 0
}

# Set up signal handlers
trap cleanup SIGINT SIGTERM

# Check if required ports are available
echo -e "${BLUE}Checking port availability...${NC}"

API_PORT=5001
ADMIN_PORT=4200
CLIENT_PORT=4201

if check_port $API_PORT; then
    echo -e "${RED}ERROR: Port $API_PORT is already in use. Please stop the service using this port.${NC}"
    exit 1
fi

if check_port $ADMIN_PORT; then
    echo -e "${RED}ERROR: Port $ADMIN_PORT is already in use. Please stop the service using this port.${NC}"
    exit 1
fi

if check_port $CLIENT_PORT; then
    echo -e "${RED}ERROR: Port $CLIENT_PORT is already in use. Please stop the service using this port.${NC}"
    exit 1
fi

echo -e "${GREEN}All ports are available. Starting services...${NC}"

# Start API Backend
echo -e "${BLUE}Starting API Backend on port $API_PORT...${NC}"
cd src/api
if [ ! -d "bin/Debug/net8.0" ]; then
    echo -e "${YELLOW}Building API...${NC}"
    dotnet build src/afonsoft.FamilyMeet.HttpApi.Host/afonsoft.FamilyMeet.HttpApi.Host.csproj
fi

dotnet run --project src/afonsoft.FamilyMeet.HttpApi.Host/afonsoft.FamilyMeet.HttpApi.Host.csproj --urls "http://localhost:$API_PORT" > ../api.log 2>&1 &
API_PID=$!
cd ../..

# Wait for API to start
if ! wait_for_service "http://localhost:$API_PORT/health" "API Backend"; then
    echo -e "${RED}Failed to start API Backend. Check api.log for details.${NC}"
    cleanup
fi

# Start AdminWeb
echo -e "${BLUE}Starting AdminWeb on port $ADMIN_PORT...${NC}"
cd src/adminWeb

# Install dependencies if needed
if [ ! -d "node_modules" ]; then
    echo -e "${YELLOW}Installing AdminWeb dependencies...${NC}"
    npm install
fi

# Build if needed
if [ ! -d "dist" ]; then
    echo -e "${YELLOW}Building AdminWeb...${NC}"
    npm run build
fi

npm start > ../admin.log 2>&1 &
ADMIN_PID=$!
cd ../..

# Wait for AdminWeb to start
if ! wait_for_service "http://localhost:$ADMIN_PORT" "AdminWeb"; then
    echo -e "${RED}Failed to start AdminWeb. Check admin.log for details.${NC}"
    cleanup
fi

# Start ClientWeb
echo -e "${BLUE}Starting ClientWeb on port $CLIENT_PORT...${NC}"
cd src/clientWeb

# Install dependencies if needed
if [ ! -d "node_modules" ]; then
    echo -e "${YELLOW}Installing ClientWeb dependencies...${NC}"
    npm install
fi

# Build if needed
if [ ! -d "dist" ]; then
    echo -e "${YELLOW}Building ClientWeb...${NC}"
    npm run build
fi

npm start > ../client.log 2>&1 &
CLIENT_PID=$!
cd ../..

# Wait for ClientWeb to start
if ! wait_for_service "http://localhost:$CLIENT_PORT" "ClientWeb"; then
    echo -e "${RED}Failed to start ClientWeb. Check client.log for details.${NC}"
    cleanup
fi

# All services started successfully
echo ""
echo "=========================================="
echo -e "${GREEN}All services started successfully!${NC}"
echo "=========================================="
echo ""
echo -e "${BLUE}Service URLs:${NC}"
echo -e "  ${GREEN}API Backend:${NC}     http://localhost:$API_PORT"
echo -e "  ${GREEN}AdminWeb:${NC}         http://localhost:$ADMIN_PORT"
echo -e "  ${GREEN}ClientWeb:${NC}        http://localhost:$CLIENT_PORT"
echo ""
echo -e "${BLUE}Test Credentials:${NC}"
echo -e "  ${YELLOW}Email:${NC}    user@familymeet.com"
echo -e "  ${YELLOW}Password:${NC} password"
echo ""
echo -e "${BLUE}Log Files:${NC}"
echo -e "  ${YELLOW}API:${NC}       api.log"
echo -e "  ${YELLOW}AdminWeb:${NC}  admin.log"
echo -e "  ${YELLOW}ClientWeb:${NC} client.log"
echo ""
echo -e "${YELLOW}Press Ctrl+C to stop all services${NC}"
echo ""

# Keep the script running and monitor services
while true; do
    sleep 10
    
    # Check if services are still running
    if ! check_port $API_PORT; then
        echo -e "${RED}ERROR: API Backend stopped unexpectedly!${NC}"
        cleanup
    fi
    
    if ! check_port $ADMIN_PORT; then
        echo -e "${RED}ERROR: AdminWeb stopped unexpectedly!${NC}"
        cleanup
    fi
    
    if ! check_port $CLIENT_PORT; then
        echo -e "${RED}ERROR: ClientWeb stopped unexpectedly!${NC}"
        cleanup
    fi
done
