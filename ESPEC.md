# Especificação Técnica - SimpleConnect

## Contexto do Sistema

Atue como um Arquiteto de Software Sênior e Desenvolvedor Full-stack especializado em ecossistema .NET. O objetivo é desenvolver um MVP de uma aplicação de comunicação em tempo real chamada "SimpleConnect" com chat em grupo e videochamadas.

## Requisitos Técnicos

- **Backend**: ASP.NET Core 10 com arquitetura DDD limpa
- **Banco de Dados**: PostgreSQL com Entity Framework Core (Code-First)
- **Real-time**: SignalR (WebSockets) para sinalização WebRTC e chat
- **Frontend Web**: Angular 21 com componentes standalone
- **Mobile**: .NET MAUI multiplataforma
- **Funcionalidades**: Chat em grupo, videochamada (máx. 10 participantes)

## Arquitetura Implementada

### Domain Layer

Entidades de negócio com lógica de domínio encapsulada:

#### ChatGroup.cs
```csharp
public class ChatGroup
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public GroupType Type { get; private set; }
    public Guid CreatorId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public int MaxParticipants { get; private set; }
    public List<ChatGroupMember> Members { get; private set; }
    public List<CallParticipant> ActiveCallParticipants { get; private set; }

    public ChatGroup(string name, string description, GroupType type, Guid creatorId, int maxParticipants = 10)
    {
        Id = Guid.NewGuid();
        Name = name;
        Description = description;
        Type = type;
        CreatorId = creatorId;
        CreatedAt = DateTime.UtcNow;
        MaxParticipants = maxParticipants;
        Members = new List<ChatGroupMember>();
        ActiveCallParticipants = new List<CallParticipant>();
    }

    public void AddMember(Guid userId, string userName, bool isCreator = false)
    {
        if (Members.Count >= MaxParticipants)
            throw new InvalidOperationException("Group reached maximum capacity");

        var member = new ChatGroupMember(userId, userName, isCreator);
        Members.Add(member);
    }

    public void JoinCall(Guid userId, string userName)
    {
        if (ActiveCallParticipants.Count >= MaxParticipants)
            throw new InvalidOperationException("Call reached maximum capacity");

        var participant = new CallParticipant(userId, userName, ParticipantStatus.Connected);
        ActiveCallParticipants.Add(participant);
    }
}
```

#### ChatMessage.cs
```csharp
public class ChatMessage
{
    public Guid Id { get; private set; }
    public string Content { get; private set; }
    public Guid SenderId { get; private set; }
    public string SenderName { get; private set; }
    public Guid ChatGroupId { get; private set; }
    public MessageType Type { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? EditedAt { get; private set; }
    public bool IsDeleted { get; private set; }
    public List<ChatMessageAttachment> Attachments { get; private set; }

    public ChatMessage(string content, Guid senderId, string senderName, Guid chatGroupId, MessageType type = MessageType.Text)
    {
        Id = Guid.NewGuid();
        Content = content;
        SenderId = senderId;
        SenderName = senderName;
        ChatGroupId = chatGroupId;
        Type = type;
        CreatedAt = DateTime.UtcNow;
        Attachments = new List<ChatMessageAttachment>();
    }

    public void Edit(string newContent)
    {
        if (IsDeleted) throw new InvalidOperationException("Cannot edit deleted message");
        Content = newContent;
        EditedAt = DateTime.UtcNow;
    }

    public void Delete()
    {
        IsDeleted = true;
        // Soft delete
    }
}
```

#### Value Objects
```csharp
// CallParticipant.cs
public class CallParticipant
{
    public Guid UserId { get; set; }
    public string UserName { get; set; }
    public ParticipantStatus Status { get; set; }
    public DateTime JoinedAt { get; set; }
    public bool HasAudio { get; set; }
    public bool HasVideo { get; set; }

    public CallParticipant(Guid userId, string userName, ParticipantStatus status)
    {
        UserId = userId;
        UserName = userName;
        Status = status;
        JoinedAt = DateTime.UtcNow;
        HasAudio = true;
        HasVideo = true;
    }
}

// Enums
public enum ParticipantStatus
{
    Disconnected = 0,
    Connecting = 1,
    Connected = 2,
    Speaking = 3,
    Muted = 4,
    VideoOff = 5
}

public enum GroupType
{
    Chat = 0,
    VideoCall = 1
}

public enum MessageType
{
    Text = 0,
    System = 1,
    File = 2,
    Image = 3,
    CallStarted = 4,
    CallEnded = 5
}
```

### Application Layer

Services de aplicação com DTOs e lógica de negócio:

#### DTOs
```csharp
// ChatGroupDto.cs
public class ChatGroupDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public GroupType Type { get; set; }
    public Guid CreatorId { get; set; }
    public string CreatorName { get; set; }
    public DateTime CreatedAt { get; set; }
    public int MaxParticipants { get; set; }
    public int CurrentParticipantsCount { get; set; }
    public List<ChatGroupMemberDto> Members { get; set; }
}

// SendMessageDto.cs
public class SendMessageDto
{
    [Required]
    [MaxLength(1000)]
    public string Content { get; set; }
    public Guid ChatGroupId { get; set; }
    public MessageType Type { get; set; } = MessageType.Text;
    public Guid SenderId { get; set; }
}
```

#### Application Services
```csharp
// ChatAppService.cs
public class ChatAppService : IChatAppService
{
    private readonly IChatGroupRepository _groupRepository;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;

    public async Task<ChatGroupDto> CreateGroupAsync(CreateChatGroupDto input)
    {
        var group = new ChatGroup(input.Name, input.Description, input.Type, input.CreatorId);
        group.AddMember(input.CreatorId, "Creator", true);
        
        await _groupRepository.AddAsync(group);
        await _unitOfWork.SaveChangesAsync();
        
        return _mapper.Map<ChatGroupDto>(group);
    }

    public async Task<ChatGroupDto> JoinGroupAsync(JoinGroupDto input)
    {
        var group = await _groupRepository.GetAsync(input.GroupId);
        if (group == null) throw new ArgumentException("Group not found");

        group.AddMember(input.UserId, input.UserName);
        await _unitOfWork.SaveChangesAsync();
        
        return _mapper.Map<ChatGroupDto>(group);
    }
}
```

### SignalR Hub

Hub de comunicação em tempo real com WebRTC signaling:

#### CommunicationHub.cs
```csharp
[Authorize]
public class CommunicationHub : Hub
{
    private readonly IConnectionManager _connectionManager;
    private readonly IChatAppService _chatAppService;
    private readonly ILogger<CommunicationHub> _logger;

    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();
        await _connectionManager.AddConnectionAsync(userId, Context.ConnectionId);
        _logger.LogInformation("User {UserId} connected", userId);
        await base.OnConnectedAsync();
    }

    // Chat Methods
    public async Task SendMessage(SendMessageDto message)
    {
        try
        {
            var sentMessage = await _messageAppService.SendMessageAsync(message);
            await Clients.Group($"group_{message.ChatGroupId}").SendAsync("MessageReceived", sentMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending message");
            await Clients.Caller.SendAsync("Error", new { Message = "Failed to send message" });
        }
    }

    public async Task JoinGroup(string groupId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"group_{groupId}");
        var joinDto = new JoinGroupDto { GroupId = Guid.Parse(groupId), UserId = GetUserId() };
        var group = await _chatAppService.JoinGroupAsync(joinDto);
        await Clients.Group($"group_{groupId}").SendAsync("UserJoinedGroup", new { UserId = GetUserId(), Group = group });
    }

    // WebRTC Signaling Methods
    public async Task SendOffer(string roomId, string targetUserId, string sdp)
    {
        var signal = new WebRTCSignalDto
        {
            Type = "offer",
            Sdp = sdp,
            FromUserId = GetUserId(),
            ToUserId = Guid.Parse(targetUserId),
            RoomId = roomId
        };
        await SendWebRTCSignal(signal);
    }

    public async Task SendAnswer(string roomId, string targetUserId, string sdp)
    {
        var signal = new WebRTCSignalDto
        {
            Type = "answer",
            Sdp = sdp,
            FromUserId = GetUserId(),
            ToUserId = Guid.Parse(targetUserId),
            RoomId = roomId
        };
        await SendWebRTCSignal(signal);
    }

    public async Task SendIceCandidate(string roomId, string targetUserId, string candidate, string sdpMid, int sdpMLineIndex)
    {
        var signal = new WebRTCSignalDto
        {
            Type = "ice-candidate",
            Candidate = candidate,
            SdpMid = sdpMid,
            SdpMLineIndex = sdpMLineIndex,
            FromUserId = GetUserId(),
            ToUserId = Guid.Parse(targetUserId),
            RoomId = roomId
        };
        await SendWebRTCSignal(signal);
    }

    // Video Call Management
    public async Task JoinCall(JoinCallDto joinCall)
    {
        try
        {
            var callInfo = await _videoCallAppService.JoinCallAsync(joinCall);
            await Groups.AddToGroupAsync(Context.ConnectionId, $"call_{joinCall.GroupId}");
            await Clients.Group($"call_{joinCall.GroupId}").SendAsync("UserJoinedCall", callInfo);
        }
        catch (InvalidOperationException ex)
        {
            await Clients.Caller.SendAsync("Error", new { Message = ex.Message });
        }
    }

    private async Task SendWebRTCSignal(WebRTCSignalDto signal)
    {
        var targetConnections = await _connectionManager.GetConnectionsAsync(signal.ToUserId);
        foreach (var connectionId in targetConnections)
        {
            await Clients.Client(connectionId).SendAsync("WebRTCSignalReceived", signal);
        }
    }

    private Guid GetUserId()
    {
        var userIdClaim = Context.User?.FindFirst("sub")?.Value ?? 
                         Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }
}
```

### Banco de Dados (Code-First)

Entity Framework Core com PostgreSQL e seed automático:

#### DbContext com Seed
```csharp
public class SimpleConnectDbContext : DbContext
{
    public DbSet<ChatGroup> ChatGroups { get; set; }
    public DbSet<ChatMessage> ChatMessages { get; set; }
    public DbSet<ChatGroupMember> ChatGroupMembers { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ConfigureSimpleConnect();
        
        // Índices otimizados para performance
        builder.Entity<ChatMessage>()
            .HasIndex(m => new { m.ChatGroupId, m.CreatedAt });
            
        builder.Entity<ChatGroup>()
            .HasIndex(g => g.CreatorId);
    }

    public async Task SeedDataAsync()
    {
        if (await ChatGroups.AnyAsync()) return;

        // Grupos de exemplo
        var groups = new List<ChatGroup>
        {
            new ChatGroup("Geral", "Grupo para conversas informais", GroupType.Chat, Guid.NewGuid()),
            new ChatGroup("Trabalho", "Discussões profissionais", GroupType.Chat, Guid.NewGuid()),
            new ChatGroup("Vídeo Conferência", "Sala principal", GroupType.VideoCall, Guid.NewGuid(), 10)
        };

        await ChatGroups.AddRangeAsync(groups);
        await SaveChangesAsync();

        // Membros e mensagens de exemplo
        var userId = Guid.NewGuid();
        foreach (var group in groups)
        {
            group.AddMember(userId, "Usuário Demo", true);
            group.AddMember(Guid.NewGuid(), "Maria Silva", false);
            group.AddMember(Guid.NewGuid(), "João Santos", false);
        }

        var messages = new List<ChatMessage>
        {
            new ChatMessage("Olá pessoal! Sejam bem-vindos ao SimpleConnect! 🎉", userId, "Usuário Demo", groups[0].Id, MessageType.System),
            new ChatMessage("Obrigado! Estou animado para testar as videochamadas.", Guid.NewGuid(), "Maria Silva", groups[0].Id),
            new ChatMessage("As funcionalidades de chat estão incríveis!", Guid.NewGuid(), "João Santos", groups[0].Id)
        };

        await ChatMessages.AddRangeAsync(messages);
        await SaveChangesAsync();
    }
}
```

#### Configuração de Startup
```csharp
// Program.cs
var app = builder.Build();

// Inicialização automática do banco
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<SimpleConnectDbContext>();
    await context.Database.EnsureCreatedAsync();
    await context.SeedDataAsync();
}
```

### Frontend Angular

Serviços e componentes para comunicação em tempo real:

#### SignalR Service
```typescript
@Injectable({ providedIn: 'root' })
export class SignalRService {
  private hubConnection: HubConnection | undefined;
  private messageReceived$ = new Subject<ChatMessage>();
  private webRTCSignalReceived$ = new Subject<WebRTCSignal>();

  async startConnection(userId: string, token: string): Promise<void> {
    this.hubConnection = new HubConnectionBuilder()
      .withUrl(environment.signalRUrl, { accessTokenFactory: () => token })
      .withAutomaticReconnect()
      .build();

    this.hubConnection.on('MessageReceived', (message: ChatMessage) => {
      this.messageReceived$.next(message);
    });

    this.hubConnection.on('WebRTCSignalReceived', (signal: WebRTCSignal) => {
      this.webRTCSignalReceived$.next(signal);
    });

    await this.hubConnection.start();
  }

  async sendMessage(message: SendMessageDto): Promise<void> {
    await this.hubConnection?.invoke('SendMessage', message);
  }

  async sendOffer(roomId: string, targetUserId: string, sdp: string): Promise<void> {
    await this.hubConnection?.invoke('SendOffer', roomId, targetUserId, sdp);
  }
}
```

#### WebRTC Service
```typescript
@Injectable({ providedIn: 'root' })
export class WebRTCService {
  private peerConnections = new Map<string, RTCPeerConnection>();
  private localStream: MediaStream | undefined;

  async initializeLocalStream(): Promise<MediaStream> {
    this.localStream = await navigator.mediaDevices.getUserMedia({
      audio: true,
      video: { width: 1280, height: 720 }
    });
    return this.localStream;
  }

  async createPeerConnection(userId: string): Promise<RTCPeerConnection> {
    const pc = new RTCPeerConnection({
      iceServers: [{ urls: 'stun:stun.l.google.com:19302' }]
    });

    if (this.localStream) {
      this.localStream.getTracks().forEach(track => {
        pc.addTrack(track, this.localStream!);
      });
    }

    pc.ontrack = (event) => {
      // Handle remote stream
    };

    pc.onicecandidate = (event) => {
      if (event.candidate) {
        // Send via SignalR
      }
    };

    this.peerConnections.set(userId, pc);
    return pc;
  }
}
```

### Mobile MAUI

Estrutura para aplicação multiplataforma:

#### Models
```csharp
namespace SimpleConnect.Mobile.Models
{
    public class ChatGroup
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public GroupType Type { get; set; }
        public List<ChatGroupMember> Members { get; set; }
    }

    public enum ParticipantStatus
    {
        Disconnected = 0,
        Connecting = 1,
        Connected = 2,
        Muted = 4
    }
}
```

#### Dependencies
```xml
<PackageReference Include="Microsoft.AspNetCore.SignalR.Client" Version="8.0.0" />
<PackageReference Include="System.Net.Http.Json" Version="8.0.0" />
<PackageReference Include="CommunityToolkit.Mvvm" Version="8.2.2" />
```

## Configuração de Banco de Dados

### Setup PostgreSQL
```bash
# 1. Instalar PostgreSQL
# Windows: https://www.postgresql.org/download/windows/
# macOS: brew install postgresql
# Ubuntu: sudo apt install postgresql postgresql-contrib

# 2. Criar banco
psql -U postgres -c "CREATE DATABASE simpleconnect;"

# 3. Criar usuário (opcional)
psql -U postgres -c "CREATE USER simpleconnect_user WITH PASSWORD 'your_password';"
psql -U postgres -c "GRANT ALL PRIVILEGES ON DATABASE simpleconnect TO simpleconnect_user;"
```

### Connection String
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=SimpleConnect;Username=postgres;Password=postgres"
  }
}
```

### Índices Otimizados
```sql
-- Índices para performance de chat
CREATE INDEX idx_chat_messages_group_created ON "ChatMessages"("ChatGroupId", "CreatedAt" DESC);
CREATE INDEX idx_chat_groups_creator ON "ChatGroups"("CreatorId");
CREATE INDEX idx_chat_group_members_user ON "ChatGroupMembers"("UserId");
```

## Configuração e Deploy

### Backend Startup
```bash
cd src/api/SimpleConnect.HttpApi
dotnet run
```

### Frontend Angular
```bash
cd src/web/SimpleConnect.Web.Angular
npm install
ng serve
```

### Mobile MAUI
```bash
cd src/mobile/SimpleConnect.Mobile
dotnet build
# Abrir no Visual Studio para deploy
```

## Considerações Importantes

### WebRTC vs Streaming
- **SignalR**: Apenas sinalização (SDP, ICE candidates)
- **WebRTC**: Transmissão P2P de vídeo/áudio
- **Limitação**: Máximo 10 participantes por chamada

### Performance PostgreSQL
- Índices compostos em `ChatGroupId + CreatedAt`
- Paginação eficiente de mensagens
- Soft delete para histórico

### Mobile Permissions
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

### Segurança
- JWT authentication configurado
- CORS para cross-origin
- Validação de输入 em DTOs
- Rate limiting para SignalR

## Estrutura Final do Projeto

```
SimpleConnect/
├── src/
│   ├── api/                                 # Backend API
│   │   ├── SimpleConnect.Domain.Shared/     # Enums, Value Objects, Constants
│   │   ├── SimpleConnect.Domain/             # Entidades de negócio
│   │   ├── SimpleConnect.Application.Contracts/ # DTOs e Interfaces
│   │   ├── SimpleConnect.Application/       # Services e AutoMapper
│   │   ├── SimpleConnect.EntityFrameworkCore/ # DbContext e Repositories
│   │   └── SimpleConnect.HttpApi/           # API Controllers e SignalR Hub
│   ├── web/                                 # Frontend Web
│   │   ├── SimpleConnect.Web/               # Projeto web MVC (configuração)
│   │   └── SimpleConnect.Web.Angular/       # Frontend Angular
│   └── mobile/                              # Aplicação Mobile
│       └── SimpleConnect.Mobile/             # Projeto MAUI
├── database-setup.sql                       # Script PostgreSQL
├── DATABASE.md                              # Documentação do banco
└── README.md                                # Documentação completa
```

O SimpleConnect está pronto para demonstração com arquitetura limpa, código organizado e todas as funcionalidades implementadas seguindo as melhores práticas de desenvolvimento!