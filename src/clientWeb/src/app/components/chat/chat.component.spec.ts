import { ComponentFixture, TestBed } from '@angular/core/testing';
import { FormsModule } from '@angular/forms';
import { By } from '@angular/platform-browser';
import { RouterTestingModule } from '@angular/router/testing';
import { of, Subject } from 'rxjs';

import { ChatComponent } from './chat.component';
import { AuthService, User } from '../../services/auth.service';
import { ChatService, Message, ChatGroup } from '../../services/chat.service';

describe('ChatComponent', () => {
  let component: ChatComponent;
  let fixture: ComponentFixture<ChatComponent>;
  let authServiceSpy: jasmine.SpyObj<AuthService>;
  let chatServiceSpy: jasmine.SpyObj<ChatService>;
  let messageSubject: Subject<Message>;

  const mockUser: User = {
    id: '1',
    email: 'test@example.com',
    name: 'Test User',
    avatar: 'https://example.com/avatar.jpg'
  };

  const mockGroups: ChatGroup[] = [
    {
      id: '1',
      name: 'Test Group 1',
      description: 'Test Description',
      type: 'chat',
      creatorId: '1',
      maxParticipants: 50,
      participants: [],
      createdAt: new Date(),
      updatedAt: new Date()
    },
    {
      id: '2',
      name: 'Test Group 2',
      description: 'Another Test',
      type: 'chat',
      creatorId: '1',
      maxParticipants: 50,
      participants: [],
      createdAt: new Date(),
      updatedAt: new Date()
    }
  ];

  const mockMessages: Message[] = [
    {
      id: '1',
      groupId: '1',
      userId: '1',
      userName: 'Test User',
      userAvatar: 'https://example.com/avatar.jpg',
      content: 'Hello World',
      timestamp: new Date(),
      type: 'text'
    },
    {
      id: '2',
      groupId: '1',
      userId: '2',
      userName: 'Other User',
      userAvatar: 'https://example.com/avatar2.jpg',
      content: 'Hi there!',
      timestamp: new Date(),
      type: 'text'
    }
  ];

  beforeEach(async () => {
    const authSpy = jasmine.createSpyObj('AuthService', ['logout']);
    const chatSpy = jasmine.createSpyObj('ChatService', [
      'getUserGroups',
      'getMessages',
      'joinGroup',
      'sendMessage',
      'createGroup',
      'onMessage'
    ]);

    messageSubject = new Subject<Message>();
    chatSpy.onMessage.and.returnValue(messageSubject.asObservable());

    await TestBed.configureTestingModule({
      imports: [
        FormsModule,
        RouterTestingModule
      ],
      declarations: [ChatComponent],
      providers: [
        { provide: AuthService, useValue: authSpy },
        { provide: ChatService, useValue: chatSpy }
      ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ChatComponent);
    component = fixture.componentInstance;
    authServiceSpy = TestBed.inject(AuthService) as jasmine.SpyObj<AuthService>;
    chatServiceSpy = TestBed.inject(ChatService) as jasmine.SpyObj<ChatService>;

    // Mock current user
    (component as any).currentUser = mockUser;
    
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should initialize with current user', () => {
    expect(component.currentUser).toBe(mockUser);
  });

  it('should load groups on initialization', () => {
    chatServiceSpy.getUserGroups.and.returnValue(Promise.resolve(mockGroups));

    component.ngOnInit();

    expect(chatServiceSpy.getUserGroups).toHaveBeenCalledWith(mockUser.id);
  });

  it('should select group and load messages', async () => {
    chatServiceSpy.getMessages.and.returnValue(Promise.resolve(mockMessages));
    chatServiceSpy.joinGroup.and.returnValue(Promise.resolve());

    await component.selectGroup(mockGroups[0]);

    expect(component.selectedGroup).toBe(mockGroups[0]);
    expect(component.showGroupList).toBe(false);
    expect(chatServiceSpy.getMessages).toHaveBeenCalledWith(mockGroups[0].id);
    expect(chatServiceSpy.joinGroup).toHaveBeenCalledWith(mockGroups[0].id);
    expect(component.messages).toEqual(mockMessages);
  });

  it('should send message when form is submitted', async () => {
    component.selectedGroup = mockGroups[0];
    component.newMessage = 'Test message';
    chatServiceSpy.sendMessage.and.returnValue(Promise.resolve());

    await component.sendMessage();

    expect(chatServiceSpy.sendMessage).toHaveBeenCalledWith({
      id: jasmine.any(String),
      groupId: mockGroups[0].id,
      userId: mockUser.id,
      userName: mockUser.name,
      userAvatar: mockUser.avatar,
      content: 'Test message',
      timestamp: jasmine.any(Date),
      type: 'text'
    });
    expect(component.newMessage).toBe('');
  });

  it('should not send empty message', async () => {
    component.selectedGroup = mockGroups[0];
    component.newMessage = '';
    chatServiceSpy.sendMessage.and.returnValue(Promise.resolve());

    await component.sendMessage();

    expect(chatServiceSpy.sendMessage).not.toHaveBeenCalled();
  });

  it('should create new group', async () => {
    const newGroup = { ...mockGroups[0], id: '3', name: 'New Group' };
    chatServiceSpy.createGroup.and.returnValue(Promise.resolve(newGroup));

    await component.createGroup('New Group');

    expect(chatServiceSpy.createGroup).toHaveBeenCalledWith({
      name: 'New Group',
      description: '',
      type: 'chat',
      creatorId: mockUser.id,
      maxParticipants: 50
    });
    expect(component.groups).toContain(newGroup);
    expect(component.showNewGroupModal).toBe(false);
  });

  it('should not create group with empty name', async () => {
    chatServiceSpy.createGroup.and.returnValue(Promise.resolve(mockGroups[0]));

    await component.createGroup('');

    expect(chatServiceSpy.createGroup).not.toHaveBeenCalled();
  });

  it('should go back to group list', () => {
    component.selectedGroup = mockGroups[0];
    component.messages = mockMessages;
    component.showGroupList = false;

    component.goBack();

    expect(component.showGroupList).toBe(true);
    expect(component.selectedGroup).toBeNull();
    expect(component.messages).toEqual([]);
  });

  it('should call logout when logout is triggered', () => {
    component.logout();

    expect(authServiceSpy.logout).toHaveBeenCalled();
  });

  it('should format time correctly', () => {
    const testDate = new Date('2023-01-01T15:30:00');
    const formattedTime = component.formatTime(testDate);

    expect(formattedTime).toMatch(/^\d{2}:\d{2}$/);
  });

  it('should identify own messages correctly', () => {
    const ownMessage: Message = {
      ...mockMessages[0],
      userId: mockUser.id
    };
    const otherMessage: Message = {
      ...mockMessages[1],
      userId: '2'
    };

    expect(component.isOwnMessage(ownMessage)).toBe(true);
    expect(component.isOwnMessage(otherMessage)).toBe(false);
  });

  it('should handle incoming messages', () => {
    component.selectedGroup = mockGroups[0];
    component.messages = [...mockMessages];

    const newMessage: Message = {
      id: '3',
      groupId: mockGroups[0].id,
      userId: '3',
      userName: 'New User',
      userAvatar: 'https://example.com/avatar3.jpg',
      content: 'New message',
      timestamp: new Date(),
      type: 'text'
    };

    messageSubject.next(newMessage);

    expect(component.messages).toContain(newMessage);
    expect(component.messages.length).toBe(mockMessages.length + 1);
  });

  it('should not add messages from other groups', () => {
    component.selectedGroup = mockGroups[0];
    component.messages = [...mockMessages];

    const otherGroupMessage: Message = {
      id: '3',
      groupId: '999', // Different group
      userId: '3',
      userName: 'New User',
      userAvatar: 'https://example.com/avatar3.jpg',
      content: 'Message from other group',
      timestamp: new Date(),
      type: 'text'
    };

    messageSubject.next(otherGroupMessage);

    expect(component.messages).not.toContain(otherGroupMessage);
    expect(component.messages.length).toBe(mockMessages.length);
  });

  it('should handle pagination correctly', () => {
    expect(component.currentPage).toBe(0);
    expect(component.totalPages).toBe(0);
  });

  it('should start video call when button is clicked', () => {
    component.selectedGroup = mockGroups[0];

    spyOn(console, 'log');

    component.startVideoCall();

    expect(console.log).toHaveBeenCalledWith('Starting video call for group:', mockGroups[0].name);
  });

  it('should cleanup subscriptions on destroy', () => {
    const subscriptionSpy = jasmine.createSpyObj('Subscription', ['unsubscribe']);
    component.subscriptions = [subscriptionSpy];

    component.ngOnDestroy();

    expect(subscriptionSpy.unsubscribe).toHaveBeenCalled();
  });
});
