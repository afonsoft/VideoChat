import { Component, OnInit, OnDestroy } from '@angular/core';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-chat-room',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="chat-container">
      <div class="chat-sidebar">
        <div class="user-info">
          <h3>FamilyChat</h3>
          <p>Usuário: {{currentUser}}</p>
          <div class="connection-status" [class]="{'connected': isConnected, 'disconnected': !isConnected}">
            {{isConnected ? '🟢 Conectado' : '🔴 Desconectado'}}
          </div>
        </div>
        
        <div class="groups-section">
          <h4>Grupos</h4>
          <div class="group-list">
            <div class="group-item" 
                 *ngFor="let group of groups" 
                 [class]="{'active': selectedGroupId === group.id}"
                 (click)="selectGroup(group)">
              <span class="group-name">{{group.name}}</span>
              <span class="group-count">{{group.participantCount}}/{{group.maxParticipants}}</span>
            </div>
          </div>
        </div>
      </div>

      <div class="chat-main">
        <div class="chat-header" *ngIf="selectedGroup">
          <h2>{{selectedGroup.name}}</h2>
          <div class="chat-actions">
            <button (click)="toggleVideoCall()" 
                    [disabled]="!isConnected || !canJoinVideoCall"
                    [class]="{'in-call': isInVideoCall}">
              {{isInVideoCall ? '📞 Sair da Chamada' : '📹 Iniciar Chamada'}}
            </button>
          </div>
        </div>

        <div class="messages-container" #messagesContainer>
          <div class="message" 
               *ngFor="let message of messages" 
               [class]="{'system': message.type === 'system', 'own': message.isOwn}">
            <div class="message-header">
              <span class="sender">{{message.senderName}}</span>
              <span class="time">{{formatTime(message.createdAt)}}</span>
            </div>
            <div class="message-content">
              {{message.content}}
            </div>
          </div>
        </div>

        <div class="message-input" *ngIf="selectedGroup">
          <form (ngSubmit)="sendMessage($event)">
            <input type="text" 
                   [(ngModel)]="newMessage" 
                   placeholder="Digite sua mensagem..." 
                   [disabled]="!isConnected"
                   (keyup.enter)="sendMessage($event)">
            <button type="submit" 
                    [disabled]="!isConnected || !newMessage.trim()">
              Enviar
            </button>
          </form>
        </div>
      </div>

      <!-- Video Call Overlay -->
      <div class="video-call-overlay" *ngIf="isInVideoCall">
        <div class="video-container">
          <div class="video-participant" *ngFor="let participant of callParticipants">
            <div class="participant-info">
              <span>{{participant.userName}}</span>
              <span class="status">{{participant.status}}</span>
            </div>
            <video #videoElement autoplay playsinline></video>
          </div>
          <div class="local-video">
            <video #localVideoElement autoplay muted playsinline></video>
            <div class="local-video-label">Você</div>
          </div>
        </div>
        <div class="call-controls">
          <button (click)="toggleAudio()" [class]="{'muted': !isAudioEnabled}">
            {{isAudioEnabled ? '🎤' : '🔇'}}
          </button>
          <button (click)="toggleVideo()" [class]="{'video-off': !isVideoEnabled}">
            {{isVideoEnabled ? '📹' : '📹'}}
          </button>
          <button (click)="endCall()" class="end-call-btn">
            📞 Encerrar Chamada
          </button>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .chat-container {
      display: flex;
      height: 100vh;
      font-family: Arial, sans-serif;
    }

    .chat-sidebar {
      width: 300px;
      background: #f8f9fa;
      border-right: 1px solid #dee2e6;
      display: flex;
      flex-direction: column;
    }

    .user-info {
      padding: 20px;
      border-bottom: 1px solid #dee2e6;
    }

    .connection-status {
      padding: 8px;
      border-radius: 4px;
      font-size: 12px;
      margin-top: 10px;
    }

    .connected {
      background: #d4edda;
      color: #155724;
    }

    .disconnected {
      background: #f8d7da;
      color: #721c24;
    }

    .groups-section h4 {
      padding: 15px 20px;
      margin: 0;
      background: #e9ecef;
    }

    .group-list {
      flex: 1;
      overflow-y: auto;
    }

    .group-item {
      padding: 12px 20px;
      border-bottom: 1px solid #dee2e6;
      cursor: pointer;
      display: flex;
      justify-content: space-between;
      align-items: center;
    }

    .group-item:hover {
      background: #f8f9fa;
    }

    .group-item.active {
      background: #007bff;
      color: white;
    }

    .group-name {
      font-weight: 500;
    }

    .group-count {
      font-size: 12px;
      opacity: 0.7;
    }

    .chat-main {
      flex: 1;
      display: flex;
      flex-direction: column;
    }

    .chat-header {
      padding: 20px;
      background: #fff;
      border-bottom: 1px solid #dee2e6;
      display: flex;
      justify-content: space-between;
      align-items: center;
    }

    .chat-actions button {
      padding: 8px 16px;
      border: none;
      border-radius: 4px;
      cursor: pointer;
      font-weight: 500;
    }

    .chat-actions button:not(:disabled) {
      background: #28a745;
      color: white;
    }

    .chat-actions button:hover:not(:disabled) {
      background: #218838;
    }

    .chat-actions button:disabled {
      background: #6c757d;
      cursor: not-allowed;
    }

    .in-call {
      background: #dc3545;
    }

    .messages-container {
      flex: 1;
      overflow-y: auto;
      padding: 20px;
      background: #f8f9fa;
    }

    .message {
      margin-bottom: 15px;
      padding: 12px;
      border-radius: 8px;
      max-width: 70%;
    }

    .message.own {
      background: #007bff;
      color: white;
      margin-left: auto;
    }

    .message.system {
      background: #6c757d;
      color: white;
      text-align: center;
      font-style: italic;
    }

    .message-header {
      display: flex;
      justify-content: space-between;
      margin-bottom: 5px;
      font-size: 12px;
      opacity: 0.7;
    }

    .message-content {
      line-height: 1.4;
    }

    .message-input {
      padding: 20px;
      border-top: 1px solid #dee2e6;
      background: white;
    }

    .message-input form {
      display: flex;
      gap: 10px;
    }

    .message-input input {
      flex: 1;
      padding: 12px;
      border: 1px solid #ced4da;
      border-radius: 4px;
      font-size: 14px;
    }

    .message-input button {
      padding: 12px 20px;
      background: #007bff;
      color: white;
      border: none;
      border-radius: 4px;
      cursor: pointer;
      font-weight: 500;
    }

    .message-input button:hover:not(:disabled) {
      background: #0056b3;
    }

    .message-input button:disabled {
      background: #6c757d;
      cursor: not-allowed;
    }

    .video-call-overlay {
      position: fixed;
      top: 0;
      left: 0;
      right: 0;
      bottom: 0;
      background: rgba(0, 0, 0, 0.9);
      z-index: 1000;
      display: flex;
      flex-direction: column;
    }

    .video-container {
      flex: 1;
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
      gap: 10px;
      padding: 20px;
    }

    .video-participant {
      background: #000;
      border-radius: 8px;
      overflow: hidden;
      position: relative;
    }

    .participant-info {
      position: absolute;
      top: 10px;
      left: 10px;
      background: rgba(0, 0, 0, 0.7);
      color: white;
      padding: 5px 10px;
      border-radius: 4px;
      font-size: 12px;
      z-index: 1;
    }

    .status {
      margin-left: 10px;
      padding: 2px 6px;
      border-radius: 3px;
      font-size: 10px;
    }

    .local-video {
      position: fixed;
      bottom: 20px;
      right: 20px;
      width: 200px;
      background: #000;
      border-radius: 8px;
      overflow: hidden;
    }

    .local-video video {
      width: 100%;
      height: 150px;
      object-fit: cover;
    }

    .local-video-label {
      position: absolute;
      bottom: 5px;
      left: 50%;
      transform: translateX(-50%);
      background: rgba(0, 0, 0, 0.7);
      color: white;
      padding: 2px 8px;
      border-radius: 3px;
      font-size: 11px;
    }

    .call-controls {
      position: fixed;
      bottom: 180px;
      right: 20px;
      display: flex;
      gap: 10px;
      z-index: 1001;
    }

    .call-controls button {
      width: 50px;
      height: 50px;
      border-radius: 50%;
      border: none;
      background: rgba(0, 0, 0, 0.7);
      color: white;
      font-size: 20px;
      cursor: pointer;
    }

    .call-controls button:hover {
      background: rgba(0, 0, 0, 0.9);
    }

    .end-call-btn {
      background: #dc3545 !important;
    }

    @media (max-width: 768px) {
      .chat-container {
        flex-direction: column;
      }
      
      .chat-sidebar {
        width: 100%;
        height: auto;
        max-height: 200px;
      }
      
      .video-container {
        grid-template-columns: 1fr;
      }
    }
  `]
})
export class ChatRoomComponent implements OnInit, OnDestroy {
  private hubConnection!: HubConnection;
  private currentUser: string = 'Usuário Demo';
  private isConnected = false;
  private selectedGroupId: string | null = null;
  private isInVideoCall = false;
  private isAudioEnabled = true;
  private isVideoEnabled = true;

  // Mock data for development
  groups = [
    { id: '1', name: 'Geral', participantCount: 4, maxParticipants: 10 },
    { id: '2', name: 'Trabalho', participantCount: 3, maxParticipants: 10 },
    { id: '3', name: 'Família', participantCount: 5, maxParticipants: 10 }
  ];

  messages = [
    {
      id: '1',
      content: 'Bem-vindo ao FamilyChat! 🎉',
      senderName: 'Sistema',
      createdAt: new Date(),
      type: 'system',
      isOwn: false
    },
    {
      id: '2',
      content: 'Olá pessoal! Como estão?',
      senderName: 'Maria Silva',
      createdAt: new Date(Date.now() - 300000),
      type: 'text',
      isOwn: false
    },
    {
      id: '3',
      content: 'Tudo bem por aqui! 👋',
      senderName: 'Usuário Demo',
      createdAt: new Date(Date.now() - 240000),
      type: 'text',
      isOwn: true
    }
  ];

  callParticipants = [
    { userName: 'Maria Silva', status: 'Conectado' },
    { userName: 'João Santos', status: 'Conectado' }
  ];

  newMessage: string = '';

  ngOnInit() {
    this.initializeSignalR();
  }

  ngOnDestroy() {
    if (this.hubConnection) {
      this.hubConnection.stop();
    }
  }

  private initializeSignalR() {
    this.hubConnection = new HubConnectionBuilder()
      .withUrl('http://localhost:5000/hubs/communication')
      .build();

    this.hubConnection.start()
      .then(() => {
        this.isConnected = true;
        console.log('Connected to FamilyChat hub');
      })
      .catch(err => console.error('Error connecting to hub:', err));

    // Set up event handlers
    this.hubConnection.on('MessageReceived', (message: any) => {
      this.messages.push(message);
      this.scrollToBottom();
    });

    this.hubConnection.on('UserJoinedGroup', (data: any) => {
      console.log('User joined group:', data);
    });

    this.hubConnection.on('UserLeftGroup', (data: any) => {
      console.log('User left group:', data);
    });
  }

  selectGroup(group: any) {
    this.selectedGroupId = group.id;
    this.loadMessages(group.id);
  }

  loadMessages(groupId: string) {
    // Mock implementation - would load from API
    console.log('Loading messages for group:', groupId);
  }

  sendMessage(event: Event) {
    event.preventDefault();
    if (!this.newMessage.trim() || !this.isConnected || !this.selectedGroupId) return;

    const message = {
      content: this.newMessage,
      chatGroupId: this.selectedGroupId,
      type: 'text'
    };

    this.hubConnection.invoke('SendMessage', message)
      .then(() => {
        this.newMessage = '';
        console.log('Message sent');
      })
      .catch(err => console.error('Error sending message:', err));
  }

  toggleVideoCall() {
    if (this.isInVideoCall) {
      this.endCall();
    } else {
      this.startVideoCall();
    }
  }

  startVideoCall() {
    this.isInVideoCall = true;
    console.log('Starting video call...');
    // WebRTC implementation would go here
  }

  endCall() {
    this.isInVideoCall = false;
    console.log('Ending video call...');
  }

  toggleAudio() {
    this.isAudioEnabled = !this.isAudioEnabled;
    console.log('Audio toggled:', this.isAudioEnabled);
  }

  toggleVideo() {
    this.isVideoEnabled = !this.isVideoEnabled;
    console.log('Video toggled:', this.isVideoEnabled);
  }

  canJoinVideoCall() {
    return this.isConnected && this.selectedGroupId !== null;
  }

  formatTime(date: Date): string {
    return date.toLocaleTimeString('pt-BR', { 
      hour: '2-digit', 
      minute: '2-digit' 
    });
  }

  private scrollToBottom() {
    setTimeout(() => {
      const container = document.querySelector('.messages-container');
      if (container) {
        container.scrollTop = container.scrollHeight;
      }
    }, 100);
  }
}
