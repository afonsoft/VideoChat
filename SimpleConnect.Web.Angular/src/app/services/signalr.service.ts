import { Injectable } from '@angular/core';
import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { BehaviorSubject, Observable, Subject } from 'rxjs';
import { environment } from '../../environments/environment';
import { ChatMessage, WebRTCSignal, CallInfo, UpdateParticipantStatus } from '../models';

@Injectable({
  providedIn: 'root'
})
export class SignalRService {
  private hubConnection: HubConnection | undefined;
  private connectionSubject = new BehaviorSubject<boolean>(false);
  
  // Chat events
  private messageReceivedSubject = new Subject<ChatMessage>();
  private messageEditedSubject = new Subject<ChatMessage>();
  private messageDeletedSubject = new Subject<{ messageId: string }>();
  private userJoinedGroupSubject = new Subject<{ userId: string; group: any }>();
  private userLeftGroupSubject = new Subject<{ userId: string }>();
  
  // Call events
  private userJoinedCallSubject = new Subject<CallInfo>();
  private userLeftCallSubject = new Subject<{ userId: string; groupId: string }>();
  private participantStatusUpdatedSubject = new Subject<UpdateParticipantStatus>();
  private webRTCSignalReceivedSubject = new Subject<WebRTCSignal>();
  private callJoinedSubject = new Subject<CallInfo>();
  
  public connection$ = this.connectionSubject.asObservable();
  public messageReceived$ = this.messageReceivedSubject.asObservable();
  public messageEdited$ = this.messageEditedSubject.asObservable();
  public messageDeleted$ = this.messageDeletedSubject.asObservable();
  public userJoinedGroup$ = this.userJoinedGroupSubject.asObservable();
  public userLeftGroup$ = this.userLeftGroupSubject.asObservable();
  public userJoinedCall$ = this.userJoinedCallSubject.asObservable();
  public userLeftCall$ = this.userLeftCallSubject.asObservable();
  public participantStatusUpdated$ = this.participantStatusUpdatedSubject.asObservable();
  public webRTCSignalReceived$ = this.webRTCSignalReceivedSubject.asObservable();
  public callJoined$ = this.callJoinedSubject.asObservable();

  constructor() {}

  async startConnection(userId: string, token: string): Promise<void> {
    try {
      this.hubConnection = new HubConnectionBuilder()
        .withUrl(environment.signalRUrl, {
          accessTokenFactory: () => token
        })
        .withAutomaticReconnect()
        .configureLogging(LogLevel.Information)
        .build();

      // Register event handlers
      this.registerEventHandlers();

      await this.hubConnection.start();
      this.connectionSubject.next(true);
      
      console.log('SignalR connection established');
    } catch (error) {
      console.error('Error establishing SignalR connection:', error);
      this.connectionSubject.next(false);
      throw error;
    }
  }

  async stopConnection(): Promise<void> {
    if (this.hubConnection) {
      try {
        await this.hubConnection.stop();
        this.connectionSubject.next(false);
        console.log('SignalR connection stopped');
      } catch (error) {
        console.error('Error stopping SignalR connection:', error);
      }
    }
  }

  private registerEventHandlers(): void {
    if (!this.hubConnection) return;

    // Chat events
    this.hubConnection.on('MessageReceived', (message: ChatMessage) => {
      this.messageReceivedSubject.next(message);
    });

    this.hubConnection.on('MessageEdited', (message: ChatMessage) => {
      this.messageEditedSubject.next(message);
    });

    this.hubConnection.on('MessageDeleted', (data: { messageId: string }) => {
      this.messageDeletedSubject.next(data);
    });

    this.hubConnection.on('UserJoinedGroup', (data: { userId: string; group: any }) => {
      this.userJoinedGroupSubject.next(data);
    });

    this.hubConnection.on('UserLeftGroup', (data: { userId: string }) => {
      this.userLeftGroupSubject.next(data);
    });

    // Call events
    this.hubConnection.on('UserJoinedCall', (callInfo: CallInfo) => {
      this.userJoinedCallSubject.next(callInfo);
    });

    this.hubConnection.on('UserLeftCall', (data: { userId: string; groupId: string }) => {
      this.userLeftCallSubject.next(data);
    });

    this.hubConnection.on('ParticipantStatusUpdated', (data: UpdateParticipantStatus) => {
      this.participantStatusUpdatedSubject.next(data);
    });

    this.hubConnection.on('WebRTCSignalReceived', (signal: WebRTCSignal) => {
      this.webRTCSignalReceivedSubject.next(signal);
    });

    this.hubConnection.on('CallJoined', (callInfo: CallInfo) => {
      this.callJoinedSubject.next(callInfo);
    });

    this.hubConnection.on('Error', (error: any) => {
      console.error('SignalR error:', error);
    });
  }

  // Group management
  async joinGroup(groupId: string): Promise<void> {
    if (this.hubConnection) {
      await this.hubConnection.invoke('JoinGroup', groupId);
    }
  }

  async leaveGroup(groupId: string): Promise<void> {
    if (this.hubConnection) {
      await this.hubConnection.invoke('LeaveGroup', groupId);
    }
  }

  // Chat messages
  async sendMessage(message: any): Promise<void> {
    if (this.hubConnection) {
      await this.hubConnection.invoke('SendMessage', message);
    }
  }

  async editMessage(messageId: string, content: string): Promise<void> {
    if (this.hubConnection) {
      await this.hubConnection.invoke('EditMessage', messageId, content);
    }
  }

  async deleteMessage(messageId: string): Promise<void> {
    if (this.hubConnection) {
      await this.hubConnection.invoke('DeleteMessage', messageId);
    }
  }

  // Video call
  async joinCall(joinCall: any): Promise<void> {
    if (this.hubConnection) {
      await this.hubConnection.invoke('JoinCall', joinCall);
    }
  }

  async leaveCall(leaveCall: any): Promise<void> {
    if (this.hubConnection) {
      await this.hubConnection.invoke('LeaveCall', leaveCall);
    }
  }

  async updateParticipantStatus(status: UpdateParticipantStatus): Promise<void> {
    if (this.hubConnection) {
      await this.hubConnection.invoke('UpdateParticipantStatus', status);
    }
  }

  // WebRTC signaling
  async sendWebRTCSignal(signal: WebRTCSignal): Promise<void> {
    if (this.hubConnection) {
      await this.hubConnection.invoke('SendWebRTCSignal', signal);
    }
  }

  async sendOffer(roomId: string, targetUserId: string, sdp: string): Promise<void> {
    if (this.hubConnection) {
      await this.hubConnection.invoke('SendOffer', roomId, targetUserId, sdp);
    }
  }

  async sendAnswer(roomId: string, targetUserId: string, sdp: string): Promise<void> {
    if (this.hubConnection) {
      await this.hubConnection.invoke('SendAnswer', roomId, targetUserId, sdp);
    }
  }

  async sendIceCandidate(roomId: string, targetUserId: string, candidate: string, sdpMid: string, sdpMLineIndex: number): Promise<void> {
    if (this.hubConnection) {
      await this.hubConnection.invoke('SendIceCandidate', roomId, targetUserId, candidate, sdpMid, sdpMLineIndex);
    }
  }

  isConnected(): boolean {
    return this.hubConnection?.state === 'Connected';
  }
}
