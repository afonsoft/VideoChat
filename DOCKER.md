# Docker Setup for FamilyMeet

## Overview

This document explains how to build and run the FamilyMeet application using Docker containers.

## Architecture

The FamilyMeet application consists of:
- **Backend API**: .NET 8 Web API
- **ClientWeb**: Angular chat application (port 4200)
- **AdminWeb**: ABP Angular admin application (port 4201)
- **PostgreSQL**: Database (port 5432)
- **Redis**: Cache (port 6379)

## Quick Start

### Prerequisites
- Docker and Docker Compose installed
- Docker Engine 20.10+ (recommended)

### Start all services
```bash
# Clone the repository
git clone https://github.com/afonsoft/VideoChat.git
cd VideoChat

# Build and start all services
docker-compose up --build
```

### Access the applications
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

### Database Configuration
```yaml
environment:
  ConnectionStrings__DefaultConnection: Host=postgres;Database=FamilyChat_db;Username=postgres;Password=your_password
```

### Redis Configuration
```yaml
environment:
  Redis__Host: redis
  Redis__Port: 6379
  Redis__Password: your_redis_password
```

### JWT Configuration
```yaml
environment:
  Jwt__Issuer: FamilyMeet
  Jwt__Audience: FamilyMeetUsers
  Jwt__Key: your_secret_key_here
  Jwt__ExpirationMinutes: 60
```

## Volumes

### Persistent Data
- **PostgreSQL data**: Stored in `postgres_data` volume
- **Redis data**: Stored in `redis_data` volume
- **Application logs**: Stored in `logs` volume

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
| PostgreSQL | 5432 | 5432 | Database |
| Redis | 6379 | 6379 | Cache |

## Health Checks

All services include health checks:
- **API**: `/health` endpoint
- **Frontend**: HTTP response check
- **Database**: PostgreSQL readiness check
- **Redis**: Redis ping check

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
- **Database**: 1GB RAM, 0.5 CPU
- **Redis**: 256MB RAM, 0.25 CPU

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

# Test database connection
docker-compose exec Meet-Api curl -f http://localhost:5000/health
```

#### 3. Build Failures
```bash
# Clean build cache
docker-compose down -v
docker system prune -f

# Rebuild without cache
docker-compose build --no-cache
```

#### 4. Memory Issues
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

### 3. Image Security
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
```bash
# Backup database
docker-compose exec postgres pg_dump -U postgres FamilyChat_db > backup.sql

# Backup volumes
docker run --rm -v postgres_data:/data -v $(pwd)/backup:/backup alpine tar czf /backup/backup.tar.gz /data
```

### Disaster Recovery
```bash
# Restore from backup
docker-compose down -v
docker-compose up -d
docker-compose exec postgres psql -U postgres -d FamilyChat_db < backup.sql
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

## Support

For issues with Docker setup:
1. Check container logs: `docker-compose logs [service-name]`
2. Verify network connectivity: `docker network ls`
3. Check resource usage: `docker stats`
4. Review this documentation and troubleshooting section

---

*Last updated: March 31, 2026*
