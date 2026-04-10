import { Injectable, OnDestroy } from '@angular/core';
import { BehaviorSubject, Subject, Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface SignalRMessage {
  type: string;
  data: unknown;
  timestamp: Date;
}

export type ConnectionStatus = 'disconnected' | 'connecting' | 'connected' | 'reconnecting';

@Injectable({
  providedIn: 'root'
})
export class SignalRService implements OnDestroy {
  private chatConnection: signalR.HubConnection | null = null;
  private videoConnection: signalR.HubConnection | null = null;

  private connectionStatusSubject = new BehaviorSubject<ConnectionStatus>('disconnected');
  private videoConnectionStatusSubject = new BehaviorSubject<ConnectionStatus>('disconnected');

  // Chat events
  private messageReceivedSubject = new Subject<unknown>();
  private messageEditedSubject = new Subject<unknown>();
  private messageDeletedSubject = new Subject<unknown>();
  private userJoinedSubject = new Subject<unknown>();
  private userLeftSubject = new Subject<unknown>();
  private userTypingSubject = new Subject<unknown>();
  private userStatusChangedSubject = new Subject<unknown>();
  private groupStatusSubject = new Subject<unknown>();

  // Video call events
  private videoCallRequestSubject = new Subject<unknown>();
  private videoCallAcceptedSubject = new Subject<unknown>();
  private videoCallRejectedSubject = new Subject<unknown>();
  private videoCallEndedSubject = new Subject<unknown>();
  private videoCallErrorSubject = new Subject<unknown>();
  private videoCallInitiatedSubject = new Subject<unknown>();

  // WebRTC signaling events
  private webrtcOfferSubject = new Subject<unknown>();
  private webrtcAnswerSubject = new Subject<unknown>();
  private webrtcIceCandidateSubject = new Subject<unknown>();
  private startWebRTCExchangeSubject = new Subject<unknown>();
  private screenShareStartedSubject = new Subject<unknown>();
  private screenShareStoppedSubject = new Subject<unknown>();
  private incomingVideoCallSubject = new Subject<unknown>();

  // Observables
  connectionStatus$ = this.connectionStatusSubject.asObservable();
  videoConnectionStatus$ = this.videoConnectionStatusSubject.asObservable();

  messageReceived$ = this.messageReceivedSubject.asObservable();
  messageEdited$ = this.messageEditedSubject.asObservable();
  messageDeleted$ = this.messageDeletedSubject.asObservable();
  userJoined$ = this.userJoinedSubject.asObservable();
  userLeft$ = this.userLeftSubject.asObservable();
  userTyping$ = this.userTypingSubject.asObservable();
  userStatusChanged$ = this.userStatusChangedSubject.asObservable();
  groupStatus$ = this.groupStatusSubject.asObservable();

  videoCallRequest$ = this.videoCallRequestSubject.asObservable();
  videoCallAccepted$ = this.videoCallAcceptedSubject.asObservable();
  videoCallRejected$ = this.videoCallRejectedSubject.asObservable();
  videoCallEnded$ = this.videoCallEndedSubject.asObservable();
  videoCallError$ = this.videoCallErrorSubject.asObservable();
  videoCallInitiated$ = this.videoCallInitiatedSubject.asObservable();

  webrtcOffer$ = this.webrtcOfferSubject.asObservable();
  webrtcAnswer$ = this.webrtcAnswerSubject.asObservable();
  webrtcIceCandidate$ = this.webrtcIceCandidateSubject.asObservable();
  startWebRTCExchange$ = this.startWebRTCExchangeSubject.asObservable();
  screenShareStarted$ = this.screenShareStartedSubject.asObservable();
  screenShareStopped$ = this.screenShareStoppedSubject.asObservable();
  incomingVideoCall$ = this.incomingVideoCallSubject.asObservable();

  async connectChatHub(token: string): Promise<void> {
    if (this.chatConnection) {
      await this.chatConnection.stop();
    }

    const signalR = await import('@microsoft/signalr');

    this.chatConnection = new signalR.HubConnectionBuilder()
      .withUrl(`${environment.apiUrl}/chat-hub`, {
        accessTokenFactory: () => token
      })
      .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
      .configureLogging(environment.enableDebug ? signalR.LogLevel.Information : signalR.LogLevel.Warning)
      .build();

    this.setupChatEventHandlers();
    this.setupChatConnectionHandlers();

    try {
      this.connectionStatusSubject.next('connecting');
      await this.chatConnection.start();
      this.connectionStatusSubject.next('connected');
      console.log('Chat hub connected');
    } catch (error) {
      this.connectionStatusSubject.next('disconnected');
      console.error('Error connecting to chat hub:', error);
      throw error;
    }
  }

  async connectVideoHub(token: string): Promise<void> {
    if (this.videoConnection) {
      await this.videoConnection.stop();
    }

    const signalR = await import('@microsoft/signalr');

    this.videoConnection = new signalR.HubConnectionBuilder()
      .withUrl(`${environment.apiUrl}/video-hub`, {
        accessTokenFactory: () => token
      })
      .withAutomaticReconnect([0, 2000, 5000, 10000])
      .configureLogging(environment.enableDebug ? signalR.LogLevel.Information : signalR.LogLevel.Warning)
      .build();

    this.setupVideoEventHandlers();
    this.setupVideoConnectionHandlers();

    try {
      this.videoConnectionStatusSubject.next('connecting');
      await this.videoConnection.start();
      this.videoConnectionStatusSubject.next('connected');
      console.log('Video hub connected');
    } catch (error) {
      this.videoConnectionStatusSubject.next('disconnected');
      console.error('Error connecting to video hub:', error);
      throw error;
    }
  }

  // Chat Hub Methods
  async sendMessage(chatGroupId: string, content: string, type: number = 0): Promise<void> {
    await this.chatConnection?.invoke('SendMessageAsync', { chatGroupId, content, type });
  }

  async editMessage(messageId: string, newContent: string): Promise<void> {
    await this.chatConnection?.invoke('EditMessageAsync', messageId, newContent);
  }

  async deleteMessage(messageId: string): Promise<void> {
    await this.chatConnection?.invoke('DeleteMessageAsync', messageId);
  }

  async joinGroup(groupId: string): Promise<void> {
    await this.chatConnection?.invoke('JoinGroupAsync', groupId);
  }

  async leaveGroup(groupId: string): Promise<void> {
    await this.chatConnection?.invoke('LeaveGroupAsync', groupId);
  }

  async setTyping(groupId: string, isTyping: boolean): Promise<void> {
    await this.chatConnection?.invoke('UserTypingAsync', groupId, isTyping);
  }

  async updateOnlineStatus(isOnline: boolean): Promise<void> {
    await this.chatConnection?.invoke('UpdateOnlineStatusAsync', isOnline);
  }

  // Video Hub Methods
  async startVideoCall(groupId: string, targetUserId: string): Promise<void> {
    await this.videoConnection?.invoke('StartVideoCallAsync', groupId, targetUserId);
  }

  async acceptVideoCall(callId: string): Promise<void> {
    await this.videoConnection?.invoke('AcceptVideoCallAsync', callId);
  }

  async rejectVideoCall(callId: string, reason?: string): Promise<void> {
    await this.videoConnection?.invoke('RejectVideoCallAsync', callId, reason ?? 'User declined');
  }

  async endVideoCall(callId: string): Promise<void> {
    await this.videoConnection?.invoke('EndVideoCallAsync', callId);
  }

  async sendWebRTCOffer(callId: string, offer: RTCSessionDescriptionInit): Promise<void> {
    await this.videoConnection?.invoke('SendWebRTCOfferAsync', callId, offer);
  }

  async sendWebRTCAnswer(callId: string, answer: RTCSessionDescriptionInit): Promise<void> {
    await this.videoConnection?.invoke('SendWebRTCAnswerAsync', callId, answer);
  }

  async sendWebRTCIceCandidate(callId: string, candidate: RTCIceCandidateInit): Promise<void> {
    await this.videoConnection?.invoke('SendWebRTCIceCandidateAsync', callId, candidate);
  }

  async startScreenShare(callId: string): Promise<void> {
    await this.videoConnection?.invoke('StartScreenShareAsync', callId);
  }

  async stopScreenShare(callId: string): Promise<void> {
    await this.videoConnection?.invoke('StopScreenShareAsync', callId);
  }

  async getActiveCalls(): Promise<void> {
    await this.videoConnection?.invoke('GetActiveCallsAsync');
  }

  // Chat hub - video call via chat hub (for notifications)
  async startVideoCallViaChat(groupId: string, targetUserId: string): Promise<void> {
    await this.chatConnection?.invoke('StartVideoCallAsync', groupId, targetUserId);
  }

  async disconnectAll(): Promise<void> {
    try {
      if (this.chatConnection) {
        await this.chatConnection.stop();
        this.chatConnection = null;
      }
      if (this.videoConnection) {
        await this.videoConnection.stop();
        this.videoConnection = null;
      }
    } catch (error) {
      console.error('Error disconnecting:', error);
    }
    this.connectionStatusSubject.next('disconnected');
    this.videoConnectionStatusSubject.next('disconnected');
  }

  private setupChatEventHandlers(): void {
    if (!this.chatConnection) return;

    this.chatConnection.on('ReceiveMessage', (data) => this.messageReceivedSubject.next(data));
    this.chatConnection.on('MessageEdited', (data) => this.messageEditedSubject.next(data));
    this.chatConnection.on('MessageDeleted', (data) => this.messageDeletedSubject.next(data));
    this.chatConnection.on('UserJoined', (data) => this.userJoinedSubject.next(data));
    this.chatConnection.on('UserLeft', (data) => this.userLeftSubject.next(data));
    this.chatConnection.on('UserTyping', (data) => this.userTypingSubject.next(data));
    this.chatConnection.on('UserStatusChanged', (data) => this.userStatusChangedSubject.next(data));
    this.chatConnection.on('GroupStatus', (data) => this.groupStatusSubject.next(data));
    this.chatConnection.on('VideoCallRequest', (data) => this.videoCallRequestSubject.next(data));
    this.chatConnection.on('VideoCallAccepted', (data) => this.videoCallAcceptedSubject.next(data));
    this.chatConnection.on('VideoCallRejected', (data) => this.videoCallRejectedSubject.next(data));
    this.chatConnection.on('VideoCallEnded', (data) => this.videoCallEndedSubject.next(data));
    this.chatConnection.on('VideoCallError', (data) => this.videoCallErrorSubject.next(data));
    this.chatConnection.on('Connected', (data) => console.log('Chat connected:', data));
  }

  private setupVideoEventHandlers(): void {
    if (!this.videoConnection) return;

    this.videoConnection.on('IncomingVideoCall', (data) => this.incomingVideoCallSubject.next(data));
    this.videoConnection.on('VideoCallInitiated', (data) => this.videoCallInitiatedSubject.next(data));
    this.videoConnection.on('VideoCallAccepted', (data) => this.videoCallAcceptedSubject.next(data));
    this.videoConnection.on('VideoCallRejected', (data) => this.videoCallRejectedSubject.next(data));
    this.videoConnection.on('VideoCallEnded', (data) => this.videoCallEndedSubject.next(data));
    this.videoConnection.on('VideoCallError', (data) => this.videoCallErrorSubject.next(data));
    this.videoConnection.on('WebRTCOffer', (data) => this.webrtcOfferSubject.next(data));
    this.videoConnection.on('WebRTCAnswer', (data) => this.webrtcAnswerSubject.next(data));
    this.videoConnection.on('WebRTCIceCandidate', (data) => this.webrtcIceCandidateSubject.next(data));
    this.videoConnection.on('StartWebRTCExchange', (data) => this.startWebRTCExchangeSubject.next(data));
    this.videoConnection.on('ScreenShareStarted', (data) => this.screenShareStartedSubject.next(data));
    this.videoConnection.on('ScreenShareStopped', (data) => this.screenShareStoppedSubject.next(data));
    this.videoConnection.on('VideoHubConnected', (data) => console.log('Video hub connected:', data));
    this.videoConnection.on('ActiveCalls', (data) => console.log('Active calls:', data));
    this.videoConnection.on('UserVideoStatus', (data) => console.log('User video status:', data));
  }

  private setupChatConnectionHandlers(): void {
    if (!this.chatConnection) return;

    this.chatConnection.onreconnecting(() => {
      this.connectionStatusSubject.next('reconnecting');
      console.log('Chat hub reconnecting...');
    });

    this.chatConnection.onreconnected(() => {
      this.connectionStatusSubject.next('connected');
      console.log('Chat hub reconnected');
    });

    this.chatConnection.onclose(() => {
      this.connectionStatusSubject.next('disconnected');
      console.log('Chat hub connection closed');
    });
  }

  private setupVideoConnectionHandlers(): void {
    if (!this.videoConnection) return;

    this.videoConnection.onreconnecting(() => {
      this.videoConnectionStatusSubject.next('reconnecting');
      console.log('Video hub reconnecting...');
    });

    this.videoConnection.onreconnected(() => {
      this.videoConnectionStatusSubject.next('connected');
      console.log('Video hub reconnected');
    });

    this.videoConnection.onclose(() => {
      this.videoConnectionStatusSubject.next('disconnected');
      console.log('Video hub connection closed');
    });
  }

  ngOnDestroy(): void {
    this.disconnectAll();
  }
}
