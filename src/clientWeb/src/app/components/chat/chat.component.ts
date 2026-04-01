import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { AuthService, User } from '../../services/auth.service';
import { ChatService, Message, ChatGroup } from '../../services/chat.service';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-chat',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './chat.component.html',
  styleUrls: ['./chat.component.scss']
})
export class ChatComponent implements OnInit, OnDestroy {
  currentUser: User | null = null;
  selectedGroup: ChatGroup | null = null;
  groups: ChatGroup[] = [];
  messages: Message[] = [];
  newMessage = '';
  isLoading = false;
  showGroupList = true;
  showNewGroupModal = false;

  private subscriptions: Subscription[] = [];

  constructor(
    private authService: AuthService,
    private chatService: ChatService
  ) {}

  ngOnInit(): void {
    this.currentUser = this.authService.currentUser;

    if (!this.currentUser) {
      return;
    }

    this.loadGroups();
    this.setupMessageListener();
  }

  ngOnDestroy(): void {
    this.subscriptions.forEach(sub => sub.unsubscribe());
  }

  async loadGroups(): Promise<void> {
    try {
      this.groups = await this.chatService.getUserGroups(this.currentUser!.id);
    } catch (error) {
      console.error('Error loading groups:', error);
    }
  }

  async selectGroup(group: ChatGroup): Promise<void> {
    this.selectedGroup = group;
    this.showGroupList = false;
    this.isLoading = true;

    try {
      this.messages = await this.chatService.getMessages(group.id);
      this.chatService.joinGroup(group.id);
    } catch (error) {
      console.error('Error loading messages:', error);
    } finally {
      this.isLoading = false;
    }
  }

  async sendMessage(): Promise<void> {
    if (!this.newMessage.trim() || !this.selectedGroup || !this.currentUser) {
      return;
    }

    const message: Message = {
      id: Date.now().toString(),
      groupId: this.selectedGroup.id,
      userId: this.currentUser.id,
      userName: this.currentUser.name,
      userAvatar: this.currentUser.avatar,
      content: this.newMessage.trim(),
      timestamp: new Date(),
      type: 'text'
    };

    try {
      await this.chatService.sendMessage(message);
      this.newMessage = '';
    } catch (error) {
      console.error('Error sending message:', error);
    }
  }

  async createGroup(groupName: string): Promise<void> {
    if (!groupName.trim() || !this.currentUser) {
      return;
    }

    try {
      const newGroup = await this.chatService.createGroup({
        name: groupName.trim(),
        description: '',
        type: 'chat',
        creatorId: this.currentUser.id,
        maxParticipants: 50
      });

      this.groups.unshift(newGroup);
      this.showNewGroupModal = false;
    } catch (error) {
      console.error('Error creating group:', error);
    }
  }

  startVideoCall(): void {
    if (this.selectedGroup) {
      // Implement video call functionality
      console.log('Starting video call for group:', this.selectedGroup.name);
    }
  }

  goBack(): void {
    this.showGroupList = true;
    this.selectedGroup = null;
    this.messages = [];
  }

  logout(): void {
    this.authService.logout();
  }

  private setupMessageListener(): void {
    const messageSub = this.chatService.onMessage().subscribe((message: Message) => {
      if (this.selectedGroup && message.groupId === this.selectedGroup.id) {
        this.messages.push(message);
      }
    });

    this.subscriptions.push(messageSub);
  }

  formatTime(date: Date): string {
    return date.toLocaleTimeString('pt-BR', {
      hour: '2-digit',
      minute: '2-digit'
    });
  }

  isOwnMessage(message: Message): boolean {
    return message.userId === this.currentUser?.id;
  }
}
