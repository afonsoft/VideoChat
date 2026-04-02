# Script para configurar o banco de dados FamilyMeet
Write-Host "Verificando se o PostgreSQL está rodando..." -ForegroundColor Green

# Verificar se o PostgreSQL está rodando
try {
    $connection = Test-NetConnection -ComputerName localhost -Port 5432 -WarningAction SilentlyContinue
    if (-not $connection.TcpTestSucceeded) {
        Write-Host "PostgreSQL não está rodando na porta 5432" -ForegroundColor Red
        Write-Host "Por favor, inicie o PostgreSQL service ou Docker container" -ForegroundColor Yellow
        Read-Host "Pressione Enter para sair"
        exit 1
    }
    
    Write-Host "PostgreSQL está rodando!" -ForegroundColor Green
}
catch {
    Write-Host "Erro ao verificar PostgreSQL: $_" -ForegroundColor Red
    Read-Host "Pressione Enter para sair"
    exit 1
}

Write-Host "Criando banco de dados FamilyMeet..." -ForegroundColor Green

try {
    # Criar banco de dados
    $createDbQuery = "CREATE DATABASE FamilyMeet WITH OWNER = postgres ENCODING = 'UTF8' LC_COLLATE = 'en_US.utf8' LC_CTYPE = 'en_US.utf8' TABLESPACE = pg_default CONNECTION LIMIT = -1;"
    psql -U postgres -c $createDbQuery
    
    if ($LASTEXITCODE -ne 0) {
        throw "Erro ao criar banco de dados"
    }
    
    Write-Host "Banco de dados FamilyMeet criado com sucesso!" -ForegroundColor Green
}
catch {
    Write-Host "Erro ao criar banco de dados: $_" -ForegroundColor Red
    Write-Host "O banco de dados pode já existir. Continuando..." -ForegroundColor Yellow
}

Write-Host "Habilitando extensão UUID..." -ForegroundColor Green

try {
    # Habilitar extensão UUID
    $uuidQuery = "CREATE EXTENSION IF NOT EXISTS 'uuid-ossp';"
    psql -U postgres -d FamilyMeet -c $uuidQuery
    
    if ($LASTEXITCODE -ne 0) {
        throw "Erro ao habilitar extensão UUID"
    }
    
    Write-Host "Extensão UUID habilitada!" -ForegroundColor Green
}
catch {
    Write-Host "Erro ao habilitar extensão UUID: $_" -ForegroundColor Red
    Read-Host "Pressione Enter para sair"
    exit 1
}

Write-Host "Executando migration..." -ForegroundColor Green

try {
    # Executar migration
    Set-Location "src\api\src\afonsoft.FamilyMeet.DbMigrator"
    dotnet run
    
    if ($LASTEXITCODE -ne 0) {
        throw "Erro ao executar migration"
    }
    
    Write-Host "Migration concluída com sucesso!" -ForegroundColor Green
}
catch {
    Write-Host "Erro ao executar migration: $_" -ForegroundColor Red
    Read-Host "Pressione Enter para sair"
    exit 1
}

Write-Host "Banco de dados FamilyMeet está pronto para uso!" -ForegroundColor Green
Write-Host "" -ForegroundColor White
Write-Host "Próximos passos:" -ForegroundColor Cyan
Write-Host "1. Inicie a API: cd src\api\src\afonsoft.FamilyMeet.HttpApi.Host && dotnet run" -ForegroundColor White
Write-Host "2. Acesse Swagger: https://localhost:44336/swagger" -ForegroundColor White
Write-Host "3. Inicie o frontend admin: cd src\adminWeb && npm start" -ForegroundColor White
Write-Host "" -ForegroundColor White
Read-Host "Pressione Enter para sair"
