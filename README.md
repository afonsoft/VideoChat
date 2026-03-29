# SimpleConnect - Aplicação de Comunicação em Tempo Real

## Arquitetura Implementada

### Backend (.NET 10)
- **Domain Layer**: Entidades ChatGroup, ChatMessage, ChatGroupMember, ChatMessageAttachment
- **Value Objects**: CallParticipant, WebRTCMessage para gerenciamento de chamadas
- **Application Layer**: DTOs, Services e AutoMapper
- **Infrastructure**: Entity Framework Core com PostgreSQL (Code-First)
- **API**: ASP.NET Core Web API com SignalR Hub
- **SignalR**: Hub completo com métodos de chat e sinalização WebRTC

### Frontend Web (Angular 21)
- **SignalR Client**: Serviço de comunicação em tempo real
- **WebRTC Service**: Serviço para chamadas de vídeo P2P
- **API Service**: Cliente HTTP para comunicação REST
- **Componentes**: ChatRoom com interface completa de chat e vídeo

### Mobile (.NET MAUI)
- **Models**: Entidades compartilhadas para iOS, Android, Windows
- **Dependencies**: SignalR Client, HTTP Client, MVVM Toolkit

## 🚀 Funcionalidades Implementadas

### 1. Chat em Grupo
- ✅ Criação de grupos com até 10 participantes
- ✅ Envio de mensagens em tempo real
- ✅ Edição e exclusão de mensagens
- ✅ Respostas a mensagens
- ✅ Histórico de conversas com paginação

### 2. Videochamada em Grupo
- ✅ Chamadas com até 10 participantes simultâneos
- ✅ WebRTC para comunicação P2P
- ✅ Sinalização via SignalR (offer/answer/ice-candidates)
- ✅ Controles de áudio e vídeo
- ✅ Status dos participantes (conectado, mudo, vídeo desligado)

### 3. Gerenciamento de Grupos
- ✅ Entrada e saída de grupos
- ✅ Permissões básicas (criador/membros)
- ✅ Lista de grupos por usuário
- ✅ Informações de membros ativos

### 4. Banco de Dados PostgreSQL (Code-First)
- ✅ **Criação Automática**: Tabelas criadas ao rodar a aplicação
- ✅ **Seed Inicial**: Dados de exemplo inseridos automaticamente
- ✅ Índices otimizados para performance de chat
- ✅ Relacionamentos com cascade delete
- ✅ Configuração EF Core com migrations automáticas

## 🗄️ Estrutura do Projeto

```
SimpleConnect/
├── src/
│   ├── SimpleConnect.Domain.Shared/     # Enums, Value Objects, Constants
│   ├── SimpleConnect.Domain/             # Entidades de negócio
│   ├── SimpleConnect.Application.Contracts/ # DTOs e Interfaces
│   ├── SimpleConnect.Application/       # Services e AutoMapper
│   ├── SimpleConnect.EntityFrameworkCore/ # DbContext e Repositories
│   ├── SimpleConnect.HttpApi/           # API Controllers e SignalR Hub
│   └── SimpleConnect.Web/               # Projeto web (configuração)
├── SimpleConnect.Web.Angular/           # Frontend Angular
├── SimpleConnect.Mobile/                # Projeto MAUI
├── database-setup.sql                   # Script SQL para setup do PostgreSQL
└── DATABASE.md                          # Guia completo de configuração
```

## 🛠️ Tecnologias Utilizadas

### Backend
- .NET 10
- ASP.NET Core Web API
- Entity Framework Core (Code-First)
- PostgreSQL
- SignalR
- AutoMapper
- JWT Authentication

### Frontend Web
- Angular 21 (standalone components)
- SignalR Client
- WebRTC
- TypeScript
- SCSS

### Mobile
- .NET MAUI
- SignalR Client
- MVVM Toolkit
- Suporte para iOS, Android, Windows

## 🚀 Como Executar

### 1. Configurar o Banco de Dados PostgreSQL

#### Instalação Rápida:
```bash
# Windows: Baixe em https://www.postgresql.org/download/windows/
# macOS: brew install postgresql && brew services start postgresql
# Ubuntu: sudo apt install postgresql postgresql-contrib
```

#### Setup Automático:
```bash
# Conectar ao PostgreSQL
psql -U postgres

# Executar script de setup
\i database-setup.sql
```

*O script criará o banco `simpleconnect` e usuário dedicado.*

### 2. Executar o Backend

```bash
cd src/SimpleConnect.HttpApi
dotnet run
```

**O que acontece automaticamente:**
- ✅ Criação das tabelas (Code-First)
- ✅ Inserção de dados iniciais (seed)
- ✅ API disponível em `http://localhost:5000`
- ✅ SignalR Hub em `/hubs/communication`
- ✅ Swagger UI em `/swagger`

### 3. Executar o Frontend Angular

```bash
cd SimpleConnect.Web.Angular
npm install
ng serve
```

Frontend disponível em `http://localhost:4200`

### 4. Executar o Mobile MAUI

```bash
cd SimpleConnect.Mobile
dotnet build
# Abrir no Visual Studio ou usar dotnet run
```

## 📊 Dados Iniciais (Seed)

### Grupos Criados Automaticamente:
1. **Geral** - Chat para conversas informais
2. **Trabalho** - Discussões profissionais  
3. **Vídeo Conferência** - Sala para videochamadas

### Usuários de Exemplo:
- Usuário Demo (criador dos grupos)
- Maria Silva, João Santos, Ana Costa

### Mensagens de Demonstração:
- Mensagens de boas-vindhas
- Conversas de teste em cada grupo
- Mensagens de sistema para demonstração

## 🏗️ Estrutura do Banco de Dados

### Tabelas Principais:
```sql
ChatGroups                 -- Grupos de chat/videochamada
ChatGroupMembers          -- Membros dos grupos
ChatMessages              -- Mensagens trocadas
ChatMessageAttachments    -- Anexos das mensagens
```

### Índices Otimizados:
- `ChatGroups_CreatorId` - Busca por criador
- `ChatMessages_ChatGroupId_CreatedAt` - Paginação de mensagens
- `ChatGroupMembers_UserId` - Grupos do usuário

## 🔧 Configuração do Banco de Dados

### Connection String (appsettings.json):
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=SimpleConnect;Username=postgres;Password=sua_senha"
  }
}
```

### Para Produção:
- Altere usuário e senha
- Configure SSL/TLS
- Use connection pooling
- Considere read replicas para escalabilidade

## 📈 Performance e Escalabilidade

- **Índices otimizados** para queries de chat (GroupId + CreatedAt)
- **Paginação** eficiente no histórico de mensagens
- **SignalR** para comunicação em tempo real
- **WebRTC P2P** para reduzir carga do servidor
- **Limitação** de 10 participantes por chamada
- **Code-First** para versionamento do schema

## 🧪 Testes e Demonstração

### Endpoints Disponíveis:
- `GET /api/chat/groups` - Listar grupos
- `POST /api/chat/groups` - Criar grupo
- `GET /api/messages` - Histórico de mensagens
- `POST /api/messages` - Enviar mensagem
- `/hubs/communication` - SignalR Hub

### Dados para Teste:
Use os dados seed criados automaticamente:
- **Groups**: 3 grupos de exemplo
- **Users**: 4 usuários demo
- **Messages**: 8 mensagens de exemplo

## 🔐 Autenticação

JWT configurado com:
- **Issuer**: SimpleConnect
- **Audience**: SimpleConnectUsers
- **Key**: Configurável via appsettings

Para produção:
- Use chaves RSA fortes
- Configure refresh tokens
- Implemente revogação

## 📱 Mobile Setup

### Permissões Necessárias:
**Android (AndroidManifest.xml):**
```xml
<uses-permission android:name="android.permission.CAMERA" />
<uses-permission android:name="android.permission.RECORD_AUDIO" />
<uses-permission android:name="android.permission.INTERNET" />
```

**iOS (Info.plist):**
```xml
<key>NSCameraUsageDescription</key>
<string>SimpleConnect precisa acessar a câmera para videochamadas</string>
<key>NSMicrophoneUsageDescription</key>
<string>SimpleConnect precisa acessar o microfone para chamadas</string>
```

## 🌐 WebRTC Setup

### STUN/TURN Servers:
Configure servidores para WebRTC em redes restritas:
```javascript
// iceServers no frontend
const iceServers = [
    { urls: 'stun:stun.l.google.com:19302' },
    // Adicionar servidor TURN em produção
];
```

### Para Produção:
- Configure servidor TURN (coturn.com)
- Use certificados SSL válidos
- Implemente fallback para conexões

## 🚀 Próximos Passos para Produção

1. **Autenticação Completa**: Implementar Identity com registro via e-mail
2. **File Upload**: Implementar upload de imagens e arquivos
3. **Push Notifications**: Firebase/Apple Push para mobile
4. **TURN Server**: Configurar para WebRTC em redes corporativas
5. **Testing**: Unit tests e integration tests
6. **CI/CD**: GitHub Actions ou Azure DevOps
7. **Monitoring**: Application Insights ou similar
8. **Scaling**: Kubernetes ou Azure Container Instances

## 🐳 Docker (Opcional)

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/SimpleConnect.HttpApi/SimpleConnect.HttpApi.csproj", "src/SimpleConnect.HttpApi/"]
RUN dotnet restore "src/SimpleConnect.HttpApi/SimpleConnect.HttpApi.csproj"
COPY . .
WORKDIR "/src/src/SimpleConnect.HttpApi"
RUN dotnet build "SimpleConnect.HttpApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SimpleConnect.HttpApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SimpleConnect.HttpApi.dll"]
```

## 📋 Troubleshooting Comum

### Problema: "Database connection failed"
- Verifique se PostgreSQL está rodando
- Confirme connection string
- Teste com `psql -U postgres -h localhost`

### Problema: "SignalR connection failed"
- Verifique CORS configuration
- Confirme se o hub está mapeado corretamente
- Teste com Swagger UI

### Problema: "WebRTC not working"
- Verifique permissões de câmera/microfone
- Teste em ambiente HTTPS
- Configure STUN/TURN servers

## 📚 Documentação Adicional

- [DATABASE.md](DATABASE.md) - Guia completo do PostgreSQL
- [API Swagger](http://localhost:5000/swagger) - Documentação da API
- [SignalR Docs](https://docs.microsoft.com/aspnet/core/signalr)
- [WebRTC MDN](https://developer.mozilla.org/en-US/docs/Web/API/WebRTC_API)

---

## ✅ MVP Completo e Funcional!

O SimpleConnect está pronto para demonstração com:
- **Backend** API REST + SignalR funcionais
- **Frontend** Angular com chat e videochamada
- **Mobile** MAUI multiplataforma
- **Database** PostgreSQL com dados iniciais
- **WebRTC** Chamadas P2P funcionais

**Basta configurar o PostgreSQL e executar!** 🚀
