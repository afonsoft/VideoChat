@echo off
echo Iniciando ambiente de desenvolvimento FamilyMeet com Docker...
echo.

REM Verificar se o Docker está rodando
docker --version > nul 2>&1
if %errorlevel% neq 0 (
    echo Docker não está instalado ou não está no PATH
    echo Por favor, instale o Docker Desktop e tente novamente
    pause
    exit /b 1
)

echo Docker está instalado!
echo.

REM Parar containers existentes
echo Parando containers existentes...
docker-compose -f docker-compose.dev.yml down --remove-orphans

REM Construir e iniciar os serviços
echo.
echo Construindo e iniciando os serviços...
docker-compose -f docker-compose.dev.yml up --build -d

REM Verificar se os serviços iniciaram corretamente
echo.
echo Verificando status dos serviços...
timeout /t 10 /nobreak > nul
docker-compose -f docker-compose.dev.yml ps

echo.
echo Aguardando os serviços ficarem prontos...
timeout /t 30 /nobreak > nul

REM Verificar saúde dos serviços
echo.
echo Verificando saúde dos serviços...
docker-compose -f docker-compose.dev.yml exec postgres pg_isready -U postgres -d FamilyMeet
docker-compose -f docker-compose.dev.yml exec redis redis-cli ping

echo.
echo Ambiente de desenvolvimento está pronto!
echo.
echo Serviços disponíveis:
echo - API: http://localhost:5000
echo - API (HTTPS): https://localhost:5001
echo - Admin Web: http://localhost:4200
echo - Client Web: http://localhost:4201
echo - PostgreSQL: localhost:5432
echo - Redis: localhost:6379
echo.
echo Swagger Documentation: https://localhost:5001/swagger
echo.
echo Para ver os logs: docker-compose -f docker-compose.dev.yml logs -f
echo Para parar: docker-compose -f docker-compose.dev.yml down
echo.
pause
