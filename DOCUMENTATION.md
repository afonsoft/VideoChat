# FamilyMeet - Documentacao do Sistema

## Indice

1. [Visao Geral](#visao-geral)
2. [Arquitetura do Sistema](#arquitetura-do-sistema)
3. [Stack Tecnologica](#stack-tecnologica)
4. [Estrutura do Projeto](#estrutura-do-projeto)
5. [API (Backend)](#api-backend)
6. [AdminWeb (Portal Administrativo)](#adminweb-portal-administrativo)
7. [ClientWeb (Aplicacao Cliente)](#clientweb-aplicacao-cliente)
8. [Mobile (MAUI)](#mobile-maui)
9. [Comunicacao em Tempo Real](#comunicacao-em-tempo-real)
10. [Video Chat (WebRTC)](#video-chat-webrtc)
11. [Configuracao e Instalacao](#configuracao-e-instalacao)
12. [Testes](#testes)
13. [Deploy](#deploy)
14. [Seguranca](#seguranca)

---

## Visao Geral

O **FamilyMeet** e uma plataforma de comunicacao em tempo real projetada para mensagens em grupo e videoconferencia multi-participante. O sistema utiliza uma arquitetura DDD (Domain-Driven Design) com backend .NET e frontends Angular e MAUI para acesso administrativo e de usuario final.

### Funcionalidades Principais

- **Chat em Grupo**: Mensagens em tempo real com suporte a texto, imagens, arquivos e mensagens de sistema
- **Video Chat**: Chamadas de video P2P e em grupo usando WebRTC com sinalizacao via SignalR
- **Compartilhamento de Tela**: Compartilhamento de tela durante chamadas de video
- **Gerenciamento de Grupos**: Criacao, edicao e gerenciamento de grupos de chat
- **Gerenciamento de Participantes**: Controle de membros, mute, ban e status online
- **Painel Administrativo**: Gerenciamento de usuarios, tenants, logs de auditoria
- **Multi-tenancy**: Suporte a multiplos tenants via ABP Framework
- **Autenticacao**: Login por email/senha e OAuth (Google)

---

## Arquitetura do Sistema

```
+-------------------+     +-------------------+     +-------------------+
|    ClientWeb      |     |    AdminWeb       |     |    Mobile (MAUI)  |
|   (Angular 21)    |     |   (Angular 21)    |     |    (.NET MAUI)    |
+--------+----------+     +--------+----------+     +--------+----------+
         |                         |                         |
         |    WebSocket/HTTP       |    HTTP/REST            |
         +------------+------------+-------------------------+
                      |
         +------------+------------+
         |      API (.NET 10)      |
         |   +------------------+  |
         |   |   SignalR Hubs   |  |
         |   |  - ChatHub       |  |
         |   |  - VideoHub      |  |
         |   +------------------+  |
         |   +------------------+  |
         |   |  ABP Framework   |  |
         |   |  - Identity      |  |
         |   |  - Permissions   |  |
         |   |  - Multi-tenant  |  |
         |   +------------------+  |
         +--------+----+----------+
                  |    |
         +--------+    +--------+
         |                      |
    +----+-----+          +-----+----+
    |PostgreSQL|          |  Redis   |
    | (Dados)  |          | (Cache/  |
    |          |          | Backplane|
    +----------+          +----------+
```

### Camadas da Arquitetura (DDD)

1. **Domain Layer** (`afonsoft.FamilyMeet.Domain`): Entidades, agregados, repositorios e servicos de dominio
2. **Application Layer** (`afonsoft.FamilyMeet.Application`): Servicos de aplicacao, DTOs, SignalR Hubs
3. **Infrastructure Layer** (`afonsoft.FamilyMeet.EntityFrameworkCore`): Contexto EF Core, migracoes, implementacao de repositorios
4. **HTTP API Layer** (`afonsoft.FamilyMeet.HttpApi`): Controllers REST, filtros e middleware
5. **Host Layer** (`afonsoft.FamilyMeet.HttpApi.Host`): Configuracao da aplicacao, startup

---

## Stack Tecnologica

### Backend
| Tecnologia | Versao | Uso |
|---|---|---|
| .NET | 10.0 | Runtime da aplicacao |
| ASP.NET Core | 10.0 | Framework web |
| ABP Framework | Latest | Framework DDD e modular |
| SignalR | 10.0 | Comunicacao em tempo real (WebSocket) |
| Entity Framework Core | 10.0 | ORM para acesso a dados |
| PostgreSQL | 16+ | Banco de dados relacional |
| Redis | 7+ | Cache distribuido e backplane SignalR |
| Serilog | Latest | Logging estruturado |
| AutoMapper | 15.1.3 | Mapeamento objeto-objeto |

### Frontend (AdminWeb)
| Tecnologia | Versao | Uso |
|---|---|---|
| Angular | 21.0.0 | Framework SPA |
| ABP Angular | Latest | Modulos ABP para Angular |
| ng-zorro-antd | Latest | Biblioteca de componentes UI |
| Lepton X | Latest | Tema administrativo |
| Vitest | 4.0.0 | Framework de testes |

### Frontend (ClientWeb)
| Tecnologia | Versao | Uso |
|---|---|---|
| Angular | 21.2.0 | Framework SPA |
| RxJS | 7.8.0 | Programacao reativa |
| @microsoft/signalr | Latest | Cliente SignalR |
| WebRTC API | Nativo | Comunicacao P2P de video/audio |
| Vitest | 4.0.8 | Framework de testes |

### Mobile
| Tecnologia | Versao | Uso |
|---|---|---|
| .NET MAUI | 10.0 | Framework cross-platform mobile |

### DevOps
| Tecnologia | Uso |
|---|---|
| Docker | Containerizacao |
| Docker Compose | Orquestracao local |
| GitHub Actions | CI/CD |

---

## Estrutura do Projeto

```
VideoChat/
├── src/
│   ├── api/                              # Backend .NET
│   │   ├── src/
│   │   │   ├── afonsoft.FamilyMeet.Domain/           # Entidades e logica de dominio
│   │   │   ├── afonsoft.FamilyMeet.Domain.Shared/    # Constantes e enums compartilhados
│   │   │   ├── afonsoft.FamilyMeet.Application/      # Servicos de aplicacao e Hubs
│   │   │   ├── afonsoft.FamilyMeet.Application.Contracts/ # DTOs e interfaces
│   │   │   ├── afonsoft.FamilyMeet.EntityFrameworkCore/   # EF Core e repositorios
│   │   │   ├── afonsoft.FamilyMeet.HttpApi/           # Controllers REST
│   │   │   └── afonsoft.FamilyMeet.HttpApi.Host/      # Host da aplicacao
│   │   └── test/
│   │       ├── afonsoft.FamilyMeet.Application.Tests/ # Testes de servicos
│   │       ├── afonsoft.FamilyMeet.Domain.Tests/      # Testes de dominio
│   │       ├── afonsoft.FamilyMeet.EntityFrameworkCore.Tests/ # Testes EF Core
│   │       ├── afonsoft.FamilyMeet.HttpApi.Tests/     # Testes de controllers
│   │       └── afonsoft.FamilyMeet.TestBase/          # Base comum para testes
│   ├── adminWeb/                         # Portal administrativo Angular
│   │   ├── src/app/
│   │   │   ├── home/                     # Pagina inicial
│   │   │   ├── audit-logs/               # Logs de auditoria
│   │   │   ├── proxy/                    # Proxies gerados pelo ABP
│   │   │   └── route.provider.ts         # Configuracao de rotas
│   │   └── package.json
│   ├── clientWeb/                        # Aplicacao cliente Angular
│   │   ├── src/app/
│   │   │   ├── components/
│   │   │   │   ├── login/                # Componente de login
│   │   │   │   ├── chat/                 # Componente de chat
│   │   │   │   └── video-call/           # Componente de video chamada
│   │   │   ├── services/
│   │   │   │   ├── auth.service.ts       # Servico de autenticacao
│   │   │   │   ├── chat.service.ts       # Servico de chat
│   │   │   │   ├── signalr.service.ts    # Servico SignalR
│   │   │   │   └── webrtc.service.ts     # Servico WebRTC
│   │   │   ├── guards/                   # Guards de autenticacao
│   │   │   └── app.routes.ts             # Configuracao de rotas
│   │   └── package.json
│   └── mobile/                           # Aplicacao MAUI
│       └── FamilyMeet.Maui/
├── docker-compose.yml                    # Orquestracao Docker
├── start.sh                              # Script de inicio (Linux/Mac)
├── start.ps1                             # Script de inicio (Windows)
└── DOCUMENTATION.md                      # Esta documentacao
```

---

## API (Backend)

### Endpoints REST

#### Chat Groups
| Metodo | Endpoint | Descricao |
|---|---|---|
| GET | `/api/app/chat-group` | Listar grupos (paginado) |
| GET | `/api/app/chat-group/{id}` | Obter grupo por ID |
| POST | `/api/app/chat-group` | Criar novo grupo |
| PUT | `/api/app/chat-group/{id}` | Atualizar grupo |
| DELETE | `/api/app/chat-group/{id}` | Excluir grupo |
| POST | `/api/app/chat-group/{id}/activate` | Ativar grupo |
| POST | `/api/app/chat-group/{id}/deactivate` | Desativar grupo |
| GET | `/api/app/chat-group/my-groups` | Listar meus grupos |
| GET | `/api/app/chat-group/{id}/participants` | Listar participantes |
| POST | `/api/app/chat-group/{id}/join` | Entrar no grupo |
| POST | `/api/app/chat-group/{id}/leave` | Sair do grupo |

#### Chat Messages
| Metodo | Endpoint | Descricao |
|---|---|---|
| GET | `/api/app/chat-message` | Listar mensagens (paginado) |
| GET | `/api/app/chat-message/{id}` | Obter mensagem por ID |
| POST | `/api/app/chat-message` | Criar mensagem |
| PUT | `/api/app/chat-message/{id}` | Editar mensagem |
| DELETE | `/api/app/chat-message/{id}` | Excluir mensagem (soft delete) |
| GET | `/api/app/chat-message/group/{groupId}` | Mensagens de um grupo |
| POST | `/api/app/chat-message/send` | Enviar mensagem |

#### Chat Participants
| Metodo | Endpoint | Descricao |
|---|---|---|
| GET | `/api/app/chat-participant/{groupId}` | Listar participantes |
| GET | `/api/app/chat-participant/{id}` | Obter participante |
| POST | `/api/app/chat-participant` | Adicionar participante |
| PUT | `/api/app/chat-participant/{id}` | Atualizar participante |
| DELETE | `/api/app/chat-participant/{id}` | Remover participante |
| POST | `/api/app/chat-participant/{id}/mute` | Silenciar participante |
| POST | `/api/app/chat-participant/{id}/unmute` | Remover silencio |
| POST | `/api/app/chat-participant/{id}/ban` | Banir participante |
| POST | `/api/app/chat-participant/{id}/unban` | Remover banimento |

### DTOs Principais

#### CreateChatGroupDto
```json
{
  "name": "string (obrigatorio, max 128)",
  "description": "string (max 500)",
  "isPublic": "boolean",
  "maxParticipants": "number (min 1)"
}
```

#### CreateChatMessageDto
```json
{
  "chatGroupId": "guid (obrigatorio)",
  "content": "string (obrigatorio, max 4000)",
  "type": "Text | Image | File | System",
  "replyToMessageId": "guid? (opcional)"
}
```

---

## AdminWeb (Portal Administrativo)

### Funcionalidades

1. **Dashboard**: Visao geral do sistema com metricas
2. **Gerenciamento de Identidade**: Usuarios, roles e permissoes (via ABP)
3. **Gerenciamento de Tenants**: Multi-tenancy (via ABP)
4. **Logs de Auditoria**: Visualizacao e busca de logs do sistema
5. **Configuracoes**: Gerenciamento de configuracoes do sistema

### Estrutura de Componentes

```
adminWeb/src/app/
├── home/
│   ├── home.component.ts          # Pagina inicial com verificacao de autenticacao
│   └── home.component.spec.ts     # Testes unitarios
├── audit-logs/
│   ├── audit-logs.component.ts    # Componente de logs de auditoria
│   └── audit-logs.component.spec.ts # Testes unitarios
└── route.provider.ts              # Registro de rotas ABP
```

### Acesso

- **URL**: `http://localhost:4201`
- **Autenticacao**: OpenID Connect via ABP Identity Server

---

## ClientWeb (Aplicacao Cliente)

### Funcionalidades

1. **Autenticacao**: Login por email/senha, Google OAuth, registro
2. **Chat em Grupo**: Envio/recebimento de mensagens em tempo real
3. **Video Chat**: Chamadas de video P2P e em grupo
4. **Compartilhamento de Tela**: Compartilhar tela durante video chat
5. **Gerenciamento de Grupos**: Criar, listar e selecionar grupos

### Rotas

| Rota | Componente | Guard | Descricao |
|---|---|---|---|
| `/login` | LoginComponent | - | Pagina de login |
| `/chat` | ChatComponent | AuthGuard | Chat e lista de grupos |
| `/video-call` | VideoCallComponent | AuthGuard | Chamada de video |

### Servicos

#### AuthService
Gerencia autenticacao e sessao do usuario.
- `login(email, password)`: Login com credenciais
- `loginWithGoogle()`: Login via Google OAuth
- `register(userData)`: Registro de novo usuario
- `logout()`: Logout e limpeza de sessao
- `refreshToken()`: Renovacao de token JWT
- `currentUser`: Usuario logado atual
- `isLoggedIn`: Estado de autenticacao

#### ChatService
Gerencia operacoes de chat via HTTP.
- `getUserGroups(userId)`: Listar grupos do usuario
- `getMessages(groupId)`: Obter mensagens de um grupo
- `sendMessage(message)`: Enviar mensagem
- `createGroup(groupData)`: Criar novo grupo
- `joinGroup(groupId)`: Entrar em um grupo
- `onMessage()`: Observable de novas mensagens

#### SignalRService
Gerencia conexoes SignalR em tempo real.
- **Chat Hub** (`/chat-hub`):
  - `sendMessage(groupId, content)`: Enviar mensagem via WebSocket
  - `joinGroup(groupId)`: Entrar em grupo
  - `leaveGroup(groupId)`: Sair de grupo
  - `setTyping(groupId, isTyping)`: Indicador de digitacao
  - `editMessage(messageId, newContent)`: Editar mensagem
  - `deleteMessage(messageId)`: Excluir mensagem
  - `updateOnlineStatus(isOnline)`: Atualizar status online
- **Video Hub** (`/video-hub`):
  - `startVideoCall(groupId, targetUserId)`: Iniciar chamada de video
  - `acceptVideoCall(callId)`: Aceitar chamada
  - `rejectVideoCall(callId, reason)`: Rejeitar chamada
  - `endVideoCall(callId)`: Encerrar chamada
  - `sendWebRTCOffer(callId, offer)`: Enviar oferta WebRTC
  - `sendWebRTCAnswer(callId, answer)`: Enviar resposta WebRTC
  - `sendWebRTCIceCandidate(callId, candidate)`: Enviar candidato ICE
  - `startScreenShare(callId)`: Iniciar compartilhamento de tela
  - `stopScreenShare(callId)`: Parar compartilhamento de tela

#### WebRTCService
Gerencia conexoes WebRTC peer-to-peer.
- `initializeLocalStream(videoEnabled, audioEnabled)`: Inicializar midia local
- `createPeerConnection(userId, userName)`: Criar conexao peer
- `createOffer(userId)`: Criar oferta SDP
- `handleOffer(userId, userName, offer)`: Processar oferta recebida
- `handleAnswer(userId, answer)`: Processar resposta recebida
- `handleIceCandidate(userId, candidate)`: Adicionar candidato ICE
- `toggleMute()`: Alternar mudo
- `toggleCamera()`: Alternar camera
- `startScreenShare()`: Iniciar compartilhamento de tela
- `stopScreenShare()`: Parar compartilhamento de tela
- `startCall(callId, groupId, isInitiator)`: Iniciar chamada
- `endCall()`: Encerrar chamada e limpar recursos
- `removePeer(userId)`: Remover peer especifico

### Acesso

- **URL**: `http://localhost:4200`
- **Autenticacao**: Email/senha ou Google OAuth

---

## Mobile (MAUI)

Aplicacao cross-platform usando .NET MAUI para iOS e Android.

- **Projeto**: `src/mobile/FamilyMeet.Maui/`
- **Framework**: .NET MAUI 10.0
- **Plataformas**: iOS, Android

---

## Comunicacao em Tempo Real

### SignalR Hubs

#### ChatHub (`/chat-hub`)

**Eventos do Servidor para Cliente:**
| Evento | Dados | Descricao |
|---|---|---|
| `ReceiveMessage` | `ChatMessageDto` | Nova mensagem recebida |
| `MessageEdited` | `{ messageId, newContent, editedAt }` | Mensagem editada |
| `MessageDeleted` | `{ messageId, deletedAt }` | Mensagem excluida |
| `UserJoinedGroup` | `{ userId, userName, groupId }` | Usuario entrou no grupo |
| `UserLeftGroup` | `{ userId, userName, groupId }` | Usuario saiu do grupo |
| `UserTyping` | `{ userId, userName, groupId, isTyping }` | Indicador de digitacao |
| `OnlineStatusChanged` | `{ userId, isOnline }` | Status online alterado |
| `VideoCallRequested` | `{ callId, callerId, callerName, groupId }` | Solicitacao de video |

**Metodos do Cliente para Servidor:**
| Metodo | Parametros | Descricao |
|---|---|---|
| `SendMessageAsync` | `groupId, content, type` | Enviar mensagem |
| `JoinGroupAsync` | `groupId` | Entrar no grupo |
| `LeaveGroupAsync` | `groupId` | Sair do grupo |
| `UserTypingAsync` | `groupId, isTyping` | Status de digitacao |
| `EditMessageAsync` | `messageId, newContent` | Editar mensagem |
| `DeleteMessageAsync` | `messageId` | Excluir mensagem |
| `UpdateOnlineStatusAsync` | `isOnline` | Atualizar status |
| `StartVideoCallAsync` | `groupId, targetUserId` | Iniciar video chamada |

#### VideoHub (`/video-hub`)

**Eventos do Servidor para Cliente:**
| Evento | Dados | Descricao |
|---|---|---|
| `IncomingVideoCall` | `{ callId, callerId, callerName, groupId }` | Chamada recebida |
| `VideoCallAccepted` | `{ callId, acceptedBy }` | Chamada aceita |
| `VideoCallRejected` | `{ callId, reason }` | Chamada rejeitada |
| `VideoCallEnded` | `{ callId, endedBy }` | Chamada encerrada |
| `VideoCallError` | `{ callId, error }` | Erro na chamada |
| `StartWebRTCExchange` | `{ callId, peerUserId, isInitiator }` | Iniciar troca WebRTC |
| `WebRTCOffer` | `{ callId, fromUserId, offer }` | Oferta SDP recebida |
| `WebRTCAnswer` | `{ callId, fromUserId, answer }` | Resposta SDP recebida |
| `WebRTCIceCandidate` | `{ callId, fromUserId, candidate }` | Candidato ICE recebido |
| `ScreenShareStarted` | `{ callId, userId }` | Compartilhamento iniciado |
| `ScreenShareStopped` | `{ callId, userId }` | Compartilhamento parado |

**Metodos do Cliente para Servidor:**
| Metodo | Parametros | Descricao |
|---|---|---|
| `StartVideoCallAsync` | `groupId, targetUserId` | Iniciar chamada |
| `AcceptVideoCallAsync` | `callId` | Aceitar chamada |
| `RejectVideoCallAsync` | `callId, reason` | Rejeitar chamada |
| `EndVideoCallAsync` | `callId` | Encerrar chamada |
| `SendWebRTCOfferAsync` | `callId, offer` | Enviar oferta SDP |
| `SendWebRTCAnswerAsync` | `callId, answer` | Enviar resposta SDP |
| `SendWebRTCIceCandidateAsync` | `callId, candidate` | Enviar candidato ICE |
| `StartScreenShareAsync` | `callId` | Iniciar screen share |
| `StopScreenShareAsync` | `callId` | Parar screen share |

---

## Video Chat (WebRTC)

### Fluxo de Sinalizacao

```
Chamador (A)                    Servidor SignalR                  Receptor (B)
     |                              |                                |
     |-- StartVideoCall ----------->|                                |
     |                              |-- IncomingVideoCall ---------->|
     |                              |                                |
     |                              |<--------- AcceptVideoCall -----|
     |<-- StartWebRTCExchange ------|-- StartWebRTCExchange -------->|
     |                              |                                |
     |== Troca WebRTC (P2P) =======|================================|
     |                              |                                |
     |-- createOffer()              |                                |
     |-- setLocalDescription()      |                                |
     |-- SendWebRTCOffer ---------->|                                |
     |                              |-- WebRTCOffer --------------->|
     |                              |                   handleOffer()|
     |                              |            setRemoteDescription|
     |                              |                  createAnswer()|
     |                              |            setLocalDescription |
     |                              |<--------- SendWebRTCAnswer ----|
     |<-- WebRTCAnswer -------------|                                |
     |-- setRemoteDescription()     |                                |
     |                              |                                |
     |-- ICE Candidate ------------>|-- ICE Candidate ------------->|
     |<-- ICE Candidate ------------|<-- ICE Candidate -------------|
     |                              |                                |
     |============ Conexao P2P Estabelecida (Audio/Video) ==========|
```

### Configuracao WebRTC

```typescript
const rtcConfig: RTCConfiguration = {
  iceServers: [
    { urls: 'stun:stun.l.google.com:19302' },
    { urls: 'stun:stun1.l.google.com:19302' },
    { urls: 'stun:stun2.l.google.com:19302' },
    { urls: 'stun:stun3.l.google.com:19302' }
  ],
  iceCandidatePoolSize: 10
};
```

### Controles de Video

- **Mudo/Desmudo**: Alterna faixas de audio do stream local
- **Camera On/Off**: Alterna faixas de video do stream local
- **Compartilhar Tela**: Substitui a faixa de video pela captura de tela
- **Encerrar Chamada**: Fecha todas as conexoes peer e limpa recursos

---

## Configuracao e Instalacao

### Pre-requisitos

- **Node.js** 22+ (para Angular apps)
- **.NET SDK** 10.0+
- **PostgreSQL** 16+
- **Redis** 7+
- **Docker** e **Docker Compose** (opcional, para deploy)

### Instalacao Local

#### 1. Clonar o Repositorio
```bash
git clone https://github.com/afonsoft/VideoChat.git
cd VideoChat
```

#### 2. Configurar o Backend (API)
```bash
cd src/api/src/afonsoft.FamilyMeet.HttpApi.Host

# Configurar connection string no appsettings.json
# PostgreSQL: "Default": "Host=localhost;Port=5432;Database=FamilyMeet;Username=postgres;Password=..."
# Redis: "Redis": { "Configuration": "localhost:6379" }

# Executar migracoes
dotnet ef database update

# Iniciar a API
dotnet run
# API disponivel em: http://localhost:5000
```

#### 3. Configurar o AdminWeb
```bash
cd src/adminWeb
npm install
npm start
# AdminWeb disponivel em: http://localhost:4201
```

#### 4. Configurar o ClientWeb
```bash
cd src/clientWeb
npm install
npm start
# ClientWeb disponivel em: http://localhost:4200
```

#### 5. Usando Docker Compose
```bash
# Na raiz do projeto
docker-compose up -d
```

#### 6. Usando Scripts de Inicio
```bash
# Linux/Mac
./start.sh

# Windows
.\start.ps1
```

### Variaveis de Ambiente

| Variavel | Descricao | Padrao |
|---|---|---|
| `ASPNETCORE_ENVIRONMENT` | Ambiente da API | `Development` |
| `ConnectionStrings__Default` | String de conexao PostgreSQL | - |
| `Redis__Configuration` | Configuracao Redis | `localhost:6379` |
| `App__CorsOrigins` | Origens CORS permitidas | `http://localhost:4200,http://localhost:4201` |

---

## Testes

### Testes do Backend (API)

```bash
cd src/api

# Executar todos os testes
dotnet test

# Executar testes especificos
dotnet test test/afonsoft.FamilyMeet.Application.Tests/
dotnet test test/afonsoft.FamilyMeet.Domain.Tests/
dotnet test test/afonsoft.FamilyMeet.EntityFrameworkCore.Tests/
dotnet test test/afonsoft.FamilyMeet.HttpApi.Tests/
```

#### Cobertura de Testes do Backend
- **ChatGroupAppServiceTests**: CRUD de grupos, validacao, ativacao/desativacao
- **ChatMessageAppServiceTests**: CRUD de mensagens, tipos, respostas, soft delete
- **ChatParticipantAppServiceTests**: CRUD de participantes, mute, ban, status online
- **ChatGroupControllerTests**: Testes de controllers HTTP
- **IdentityAppServiceTests**: Testes de identidade
- **TenantAppServiceTests**: Testes de multi-tenancy
- **SettingAppServiceTests**: Testes de configuracoes
- **FeatureAppServiceTests**: Testes de features
- **VideoChatHubTests**: Testes do hub de video chat

### Testes do AdminWeb

```bash
cd src/adminWeb

# Executar testes
npm test

# Executar testes com cobertura
npm run test -- --coverage
```

#### Cobertura de Testes do AdminWeb
- **HomeComponent**: Renderizacao, estado de autenticacao
- **AuditLogsComponent**: Formulario de busca, paginacao, reset
- **DashboardComponent**: Metricas e graficos
- **ChatManagementComponent**: Gerenciamento de grupos de chat

### Testes do ClientWeb

```bash
cd src/clientWeb

# Executar testes
npm test

# Executar testes com cobertura
npm run test -- --coverage
```

#### Cobertura de Testes do ClientWeb
- **LoginComponent**: Formulario, validacao, login Google
- **ChatComponent**: Mensagens, grupos, video call
- **VideoCallComponent**: UI de chamada, controles, modais
- **AuthService**: Login, logout, token, sessao
- **ChatService**: Mensagens HTTP
- **SignalRService**: Conexoes hub, eventos, metodos
- **WebRTCService**: Peer connections, streams, ICE

---

## Deploy

### Docker Compose (Producao)

```yaml
# docker-compose.yml
services:
  api:
    build: ./src/api
    ports:
      - "5000:5000"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__Default=Host=db;...
    depends_on:
      - db
      - redis

  web:
    build: ./src/clientWeb
    ports:
      - "4200:80"

  admin:
    build: ./src/adminWeb
    ports:
      - "4201:80"

  db:
    image: postgres:16
    volumes:
      - pgdata:/var/lib/postgresql/data

  redis:
    image: redis:7-alpine
    ports:
      - "6379:6379"
```

### Nginx como Reverse Proxy

Para producao, recomenda-se usar Nginx como reverse proxy com suporte a WebSocket:

```nginx
server {
    listen 80;
    server_name familymeet.example.com;

    location / {
        proxy_pass http://localhost:5000;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }

    location /chat-hub {
        proxy_pass http://localhost:5000/chat-hub;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection "upgrade";
    }

    location /video-hub {
        proxy_pass http://localhost:5000/video-hub;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection "upgrade";
    }
}
```

---

## Seguranca

### Autenticacao
- JWT (JSON Web Tokens) para autenticacao da API
- OpenID Connect para o portal administrativo
- OAuth 2.0 (Google) para login social

### Autorizacao
- ABP Permission System para controle de acesso baseado em roles
- Guards Angular para protecao de rotas no frontend

### Boas Praticas
- Todas as conexoes em producao devem usar HTTPS/WSS
- Tokens JWT possuem tempo de expiracao configuravel
- Senhas armazenadas com hashing seguro (Identity Framework)
- CORS configurado para origens especificas
- Headers de seguranca (X-Frame-Options, CSP, etc.)
- Soft delete para preservacao de historico de mensagens
- Audit logging para rastreamento de acoes

### WebRTC
- Comunicacao P2P e criptografada por padrao (SRTP/DTLS)
- Servidores STUN publicos para NAT traversal
- Para ambientes restritos, considerar servidores TURN dedicados

---

## Funcionalidades Adicionais Implementadas

### Compartilhamento de Tela
- Integrado no componente de video chamada
- Substitui a faixa de video pela captura de tela
- Notifica outros participantes via SignalR
- Restaura a camera ao parar o compartilhamento

### Sistema de Notificacoes
- Notificacao de chamada recebida com modal interativo
- Indicadores de digitacao em tempo real
- Status online/offline dos participantes
- Notificacao de entrada/saida de membros no grupo

### Gerenciamento de Participantes
- Lista de participantes em tempo real durante chamadas
- Controles de mudo e ban para administradores
- Indicadores visuais de status (online, mudo, banido)
- Contagem de participantes em chamadas ativas
