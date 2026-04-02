# Database Setup Guide

## 📋 Pré-requisitos

- **PostgreSQL 15+** instalado e rodando
- **.NET 10 SDK** instalado
- **Docker** (opcional, para ambiente containerizado)

## 🚀 Setup Rápido

### Opção 1: Usar Docker (Recomendado)

```bash
# Iniciar todos os serviços com Docker
docker-compose -f docker-compose.dev.yml up --build

# Executar apenas a migration
docker-compose -f docker-compose.dev.yml up --profile migration
```

### Opção 2: Setup Manual

#### 1. Criar Banco de Dados

**Windows (PowerShell):**
```powershell
.\setup-database.ps1
```

**Windows (Batch):**
```cmd
setup-database.bat
```

**Linux/macOS:**
```bash
# Criar banco
psql -U postgres -c "CREATE DATABASE FamilyMeet WITH OWNER = postgres ENCODING = 'UTF8';"

# Habilitar extensão UUID
psql -U postgres -d FamilyMeet -c "CREATE EXTENSION IF NOT EXISTS 'uuid-ossp';"
```

#### 2. Executar Migration

```bash
cd src/api/src/afonsoft.FamilyMeet.DbMigrator
dotnet run
```

## 📁 Estrutura do Banco

### Tabelas Principais

#### `AppChatChatGroups`
- **Id**: UUID (PK)
- **Name**: VARCHAR(128) - Nome do grupo
- **Description**: VARCHAR(500) - Descrição
- **IsPublic**: BOOLEAN - Grupo público ou privado
- **MaxParticipants**: INTEGER - Máximo de participantes
- **IsActive**: BOOLEAN - Status do grupo
- **LastMessageAt**: TIMESTAMP - Última mensagem

#### `AppChatChatMessages`
- **Id**: UUID (PK)
- **ChatGroupId**: UUID (FK) - Referência ao grupo
- **SenderId**: UUID - ID do remetente
- **SenderName**: VARCHAR(128) - Nome do remetente
- **Content**: VARCHAR(4000) - Conteúdo da mensagem
- **Type**: INTEGER - Tipo (0=Text, 1=Image, 2=File, 3=System)
- **IsEdited**: BOOLEAN - Mensagem editada
- **EditedAt**: TIMESTAMP - Data da edição
- **ReplyToMessageId**: UUID (FK) - Resposta a mensagem
- **IsDeleted**: BOOLEAN - Soft delete
- **DeletedAt**: TIMESTAMP - Data da exclusão

#### `AppChatChatParticipants`
- **Id**: UUID (PK)
- **ChatGroupId**: UUID (FK) - Referência ao grupo
- **UserId**: UUID - ID do usuário
- **UserName**: VARCHAR(128) - Nome do usuário
- **IsOnline**: BOOLEAN - Status online
- **LastSeenAt**: TIMESTAMP - Última visualização
- **IsMuted**: BOOLEAN - Usuário silenciado
- **IsBanned**: BOOLEAN - Usuário banido
- **BannedUntil**: TIMESTAMP - Fim do banimento
- **IsCreator**: BOOLEAN - Criador do grupo

### Tabelas do ABP Framework

- **AbpUsers**: Usuários do sistema
- **AbpRoles**: Papéis e permissões
- **AbpTenants**: Multi-tenancy
- **AbpPermissions**: Permissões granulares

## 🔗 Índices Otimizados

### Performance Indexes

```sql
-- Índices compostos para queries críticas
CREATE INDEX idx_chat_messages_group_created 
ON "AppChatChatMessages"("ChatGroupId", "CreationTime" DESC);

CREATE INDEX idx_chat_messages_group_deleted_created 
ON "AppChatChatMessages"("ChatGroupId", "IsDeleted", "CreationTime" DESC);

-- Índices únicos
CREATE UNIQUE INDEX idx_chat_participants_group_user 
ON "AppChatChatParticipants"("ChatGroupId", "UserId");

-- Índices de busca
CREATE INDEX idx_chat_groups_name 
ON "AppChatChatGroups"("Name");

CREATE INDEX idx_chat_groups_active 
ON "AppChatChatGroups"("IsActive");
```

## 🌐 Connection Strings

### Desenvolvimento Local
```json
{
  "ConnectionStrings": {
    "Default": "Host=localhost;Port=5432;Database=FamilyMeet;User ID=postgres;Password=postgres;"
  }
}
```

### Docker
```json
{
  "ConnectionStrings": {
    "Default": "Host=postgres;Port=5432;Database=FamilyMeet;User ID=postgres;Password=postgres;"
  }
}
```

### Produção
```json
{
  "ConnectionStrings": {
    "Default": "Host=your-db-host;Port=5432;Database=FamilyMeet;User ID=app_user;Password=secure_password;SSL Mode=Require;"
  }
}
```

## 📊 Dados Iniciais (Seed)

O sistema cria automaticamente:

### Grupos de Exemplo
1. **General Discussion** - Chat geral para todos
2. **Family Events** - Planejamento de eventos
3. **Kids Corner** - Chat para os mais jovens
4. **Private - Parents Only** - Discussões privadas
5. **Photo Sharing** - Compartilhamento de fotos

### Mensagens de Exemplo
- Mensagens de boas-vindas
- Mensagens de diferentes tipos (Text, Image, System)
- Respostas encadeadas

### Participantes
- Admin (criador de todos os grupos)
- 10 membros da família com diferentes permissões

## 🔧 Troubleshooting

### Problemas Comuns

#### 1. "Connection refused"
**Causa:** PostgreSQL não está rodando
**Solução:**
```bash
# Windows
net start postgresql-x64-15

# Linux/macOS
brew services start postgresql
# ou
sudo systemctl start postgresql
```

#### 2. "Database does not exist"
**Causa:** Banco não foi criado
**Solução:**
```bash
psql -U postgres -c "CREATE DATABASE FamilyMeet;"
```

#### 3. "Extension uuid-ossp does not exist"
**Causa:** Extensão não habilitada
**Solução:**
```bash
psql -U postgres -d FamilyMeet -c "CREATE EXTENSION IF NOT EXISTS 'uuid-ossp';"
```

#### 4. Migration falha
**Causa:** Problemas de conexão ou permissões
**Solução:**
```bash
# Verificar connection string
# Verificar se o usuário tem permissões
# Rodar migration com verbose logging
dotnet run --verbosity detailed
```

### Comandos Úteis

#### Verificar Tabelas
```sql
\dt
\dt AppChat*
```

#### Verificar Estrutura
```sql
\d AppChatChatGroups
\d AppChatChatMessages
\d AppChatChatParticipants
```

#### Limpar Dados
```sql
TRUNCATE TABLE "AppChatChatMessages" CASCADE;
TRUNCATE TABLE "AppChatChatParticipants" CASCADE;
TRUNCATE TABLE "AppChatChatGroups" CASCADE;
```

#### Reset Completo
```bash
# Apagar banco e recriar
psql -U postgres -c "DROP DATABASE IF EXISTS FamilyMeet;"
psql -U postgres -c "CREATE DATABASE FamilyMeet;"
psql -U postgres -d FamilyMeet -c "CREATE EXTENSION IF NOT EXISTS 'uuid-ossp';"
cd src/api/src/afonsoft.FamilyMeet.DbMigrator && dotnet run
```

## 🚀 Performance Tips

### 1. Connection Pooling
```json
{
  "ConnectionStrings": {
    "Default": "Host=localhost;Port=5432;Database=FamilyMeet;User ID=postgres;Password=postgres;Maximum Pool Size=100;Connection Lifetime=300;"
  }
}
```

### 2. Cache Configuration
```json
{
  "Redis": {
    "Configuration": "localhost:6379"
  }
}
```

### 3. Query Optimization
- Use `IncludeDeleted=false` para mensagens ativas
- Paginar com `Skip/Take` ou `PageBy`
- Usar índices compostos para filtros múltiplos

## 📝 Logs e Monitoramento

### Habilitar Logging
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.EntityFrameworkCore.Database.Command": "Information"
    }
  }
}
```

### Verificar Queries SQL
```bash
# No appsettings.json
"EnableSensitiveDataLogging": true,
"EnableDetailedErrors": true
```

## 🔄 Backup e Restore

### Backup
```bash
pg_dump -U postgres -h localhost FamilyMeet > backup.sql
```

### Restore
```bash
psql -U postgres -h localhost FamilyMeet < backup.sql
```

---

**Pronto!** Seu banco de dados está configurado e pronto para uso com o FamilyMeet! 🎉
