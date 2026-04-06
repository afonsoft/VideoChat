# SignalR & WebRTC Implementation Guide

## 🚀 SignalR Hub Configuration

### ChatHub (`/chat-hub`)
Hub principal para mensagens de chat em tempo real e gerenciamento de grupos.

#### Funcionalidades
- ✅ **Mensagens em tempo real** - Envio e recebimento instantâneos
- ✅ **Gerenciamento de grupos** - Entrar/sair de grupos de chat
- ✅ **Status de usuários** - Online/offline, typing indicators
- ✅ **Edição e exclusão** - Soft delete de mensagens
- ✅ **Video call signaling** - Chamadas de vídeo P2P

#### Eventos SignalR

**Client → Server:**
```javascript
// Enviar mensagem
connection.invoke("SendMessageAsync", {
    chatGroupId: "guid",
    content: "Hello World",
    type: 0 // Text
});

// Entrar em grupo
connection.invoke("JoinGroupAsync", groupId);

// Indicar que está digitando
connection.invoke("UserTypingAsync", groupId, true);

// Iniciar chamada de vídeo
connection.invoke("StartVideoCallAsync", groupId, targetUserId);
```

**Server → Client:**
```javascript
// Receber mensagem
connection.on("ReceiveMessage", (data) => {
    console.log("Nova mensagem:", data.message);
});

// Usuário entrou no grupo
connection.on("UserJoined", (data) => {
    console.log(`${data.userName} entrou no grupo`);
});

// Requisição de chamada de vídeo
connection.on("VideoCallRequest", (data) => {
    console.log("Chamada recebida de:", data.callerName);
});
```

### VideoHub (`/video-hub`)
Hub especializado para streaming de vídeo P2P com WebRTC.

#### Funcionalidades
- ✅ **Video calls P2P** - Chamadas ponto a ponto
- ✅ **WebRTC signaling** - Troca de offers/answers/ICE candidates
- ✅ **Screen sharing** - Compartilhamento de tela
- ✅ **Call management** - Iniciar, aceitar, rejeitar, encerrar chamadas

#### Eventos SignalR

**Client → Server:**
```javascript
// Iniciar chamada
connection.invoke("StartVideoCallAsync", groupId, targetUserId);

// Aceitar chamada
connection.invoke("AcceptVideoCallAsync", callId);

// Enviar WebRTC offer
connection.invoke("SendWebRTCOfferAsync", callId, offer);

// Enviar ICE candidate
connection.invoke("SendWebRTCIceCandidateAsync", callId, candidate);
```

**Server → Client:**
```javascript
// Chamada recebida
connection.on("IncomingVideoCall", (data) => {
    console.log("Chamada de:", data.callerName);
});

// WebRTC offer recebida
connection.on("WebRTCOffer", (data) => {
    await peerConnection.setRemoteDescription(data.offer);
    const answer = await peerConnection.createAnswer();
    connection.invoke("SendWebRTCAnswerAsync", data.callId, answer);
});
```

## 🎥 WebRTC P2P Implementation

### Arquitetura P2P

```
Client A ←→ SignalR Hub ←→ Client B
    ↕            ↕           ↕
WebRTC ←→ Signaling ←→ WebRTC
```

### Flow de Chamada de Vídeo

1. **Initiation**: Cliente A inicia chamada via SignalR
2. **Signaling**: Troca de WebRTC offers/answers
3. **Connection**: Estabelecimento de conexão P2P
4. **Streaming**: Transmissão de vídeo/audio direta
5. **Termination**: Encerramento da chamada

### WebRTC JavaScript Implementation

```javascript
class VideoCallManager {
    constructor(hubConnection) {
        this.hubConnection = hubConnection;
        this.peerConnection = null;
        this.localStream = null;
        this.remoteStream = null;
    }

    async startCall(targetUserId) {
        // 1. Criar peer connection
        this.peerConnection = new RTCPeerConnection({
            iceServers: [
                { urls: 'stun:stun.l.google.com:19302' }
            ]
        });

        // 2. Configurar handlers
        this.peerConnection.onicecandidate = (event) => {
            if (event.candidate) {
                this.hubConnection.invoke('SendWebRTCIceCandidateAsync', 
                    this.callId, event.candidate);
            }
        };

        this.peerConnection.ontrack = (event) => {
            this.remoteStream = event.streams[0];
            this.onRemoteStream(this.remoteStream);
        };

        // 3. Obter stream local
        this.localStream = await navigator.mediaDevices.getUserMedia({
            video: true,
            audio: true
        });

        this.localStream.getTracks().forEach(track => {
            this.peerConnection.addTrack(track, this.localStream);
        });

        // 4. Iniciar chamada via SignalR
        this.callId = await this.hubConnection.invoke('StartVideoCallAsync', 
            this.groupId, targetUserId);
    }

    async handleWebRTCOffer(offer) {
        await this.peerConnection.setRemoteDescription(offer);
        const answer = await this.peerConnection.createAnswer();
        await this.peerConnection.setLocalDescription(answer);
        
        await this.hubConnection.invoke('SendWebRTCAnswerAsync', 
            this.callId, answer);
    }

    async handleWebRTCAnswer(answer) {
        await this.peerConnection.setRemoteDescription(answer);
    }

    async handleIceCandidate(candidate) {
        await this.peerConnection.addIceCandidate(candidate);
    }
}
```

## 🔧 Configuration

### SignalR Server Configuration

```csharp
// FamilyMeetHttpApiHostModule.cs
private void ConfigureSignalR(ServiceConfigurationContext context, IConfiguration configuration)
{
    context.Services.AddSignalR(options =>
    {
        options.EnableDetailedErrors = true;
        options.KeepAliveInterval = TimeSpan.FromSeconds(15);
        options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
        options.HandshakeTimeout = TimeSpan.FromSeconds(15);
        options.StreamBufferCapacity = 10;
    });

    context.Services.Configure<HubOptions>(options =>
    {
        options.EnableDetailedErrors = true;
        options.KeepAliveInterval = TimeSpan.FromSeconds(15);
        options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
        options.HandshakeTimeout = TimeSpan.FromSeconds(15);
        options.StreamBufferCapacity = 10;
    });
}
```

### Endpoint Configuration

```csharp
app.UseEndpoints(endpoints =>
{
    endpoints.MapHub<ChatHub>("/chat-hub", options =>
    {
        options.Transports = HttpTransportType.WebSockets | HttpTransportType.LongPolling;
        options.WebSockets.CloseTimeout = TimeSpan.FromSeconds(30);
        options.ApplicationMaxBufferSize = 32 * 1024 * 1024; // 32MB
        options.TransportMaxBufferSize = 32 * 1024 * 1024; // 32MB
    });
    
    endpoints.MapHub<VideoHub>("/video-hub", options =>
    {
        options.Transports = HttpTransportType.WebSockets;
        options.WebSockets.CloseTimeout = TimeSpan.FromSeconds(30);
        options.ApplicationMaxBufferSize = 64 * 1024 * 1024; // 64MB
        options.TransportMaxBufferSize = 64 * 1024 * 1024; // 64MB
    });
});
```

## 🌐 Client Implementation

### Angular SignalR Client

```typescript
// chat.service.ts
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';

@Injectable({
  providedIn: 'root'
})
export class ChatService {
  private hubConnection: HubConnection;
  
  constructor() {
    this.hubConnection = new HubConnectionBuilder()
      .withUrl('/chat-hub')
      .withAutomaticReconnect()
      .build();
  }

  async startConnection() {
    try {
      await this.hubConnection.start();
      console.log('SignalR Connected');
    } catch (err) {
      console.error('Error connecting to SignalR:', err);
    }
  }

  // Event listeners
  onReceiveMessage(callback: (message: any) => void) {
    this.hubConnection.on('ReceiveMessage', callback);
  }

  onUserJoined(callback: (user: any) => void) {
    this.hubConnection.on('UserJoined', callback);
  }

  onVideoCallRequest(callback: (call: any) => void) {
    this.hubConnection.on('VideoCallRequest', callback);
  }

  // Methods
  async sendMessage(message: any) {
    return this.hubConnection.invoke('SendMessageAsync', message);
  }

  async joinGroup(groupId: string) {
    return this.hubConnection.invoke('JoinGroupAsync', groupId);
  }

  async startVideoCall(groupId: string, targetUserId: string) {
    return this.hubConnection.invoke('StartVideoCallAsync', groupId, targetUserId);
  }
}
```

### Video Call Component

```typescript
// video-call.component.ts
@Component({
  selector: 'app-video-call',
  template: `
    <div class="video-container">
      <video #localVideo autoplay muted></video>
      <video #remoteVideo autoplay></video>
      
      <div class="controls">
        <button (click)="startCall()" [disabled]="isInCall">Start Call</button>
        <button (click)="endCall()" [disabled]="!isInCall">End Call</button>
        <button (click)="toggleScreenShare()">Screen Share</button>
      </div>
    </div>
  `
})
export class VideoCallComponent implements OnInit {
  @ViewChild('localVideo') localVideo!: ElementRef;
  @ViewChild('remoteVideo') remoteVideo!: ElementRef;
  
  private peerConnection: RTCPeerConnection;
  private localStream: MediaStream;
  private isInCall = false;

  constructor(private chatService: ChatService) {}

  ngOnInit() {
    this.setupSignalRListeners();
  }

  async startCall() {
    this.localStream = await navigator.mediaDevices.getUserMedia({
      video: true,
      audio: true
    });
    
    this.localVideo.nativeElement.srcObject = this.localStream;
    
    // Iniciar chamada via SignalR
    await this.chatService.startVideoCall(this.groupId, this.targetUserId);
  }

  private setupSignalRListeners() {
    this.chatService.onWebRTCOffer(async (offer) => {
      await this.handleOffer(offer);
    });

    this.chatService.onWebRTCAnswer(async (answer) => {
      await this.handleAnswer(answer);
    });

    this.chatService.onWebRTCIceCandidate(async (candidate) => {
      await this.handleIceCandidate(candidate);
    });
  }
}
```

## 📱 Mobile Support

### React Native SignalR

```javascript
import * as signalR from '@microsoft/signalr';

const connection = new signalR.HubConnectionBuilder()
  .withUrl('https://your-api.com/chat-hub')
  .withAutomaticReconnect()
  .build();

connection.on('ReceiveMessage', (message) => {
  // Handle message
});

await connection.start();
```

### WebRTC Mobile

```javascript
// React Native WebRTC
import { RTCPeerConnection, RTCView } from 'react-native-webrtc';

const configuration = {
  iceServers: [
    { urls: 'stun:stun.l.google.com:19302' }
  ]
};

const peerConnection = new RTCPeerConnection(configuration);
```

## 🔒 Security Considerations

### Authentication
```typescript
const connection = new HubConnectionBuilder()
  .withUrl('/chat-hub', {
    accessTokenFactory: () => this.authService.getAccessToken()
  })
  .build();
```

### Rate Limiting
```csharp
// No ChatHub
[HubMethodName("SendMessageAsync")]
public async Task SendMessageAsync(ChatMessageSignalRDto message)
{
    // Implement rate limiting
    var userKey = $"rate_limit_{CurrentUser.Id}";
    var count = await _cache.GetStringAsync(userKey);
    
    if (int.TryParse(count, out var messageCount) && messageCount > 10)
    {
        throw new HubException("Rate limit exceeded");
    }
    
    await _cache.SetStringAsync(userKey, (messageCount + 1).ToString(), 
        TimeSpan.FromMinutes(1));
}
```

### Message Validation
```csharp
public class ChatMessageSignalRDto
{
    [Required]
    [StringLength(4000)]
    public string Content { get; set; }
    
    [Required]
    public Guid ChatGroupId { get; set; }
    
    [Range(0, 3)]
    public MessageType Type { get; set; }
}
```

## 🚀 Performance Optimization

### Connection Pooling
```csharp
// Configurar keep-alive
options.KeepAliveInterval = TimeSpan.FromSeconds(15);
options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
```

### Message Batching
```typescript
// Client-side batching
const messageQueue = [];
let batchTimeout;

function queueMessage(message) {
    messageQueue.push(message);
    
    clearTimeout(batchTimeout);
    batchTimeout = setTimeout(() => {
        connection.invoke('SendBatchMessagesAsync', messageQueue);
        messageQueue.length = 0;
    }, 100);
}
```

### Compression
```csharp
// Server-side compression
services.AddSignalR(options =>
{
    options.EnableDetailedErrors = false; // Disable in production
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
});
```

## 📊 Monitoring & Logging

### SignalR Metrics
```csharp
public class ChatHub : AbpHub
{
    private readonly ILogger<ChatHub> _logger;

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("User {UserId} connected", CurrentUser.Id);
        await base.OnConnectedAsync();
    }

    public async Task SendMessageAsync(ChatMessageSignalRDto message)
    {
        _logger.LogInformation("Message sent to group {GroupId}", message.ChatGroupId);
        // ... implementation
    }
}
```

### Performance Monitoring
```typescript
// Client-side metrics
const metrics = {
    messagesReceived: 0,
    messagesSent: 0,
    connectionRestarts: 0,
    averageLatency: 0
};

connection.onreconnected(() => {
    metrics.connectionRestarts++;
});
```

## 🔧 Troubleshooting

### Common Issues

1. **WebSocket Connection Failed**
   - Check CORS configuration
   - Verify WebSocket support in browser
   - Check firewall settings

2. **WebRTC Connection Failed**
   - Verify STUN/TURN servers
   - Check network connectivity
   - Ensure HTTPS in production

3. **SignalR Reconnection Issues**
   - Implement exponential backoff
   - Check authentication tokens
   - Monitor network stability

### Debug Tools

```typescript
// Enable SignalR logging
const connection = new HubConnectionBuilder()
  .withUrl('/chat-hub')
  .configureLogging(signalR.LogLevel.Information)
  .build();
```

---

**Pronto!** Sistema de chat em tempo real com videoconferência P2P implementado! 🎉
