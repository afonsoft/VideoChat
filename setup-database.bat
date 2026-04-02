@echo off
echo Verificando se o PostgreSQL está rodando...

REM Verificar se o PostgreSQL está rodando na porta 5432
netstat -an | findstr ":5432" > nul
if %errorlevel% neq 0 (
    echo PostgreSQL não está rodando na porta 5432
    echo Por favor, inicie o PostgreSQL service ou Docker container
    pause
    exit /b 1
)

echo PostgreSQL está rodando!

echo Criando banco de dados FamilyMeet...

REM Conectar ao PostgreSQL e criar o banco
psql -U postgres -c "CREATE DATABASE FamilyMeet WITH OWNER = postgres ENCODING = 'UTF8' LC_COLLATE = 'en_US.utf8' LC_CTYPE = 'en_US.utf8' TABLESPACE = pg_default CONNECTION LIMIT = -1;"

if %errorlevel% neq 0 (
    echo Erro ao criar banco de dados
    pause
    exit /b 1
)

echo Banco de dados FamilyMeet criado com sucesso!

echo Habilitando extensão UUID...

psql -U postgres -d FamilyMeet -c "CREATE EXTENSION IF NOT EXISTS \"uuid-ossp\";"

if %errorlevel% neq 0 (
    echo Erro ao habilitar extensão UUID
    pause
    exit /b 1
)

echo Extensão UUID habilitada!

echo Executando migration...

cd src\api\src\afonsoft.FamilyMeet.DbMigrator
dotnet run

if %errorlevel% neq 0 (
    echo Erro ao executar migration
    pause
    exit /b 1
)

echo Migration concluída com sucesso!
echo Banco de dados FamilyMeet está pronto para uso!

pause
