# Configuração do Banco de Dados PostgreSQL

## Passo 1: Instalar PostgreSQL

### Windows
1. Baixe o PostgreSQL em: https://www.postgresql.org/download/windows/
2. Execute o instalador e anote a senha do usuário postgres
3. Adicione o PostgreSQL ao PATH durante a instalação

### macOS
```bash
# Usar Homebrew
brew install postgresql
brew services start postgresql
```

### Linux (Ubuntu/Debian)
```bash
sudo apt update
sudo apt install postgresql postgresql-contrib
sudo systemctl start postgresql
sudo systemctl enable postgresql
```

## Passo 2: Configurar o Banco de Dados

### Opção A: Via linha de comando (recomendado)
```bash
# Conectar ao PostgreSQL
psql -U postgres

# Executar o script de setup
\i database-setup.sql
```

### Opção B: Via pgAdmin
1. Abra o pgAdmin
2. Conecte com o usuário postgres
3. Execute o script `database-setup.sql`

## Passo 3: Verificar Configuração

Conecte ao banco e verifique se foi criado:
```bash
psql -U postgres -d simpleconnect
\dt
```

## Passo 4: Configurar Connection String

No arquivo `src/SimpleConnect.HttpApi/appsettings.json`, verifique:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=SimpleConnect;Username=postgres;Password=sua_senha"
  }
}
```

## Passo 5: Executar a Aplicação

```bash
cd src/SimpleConnect.HttpApi
dotnet run
```

A aplicação irá:
1. ✅ Criar automaticamente as tabelas (Code-First)
2. ✅ Inserir dados iniciais (seed)
3. ✅ Iniciar o servidor na porta 5000
4. ✅ Disponibilizar o SignalR Hub em `/hubs/communication`
5. ✅ Disponibilizar a API Swagger em `/swagger`

## Estrutura do Banco Criada

### Tabelas Principais:
- **ChatGroups**: Grupos de chat e videochamada
- **ChatGroupMembers**: Membros dos grupos
- **ChatMessages**: Mensagens trocadas
- **ChatMessageAttachments**: Anexos das mensagens

### Índices Otimizados:
- `ChatGroups_CreatorId`: Para buscar grupos por criador
- `ChatMessages_ChatGroupId_CreatedAt`: Para paginação de mensagens
- `ChatGroupMembers_UserId`: Para buscar grupos do usuário

## Dados Iniciais (Seed)

A aplicação criará automaticamente:

### Grupos de Exemplo:
1. **Geral** - Chat para conversas informais
2. **Trabalho** - Discussões profissionais
3. **Vídeo Conferência** - Sala para videochamadas

### Usuários de Exemplo:
- Usuário Demo (criador dos grupos)
- Maria Silva
- João Santos  
- Ana Costa

### Mensagens de Exemplo:
- Mensagens de boas-vindas
- Mensagens de teste em cada grupo
- Mensagens de sistema para demonstração

## Troubleshooting

### Problema: "Connection refused"
- Verifique se o PostgreSQL está rodando
- Verifique se a porta 5432 está liberada
- Confirme usuário e senha

### Problema: "Database does not exist"
- Execute o script `database-setup.sql`
- Verifique se o nome do banco está correto

### Problema: "Permission denied"
- Verifique as permissões do usuário
- Conceda privilégios ao usuário da aplicação

## Backup e Restore

### Backup
```bash
pg_dump -U postgres simpleconnect > backup.sql
```

### Restore
```bash
psql -U postgres simpleconnect < backup.sql
```

## Performance

O banco está configurado com:
- **Índices compostos** para queries otimizadas
- **Paginação** eficiente de mensagens
- **Relacionamentos** com cascade delete
- **Constraints** para integridade de dados

## Próximo Passo

Após configurar o banco, a aplicação estará pronta para uso com:
- ✅ API REST funcional
- ✅ SignalR para comunicação em tempo real
- ✅ Dados de exemplo para testes
- ✅ Swagger para documentação da API
