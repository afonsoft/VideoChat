# Docker Setup for FamilyMeet

## Overview

This document explains how to build and run the FamilyMeet application using Docker containers.

## Architecture

The FamilyMeet application consists of:
- **Backend API**: .NET 8 Web API
- **ClientWeb**: Angular chat application (port 4200)
- **AdminWeb**: ABP Angular admin application (port 4201)
- **PostgreSQL**: Database (hosted externally)
- **Redis**: Cache (hosted externally)

## Quick Start

### Prerequisites
- Docker and Docker Compose installed
- Docker Engine 20.10+ (recommended)
- PostgreSQL and Redis running on host (or external servers)

### Start all services
```bash
# Clone the repository
git clone https://github.com/afonsoft/VideoChat.git
cd VideoChat

# Build and start all services
docker-compose up --build
```

### Access applications
- **ClientWeb**: http://localhost:4200
- **AdminWeb**: http://localhost:4201
- **API**: http://localhost:5000
- **API Swagger**: http://localhost:5000/swagger

## Individual Services

### Backend API
```bash
# Build and start only the API
docker-compose up --build Meet-Api

# View logs
docker-compose logs -f Meet-Api
```

### ClientWeb (Chat)
```bash
# Build and start only the client web
docker-compose up --build Client-Web

# View logs
docker-compose logs -f Client-Web
```

### AdminWeb (Admin)
```bash
# Build and start only the admin web
docker-compose up --build Admin-Web

# View logs
docker-compose logs -f Admin-Web
```

## Development Workflow

### 1. Development Mode
```bash
# Start with hot reload for development
docker-compose -f docker-compose.dev.yml up --build
```

### 2. Production Mode
```bash
# Start in production mode
docker-compose -f docker-compose.prod.yml up --build -d
```

### 3. Stop Services
```bash
# Stop all services
docker-compose down

# Stop specific service
docker-compose stop Client-Web
```

### 4. Clean Up
```bash
# Remove containers and networks
docker-compose down -v

# Remove images (optional)
docker system prune -f
```

## Environment Variables

### Using .env File

Create a `.env` file based on `.env.example`:

```bash
# Copy example file
cp .env.example .env

# Edit with your values
nano .env
```

### Database Configuration (External PostgreSQL)
```yaml
# Using environment variables
environment:
  ConnectionStrings__DefaultConnection: Host=${POSTGRESDB_HOST:-host.docker.internal};Database=${POSTGRESDB_NAME:-FamilyChat_db};Username=${POSTGRESDB_USER:-postgres};Password=${POSTGRESDB_PASSWORD:-postgres};Port=${POSTGRESDB_PORT:-5432}
```

### Redis Configuration (External Redis)
```yaml
# Using environment variables
environment:
  Redis__Host: ${REDIS_HOST:-host.docker.internal}
  Redis__Port: ${REDIS_PORT:-6379}
  Redis__Password: ${REDIS_PASSWORD:-}
  Redis__InstanceName: ${REDIS_INSTANCE_NAME:-FamilyMeet}
```

### JWT Configuration
```yaml
# Using environment variables
environment:
  Jwt__Issuer: ${JWT_ISSUER:-FamilyMeet}
  Jwt__Audience: ${JWT_AUDIENCE:-FamilyMeetUsers}
  Jwt__Key: ${JWT_KEY:-FamilyMeetSecretKey123456789}
  Jwt__ExpirationMinutes: ${JWT_EXPIRATION_MINUTES:-60}
```

### Port Configuration
```yaml
# Using environment variables
ports:
  - "${API_PORT:-5000}:5000"
  - "${CLIENTWEB_PORT:-4200}:80"
  - "${ADMINWEB_PORT:-4201}:80"
```

## Development vs Production

### Development Environment
Use `docker-compose.dev.yml` for development:

```bash
# Uses .env file
docker-compose -f docker-compose.dev.yml up --build

# Features:
- Hot reload with source mounting
- Development environment variables
- Debugging enabled
- Performance optimizations disabled
```

### Production Environment
Use `docker-compose.yml` for production:

```bash
# Uses default values
docker-compose up --build -d

# Features:
- Optimized builds
- Production environment
- Performance optimizations enabled
- No source mounting
```

## Environment-Specific Variables

### Local Development
```bash
# .env for local development
POSTGRESDB_HOST=192.168.68.113
REDIS_HOST=192.168.68.113
POSTGRESDB_PASSWORD=your_local_password
REDIS_PASSWORD=your_local_redis_password
```

### Docker Development
```bash
# .env for Docker containers
POSTGRESDB_HOST=host.docker.internal
REDIS_HOST=host.docker.internal
POSTGRESDB_PASSWORD=postgres
REDIS_PASSWORD=
```

### Production
```bash
# .env for production
POSTGRESDB_HOST=your_production_db_host
REDIS_HOST=your_production_redis_host
POSTGRESDB_PASSWORD=your_production_password
REDIS_PASSWORD=your_production_redis_password
JWT_KEY=your_super_secret_production_key
```

## External Services Setup

### PostgreSQL (Host)
Since PostgreSQL is hosted externally, make sure:

1. **Database exists**: Create `FamilyChat_db` database
2. **User permissions**: Ensure user has proper permissions
3. **Network access**: Docker containers can reach the database
4. **Connection string**: Update with correct host credentials

```sql
-- Create database
CREATE DATABASE FamilyChat_db;

-- Create user (if needed)
CREATE USER postgres WITH PASSWORD 'your_password';
GRANT ALL PRIVILEGES ON DATABASE FamilyChat_db TO postgres;
```

### Redis (Host)
Ensure Redis is accessible from Docker containers:

1. **Network access**: Allow connections from Docker network
2. **Authentication**: Configure password if required
3. **Port mapping**: Ensure port 6379 is accessible

## Volumes

### Persistent Data
- **Application logs**: Stored in `logs` volume
- **Custom volumes**: You can mount additional directories

### Custom Volumes
You can mount custom directories:
```yaml
volumes:
  - ./custom-logs:/app/logs
  - ./custom-data:/app/data
```

## Networking

### Internal Network
All services communicate through the `familymeet-network` bridge network.

### Port Mapping
| Service | Internal Port | External Port | Description |
|---------|---------------|---------------|-------------|
| Meet-Api | 5000 | 5000 | Backend API |
| Client-Web | 80 | 4200 | Chat frontend |
| Admin-Web | 80 | 4201 | Admin frontend |

### External Services
| Service | Host Port | Description |
|---------|------------|-------------|
| PostgreSQL | 5432 | External database |
| Redis | 6379 | External cache |

## Health Checks

All services include health checks:
- **API**: `/health` endpoint
- **Frontend**: HTTP response check

### Health Check Status
```bash
# Check health of all services
docker-compose ps

# Detailed health information
docker inspect --format='{{.State.Health}}' $(docker-compose ps -q)
```

## Performance Optimization

### Resource Limits
- **API**: 1GB RAM, 1 CPU
- **ClientWeb**: 256MB RAM, 0.25 CPU
- **AdminWeb**: 512MB RAM, 0.5 CPU

### Build Optimization
- Multi-stage builds reduce image size
- `.dockerignore` excludes unnecessary files
- Layer caching for faster builds

## Troubleshooting

### Common Issues

#### 1. Port Conflicts
```bash
# Check if ports are in use
netstat -tulpn | grep :4200
netstat -tulpn | grep :5000

# Kill processes using ports
sudo lsof -ti:4200
sudo lsof -ti:5000
```

#### 2. Database Connection Issues
```bash
# Check database logs
docker-compose logs Meet-Api

# Test database connection from container
docker-compose exec Meet-Api curl -f http://localhost:5000/health
```

#### 3. External Service Connectivity
```bash
# Test PostgreSQL connection
docker-compose exec Meet-Api ping your_postgres_host

# Test Redis connection
docker-compose exec Meet-Api redis-cli -h your_redis_host -p 6379 ping
```

#### 4. Build Failures
```bash
# Clean build cache
docker-compose down -v
docker system prune -f

# Rebuild without cache
docker-compose build --no-cache
```

#### 5. Memory Issues
```bash
# Monitor resource usage
docker stats

# Check container logs for OOM errors
docker-compose logs Meet-Api | grep -i "killed\|oom"
```

## Production Deployment

### Environment-Specific Configurations

#### Development
```yaml
# docker-compose.dev.yml
environment:
  ASPNETCORE_ENVIRONMENT: Development
  DOTNET_gcServer: "false"
```

#### Staging
```yaml
# docker-compose.staging.yml
environment:
  ASPNETCORE_ENVIRONMENT: Staging
  DOTNET_gcServer: "true"
```

#### Production
```yaml
# docker-compose.prod.yml
environment:
  ASPNETCORE_ENVIRONMENT: Production
  DOTNET_gcServer: "true"
  DOTNET_gcConcurrent: "true"
```

## Security Considerations

### 1. Secrets Management
```bash
# Use environment variables for secrets
# Never hardcode passwords in Dockerfile
# Use Docker secrets or external secret management
```

### 2. Network Security
```yaml
# Internal network isolation
# Only expose necessary ports
# Use HTTPS in production
```

### 3. External Service Security
- **PostgreSQL**: Use SSL connections, strong passwords
- **Redis**: Enable authentication, network restrictions
- **API**: Configure firewall rules for external access

### 4. Image Security
```bash
# Use specific image tags
# Regularly update base images
# Scan images for vulnerabilities
docker scan Meet-Api
```

## Monitoring

### Logs Aggregation
```bash
# Centralized logging
docker-compose logs -f > application.log

# Log rotation setup
logging:
  driver: "json-file"
  options:
    max-size: "10m"
    max-file: "3"
```

### Metrics Collection
```bash
# Resource usage monitoring
docker stats --no-stream

# Custom health check endpoints
curl http://localhost:5000/metrics
```

## Backup and Recovery

### Data Backup
Since PostgreSQL and Redis are external, use their native backup tools:

```bash
# Backup PostgreSQL (run on host)
pg_dump -h your_postgres_host -U postgres -d FamilyChat_db > backup.sql

# Backup Redis (run on host)
redis-cli --rdb /path/to/backup.rdb
```

### Disaster Recovery
```bash
# Restore PostgreSQL (run on host)
psql -h your_postgres_host -U postgres -d FamilyChat_db < backup.sql

# Restore Redis (run on host)
redis-cli --rdb /path/to/backup.rdb
```

## Advanced Usage

### Custom Dockerfiles
You can use custom Dockerfiles for different environments:
```bash
# Development build
docker-compose -f docker-compose.dev.yml build

# Production build
docker-compose -f docker-compose.prod.yml build
```

### Service Scaling
```bash
# Scale frontend services
docker-compose up --scale Client-Web=3 --scale Admin-Web=2

# Update load balancer configuration
# (Requires additional setup for load balancing)
```

## Best Practices

1. **Use Multi-Stage Builds**: Reduces final image size
2. **Optimize Layer Caching**: Order Dockerfile operations efficiently
3. **Minimize Attack Surface**: Only expose necessary ports
4. **Use Health Checks**: Ensure service reliability
5. **Monitor Resources**: Set appropriate limits and reservations
6. **Secure Secrets**: Use environment variables, not hardcoded values
7. **Regular Updates**: Keep base images updated
8. **Log Everything**: Implement comprehensive logging strategy
9. **External Services**: Properly configure external dependencies
10. **Network Isolation**: Use Docker networks for service communication

## Support

For issues with Docker setup:
1. Check container logs: `docker-compose logs [service-name]`
2. Verify network connectivity: `docker network ls`
3. Check resource usage: `docker stats`
4. Test external service connectivity
5. Review this documentation and troubleshooting section

---

*Last updated: March 31, 2026*
