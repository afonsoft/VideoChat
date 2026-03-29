import { Component, OnInit, OnDestroy, ViewChild, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Subscription } from 'rxjs';
import { SignalRService, WebRTCService, ApiService } from '../../services';
import {
  ChatGroup,
  ChatMessage,
  SendMessage,
  CallInfo,
  JoinCall,
  LeaveCall,
  WebRTCSignal,
  UpdateParticipantStatus,
  MessageType,
  ParticipantStatus
} from '../../models';

@Component({
  selector: 'app-chat-room',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="chat-container">
      <!-- Group List -->
      <div class="group-list">
        <h3>Groups</h3>
        <button (click)="createNewGroup()" class="btn btn-primary">New Group</button>
        <div class="group-item"
             *ngFor="let group of groups"
             [class.active]="selectedGroup?.id === group.id"
             (click)="selectGroup(group)">
          <span>{{ group.name }}</span>
          <span class="participant-count">{{ group.currentParticipantsCount }}</span>
        </div>
      </div>

      <!-- Chat Area -->
      <div class="chat-area" *ngIf="selectedGroup">
        <div class="chat-header">
          <h3>{{ selectedGroup.name }}</h3>
          <div class="header-actions">
            <button (click)="toggleCall()"
                    [class.btn-call]="!isInCall"
                    [class.btn-hang-up]="isInCall"
                    class="btn">
              {{ isInCall ? 'Hang Up' : 'Start Call' }}
            </button>
            <span class="participant-count">
              {{ selectedGroup.activeCallParticipantsCount }} in call
            </span>
          </div>
        </div>

        <!-- Messages -->
        <div class="messages-container" #messagesContainer>
          <div class="message"
               *ngFor="let message of messages"
               [class.own]="message.senderId === currentUserId">
            <div class="message-header">
              <strong>{{ message.senderName }}</strong>
              <span class="timestamp">{{ formatTime(message.createdAt) }}</span>
            </div>
            <div class="message-content">{{ message.content }}</div>
          </div>
        </div>

        <!-- Message Input -->
        <div class="message-input">
          <input [(ngModel)]="newMessage"
                 (keyup.enter)="sendMessage()"
                 placeholder="Type a message..."
                 class="form-control" />
          <button (click)="sendMessage()" class="btn btn-primary">Send</button>
        </div>
      </div>

      <!-- Video Call Area -->
      <div class="video-call-area" *ngIf="isInCall && selectedGroup">
        <div class="video-grid">
          <!-- Local Video -->
          <div class="video-item local-video">
            <video #localVideo autoplay muted></video>
            <div class="video-label">You</div>
            <div class="video-controls">
              <button (click)="toggleAudio()"
                      [class.muted]="!isAudioEnabled"
                      class="btn-control">
                {{ isAudioEnabled ? '🎤' : '🔇' }}
              </button>
              <button (click)="toggleVideo()"
                      [class.muted]="!isVideoEnabled"
                      class="btn-control">
                {{ isVideoEnabled ? '📹' : '📹' }}
              </button>
            </div>
          </div>

          <!-- Remote Videos -->
          <div class="video-item"
               *ngFor="let remoteStream of remoteStreams">
            <video [srcObject]="remoteStream.stream" autoplay></video>
            <div class="video-label">{{ remoteStream.userName }}</div>
          </div>
        </div>

        <!-- Call Participants -->
        <div class="call-participants">
          <h4>Participants ({{ callInfo?.participants.length || 0 }})</h4>
          <div class="participant-list">
            <div class="participant-item"
                 *ngFor="let participant of callInfo?.participants">
              <span>{{ participant.userName }}</span>
              <span class="status">{{ participant.status }}</span>
            </div>
          </div>
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

    .group-list {
      width: 250px;
      border-right: 1px solid #ddd;
      padding: 20px;
      background: #f5f5f5;
    }

    .group-item {
      padding: 10px;
      margin: 5px 0;
      cursor: pointer;
      border-radius: 5px;
      display: flex;
      justify-content: space-between;
      align-items: center;
    }

    .group-item:hover {
      background: #e0e0e0;
    }

    .group-item.active {
      background: #007bff;
      color: white;
    }

    .chat-area {
      flex: 1;
      display: flex;
      flex-direction: column;
    }

    .chat-header {
      padding: 20px;
      border-bottom: 1px solid #ddd;
      display: flex;
      justify-content: space-between;
      align-items: center;
    }

    .header-actions {
      display: flex;
      align-items: center;
      gap: 10px;
    }

    .messages-container {
      flex: 1;
      padding: 20px;
      overflow-y: auto;
      background: #fafafa;
    }

    .message {
      margin: 10px 0;
      padding: 10px;
      border-radius: 10px;
      max-width: 70%;
    }

    .message.own {
      background: #007bff;
      color: white;
      margin-left: auto;
    }

    .message:not(.own) {
      background: white;
      border: 1px solid #ddd;
    }

    .message-header {
      font-size: 12px;
      opacity: 0.7;
      margin-bottom: 5px;
      display: flex;
      justify-content: space-between;
    }

    .message-input {
      padding: 20px;
      border-top: 1px solid #ddd;
      display: flex;
      gap: 10px;
    }

    .message-input input {
      flex: 1;
      padding: 10px;
      border: 1px solid #ddd;
      border-radius: 5px;
    }

    .btn {
      padding: 10px 20px;
      border: none;
      border-radius: 5px;
      cursor: pointer;
    }

    .btn-primary {
      background: #007bff;
      color: white;
    }

    .btn-call {
      background: #28a745;
      color: white;
    }

    .btn-hang-up {
      background: #dc3545;
      color: white;
    }

    .video-call-area {
      position: fixed;
      top: 0;
      right: 0;
      width: 400px;
      height: 100vh;
      background: #333;
      color: white;
      z-index: 1000;
      overflow-y: auto;
    }

    .video-grid {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 10px;
      padding: 10px;
    }

    .video-item {
      position: relative;
      background: #000;
      border-radius: 5px;
      overflow: hidden;
    }

    .video-item video {
      width: 100%;
      height: 150px;
      object-fit: cover;
    }

    .video-label {
      position: absolute;
      bottom: 5px;
      left: 5px;
      background: rgba(0,0,0,0.7);
      padding: 2px 8px;
      border-radius: 3px;
      font-size: 12px;
    }

    .video-controls {
      position: absolute;
      top: 5px;
      right: 5px;
      display: flex;
      gap: 5px;
    }

    .btn-control {
      width: 30px;
      height: 30px;
      border: none;
      border-radius: 50%;
      background: rgba(0,0,0,0.7);
      color: white;
      cursor: pointer;
      font-size: 14px;
    }

    .btn-control.muted {
      background: #dc3545;
    }

    .call-participants {
      padding: 10px;
      border-top: 1px solid #555;
    }

    .participant-item {
      display: flex;
      justify-content: space-between;
      padding: 5px 0;
      font-size: 14px;
    }

    .participant-count {
      font-size: 12px;
      opacity: 0.7;
    }

    .timestamp {
      font-size: 11px;
      opacity: 0.6;
    }
  `]
})
export class ChatRoomComponent implements OnInit, OnDestroy {
  @ViewChild('messagesContainer') messagesContainer!: ElementRef;
  @ViewChild('localVideo') localVideo!: ElementRef;

  currentUserId = 'user123'; // This should come from authentication
  groups: ChatGroup[] = [];
  selectedGroup: ChatGroup | undefined;
  messages: ChatMessage[] = [];
  newMessage = '';

  // Call related
  isInCall = false;
  callInfo: CallInfo | undefined;
  remoteStreams: any[] = [];
  isAudioEnabled = true;
  isVideoEnabled = true;

  private subscriptions: Subscription[] = [];

  constructor(
    private signalRService: SignalRService,
    private webRTCService: WebRTCService,
    private apiService: ApiService
  ) {}

  async ngOnInit() {
    await this.initializeServices();
    this.loadGroups();
    this.setupEventHandlers();
  }

  ngOnDestroy() {
    this.subscriptions.forEach(sub => sub.unsubscribe());
    this.disconnect();
  }

  private async initializeServices() {
    try {
      // Initialize SignalR connection
      await this.signalRService.startConnection(this.currentUserId, 'dummy-token');

      // Setup WebRTC callbacks
      this.webRTCService.setOnOfferCreated((userId, offer) => {
        this.signalRService.sendOffer(this.selectedGroup!.id, userId, offer.sdp);
      });

      this.webRTCService.setOnAnswerCreated((userId, answer) => {
        this.signalRService.sendAnswer(this.selectedGroup!.id, userId, answer.sdp);
      });

      this.webRTCService.setOnIceCandidate((userId, candidate) => {
        this.signalRService.sendIceCandidate(
          this.selectedGroup!.id,
          userId,
          candidate.candidate,
          candidate.sdpMid!,
          candidate.sdpMLineIndex!
        );
      });

      // Subscribe to WebRTC streams
      this.subscriptions.push(
        this.webRTCService.remoteStreams$.subscribe(streams => {
          this.remoteStreams = streams;
        })
      );

      this.subscriptions.push(
        this.webRTCService.localStream$.subscribe(stream => {
          if (stream && this.localVideo) {
            this.localVideo.nativeElement.srcObject = stream;
          }
        })
      );
    } catch (error) {
      console.error('Error initializing services:', error);
    }
  }

  private setupEventHandlers() {
    // SignalR event handlers
    this.subscriptions.push(
      this.signalRService.messageReceived$.subscribe(message => {
        if (message.chatGroupId === this.selectedGroup?.id) {
          this.messages.push(message);
          this.scrollToBottom();
        }
      })
    );

    this.subscriptions.push(
      this.signalRService.webRTCSignalReceived$.subscribe(signal => {
        this.handleWebRTCSignal(signal);
      })
    );

    this.subscriptions.push(
      this.signalRService.userJoinedCall$.subscribe(callInfo => {
        this.callInfo = callInfo;
        if (this.isInCall) {
          // Create peer connection for new participant
          this.webRTCService.createPeerConnection(signal.fromUserId, true);
        }
      })
    );

    this.subscriptions.push(
      this.signalRService.userLeftCall$.subscribe(data => {
        if (this.callInfo) {
          this.callInfo.participants = this.callInfo.participants.filter(
            p => p.userId !== data.userId
          );
        }
        this.webRTCService.removePeerConnection(data.userId);
      })
    );
  }

  private loadGroups() {
    this.apiService.getUserGroups(this.currentUserId).subscribe(groups => {
      this.groups = groups;
    });
  }

  selectGroup(group: ChatGroup) {
    this.selectedGroup = group;
    this.loadMessages();
    this.signalRService.joinGroup(group.id);
  }

  private loadMessages() {
    if (!this.selectedGroup) return;

    this.apiService.getMessages({
      chatGroupId: this.selectedGroup.id,
      pageNumber: 1,
      pageSize: 50
    }).subscribe(result => {
      this.messages = result.items;
      this.scrollToBottom();
    });
  }

  sendMessage() {
    if (!this.newMessage.trim() || !this.selectedGroup) return;

    const message: SendMessage = {
      content: this.newMessage,
      chatGroupId: this.selectedGroup.id,
      type: MessageType.Text
    };

    this.signalRService.sendMessage(message);
    this.newMessage = '';
  }

  async toggleCall() {
    if (!this.selectedGroup) return;

    if (this.isInCall) {
      await this.leaveCall();
    } else {
      await this.joinCall();
    }
  }

  private async joinCall() {
    try {
      // Initialize local media
      await this.webRTCService.initializeLocalStream(true, true);

      const joinCall: JoinCall = {
        groupId: this.selectedGroup!.id,
        userId: this.currentUserId,
        userName: 'Current User',
        hasAudio: true,
        hasVideo: true
      };

      await this.signalRService.joinCall(joinCall);
      this.isInCall = true;
    } catch (error) {
      console.error('Error joining call:', error);
    }
  }

  private async leaveCall() {
    try {
      const leaveCall: LeaveCall = {
        groupId: this.selectedGroup!.id,
        userId: this.currentUserId
      };

      await this.signalRService.leaveCall(leaveCall);
      this.webRTCService.disconnect();
      this.isInCall = false;
      this.callInfo = undefined;
    } catch (error) {
      console.error('Error leaving call:', error);
    }
  }

  private async handleWebRTCSignal(signal: WebRTCSignal) {
    try {
      switch (signal.type) {
        case 'offer':
          await this.webRTCService.handleOffer(signal.fromUserId, {
            type: 'offer',
            sdp: signal.sdp
          });
          break;
        case 'answer':
          await this.webRTCService.handleAnswer(signal.fromUserId, {
            type: 'answer',
            sdp: signal.sdp
          });
          break;
        case 'ice-candidate':
          await this.webRTCService.handleIceCandidate(signal.fromUserId, {
            candidate: signal.candidate,
            sdpMid: signal.sdpMid,
            sdpMLineIndex: signal.sdpMLineIndex
          });
          break;
      }
    } catch (error) {
      console.error('Error handling WebRTC signal:', error);
    }
  }

  toggleAudio() {
    this.webRTCService.toggleAudio();
    this.isAudioEnabled = this.webRTCService.isAudioEnabled();
    this.updateParticipantStatus();
  }

  toggleVideo() {
    this.webRTCService.toggleVideo();
    this.isVideoEnabled = this.webRTCService.isVideoEnabled();
    this.updateParticipantStatus();
  }

  private updateParticipantStatus() {
    if (!this.selectedGroup || !this.isInCall) return;

    const status: UpdateParticipantStatus = {
      groupId: this.selectedGroup.id,
      userId: this.currentUserId,
      status: ParticipantStatus.Connected,
      hasAudio: this.isAudioEnabled,
      hasVideo: this.isVideoEnabled
    };

    this.signalRService.updateParticipantStatus(status);
  }

  createNewGroup() {
    // This would open a modal or navigate to create group page
    console.log('Create new group');
  }

  private scrollToBottom() {
    setTimeout(() => {
      if (this.messagesContainer) {
        this.messagesContainer.nativeElement.scrollTop =
          this.messagesContainer.nativeElement.scrollHeight;
      }
    });
  }

  private formatTime(dateString: string): string {
    const date = new Date(dateString);
    return date.toLocaleTimeString();
  }

  private disconnect() {
    if (this.isInCall) {
      this.leaveCall();
    }
    this.signalRService.stopConnection();
  }
}
