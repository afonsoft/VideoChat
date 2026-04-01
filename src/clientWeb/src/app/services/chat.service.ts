import { Injectable } from '@angular/core';
import { Subject, Observable, BehaviorSubject } from 'rxjs';
import { webSocket, WebSocketSubject } from 'rxjs/webSocket';

export interface Message {
  id: string;
  groupId: string;
  userId: string;
  userName: string;
  userAvatar?: string;
  content: string;
  timestamp: Date;
  type: 'text' | 'system' | 'file' | 'image';
}

export interface ChatGroup {
  id: string;
  name: string;
  description?: string;
  type: 'chat' | 'video';
  creatorId: string;
  creatorName: string;
  createdAt: Date;
  lastActivityAt?: Date;
  isActive: boolean;
  currentParticipantsCount: number;
  maxParticipants: number;
  avatar?: string;
}

export interface CreateGroupRequest {
  name: string;
  description?: string;
  type: 'chat' | 'video';
  creatorId: string;
  maxParticipants: number;
}

@Injectable({
  providedIn: 'root'
})
export class ChatService {
  private readonly API_URL = 'http://localhost:5000/api';
  private readonly WS_URL = 'ws://localhost:5000/hubs/communication';
  
  private socket$!: WebSocketSubject<any>;
  private messageSubject = new Subject<Message>();
  private connectionStatus$ = new BehaviorSubject<boolean>(false);

  constructor() {
    this.connectWebSocket();
  }

  // WebSocket connection
  private connectWebSocket(): void {
    this.socket$ = webSocket({
      url: this.WS_URL,
      openObserver: {
        next: () => {
          console.log('WebSocket connected');
          this.connectionStatus$.next(true);
        }
      },
      closeObserver: {
        next: () => {
          console.log('WebSocket disconnected');
          this.connectionStatus$.next(false);
          // Try to reconnect after 5 seconds
          setTimeout(() => this.connectWebSocket(), 5000);
        }
      }
    });

    this.socket$.subscribe({
      next: (message) => this.handleWebSocketMessage(message),
      error: (error) => console.error('WebSocket error:', error)
    });
  }

  private handleWebSocketMessage(message: any): void {
    switch (message.type) {
      case 'newMessage':
        this.messageSubject.next(message.data);
        break;
      case 'userJoined':
        console.log('User joined:', message.data);
        break;
      case 'userLeft':
        console.log('User left:', message.data);
        break;
      default:
        console.log('Unknown message type:', message);
    }
  }

  // API methods
  async getUserGroups(userId: string): Promise<ChatGroup[]> {
    try {
      const response = await fetch(`${this.API_URL}/chat/user-groups/${userId}`, {
        headers: this.getHeaders()
      });

      if (!response.ok) {
        throw new Error('Failed to fetch groups');
      }

      return await response.json();
    } catch (error) {
      console.error('Error fetching groups:', error);
      return this.getMockGroups();
    }
  }

  async getMessages(groupId: string): Promise<Message[]> {
    try {
      const response = await fetch(`${this.API_URL}/chat/messages/${groupId}`, {
        headers: this.getHeaders()
      });

      if (!response.ok) {
        throw new Error('Failed to fetch messages');
      }

      return await response.json();
    } catch (error) {
      console.error('Error fetching messages:', error);
      return this.getMockMessages(groupId);
    }
  }

  async sendMessage(message: Message): Promise<void> {
    try {
      // Send via WebSocket for real-time
      this.socket$?.next({
        type: 'sendMessage',
        data: message
      });

      // Also save via API
      const response = await fetch(`${this.API_URL}/chat/messages`, {
        method: 'POST',
        headers: this.getHeaders(),
        body: JSON.stringify(message)
      });

      if (!response.ok) {
        throw new Error('Failed to send message');
      }
    } catch (error) {
      console.error('Error sending message:', error);
      throw error;
    }
  }

  async createGroup(groupData: CreateGroupRequest): Promise<ChatGroup> {
    try {
      const response = await fetch(`${this.API_URL}/chat/groups`, {
        method: 'POST',
        headers: this.getHeaders(),
        body: JSON.stringify(groupData)
      });

      if (!response.ok) {
        throw new Error('Failed to create group');
      }

      return await response.json();
    } catch (error) {
      console.error('Error creating group:', error);
      throw error;
    }
  }

  joinGroup(groupId: string): void {
    this.socket$?.next({
      type: 'joinGroup',
      data: { groupId }
    });
  }

  leaveGroup(groupId: string): void {
    this.socket$?.next({
      type: 'leaveGroup',
      data: { groupId }
    });
  }

  // Observables
  onMessage(): Observable<Message> {
    return this.messageSubject.asObservable();
  }

  getConnectionStatus(): Observable<boolean> {
    return this.connectionStatus$.asObservable();
  }

  // Private helper methods
  private getHeaders(): HeadersInit {
    const token = localStorage.getItem('familymeet_token');
    return {
      'Content-Type': 'application/json',
      ...(token && { 'Authorization': `Bearer ${token}` })
    };
  }

  // Mock data for development
  private getMockGroups(): ChatGroup[] {
    return [
      {
        id: '1',
        name: 'Família',
        description: 'Grupo da família',
        type: 'chat',
        creatorId: '1',
        creatorName: 'Você',
        createdAt: new Date('2024-01-15'),
        lastActivityAt: new Date(),
        isActive: true,
        currentParticipantsCount: 4,
        maxParticipants: 50,
        avatar: 'https://picsum.photos/seed/family/200/200.jpg'
      },
      {
        id: '2',
        name: 'Amigos',
        description: 'Grupo dos amigos',
        type: 'chat',
        creatorId: '1',
        creatorName: 'Você',
        createdAt: new Date('2024-02-20'),
        lastActivityAt: new Date(Date.now() - 3600000),
        isActive: true,
        currentParticipantsCount: 8,
        maxParticipants: 50,
        avatar: 'https://picsum.photos/seed/friends/200/200.jpg'
      },
      {
        id: '3',
        name: 'Trabalho',
        description: 'Equipe de trabalho',
        type: 'chat',
        creatorId: '1',
        creatorName: 'Você',
        createdAt: new Date('2024-03-10'),
        lastActivityAt: new Date(Date.now() - 7200000),
        isActive: true,
        currentParticipantsCount: 12,
        maxParticipants: 50,
        avatar: 'https://picsum.photos/seed/work/200/200.jpg'
      }
    ];
  }

  private getMockMessages(groupId: string): Message[] {
    const now = new Date();
    return [
      {
        id: '1',
        groupId,
        userId: '2',
        userName: 'Maria',
        userAvatar: 'https://picsum.photos/seed/maria/200/200.jpg',
        content: 'Oi pessoal! Como estão todos?',
        timestamp: new Date(now.getTime() - 3600000),
        type: 'text'
      },
      {
        id: '2',
        groupId,
        userId: '3',
        userName: 'João',
        userAvatar: 'https://picsum.photos/seed/joao/200/200.jpg',
        content: 'Tudo bem por aqui! 😊',
        timestamp: new Date(now.getTime() - 3000000),
        type: 'text'
      },
      {
        id: '3',
        groupId,
        userId: '1',
        userName: 'Você',
        userAvatar: 'https://picsum.photos/seed/user1/200/200.jpg',
        content: 'Ótimo! Alguém quer fazer uma videochamada mais tarde?',
        timestamp: new Date(now.getTime() - 2400000),
        type: 'text'
      }
    ];
  }
}
