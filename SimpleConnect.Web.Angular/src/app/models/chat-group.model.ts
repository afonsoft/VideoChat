import { ParticipantStatus } from './enums';

export interface ChatGroup {
  id: string;
  name: string;
  description: string;
  type: GroupType;
  creatorId: string;
  creatorName: string;
  createdAt: string;
  lastActivityAt?: string;
  isActive: boolean;
  maxParticipants: number;
  currentParticipantsCount: number;
  activeCallParticipantsCount: number;
  members: ChatGroupMember[];
  activeCallParticipants: CallParticipant[];
}

export interface ChatGroupMember {
  id: string;
  userId: string;
  userName: string;
  isCreator: boolean;
  joinedAt: string;
  lastSeenAt?: string;
  isActive: boolean;
  isInCall: boolean;
}

export interface CallParticipant {
  userId: string;
  userName: string;
  status: ParticipantStatus;
  joinedAt: string;
  hasAudio: boolean;
  hasVideo: boolean;
}

export interface CreateChatGroup {
  name: string;
  description: string;
  type: GroupType;
  maxParticipants: number;
}

export interface UpdateChatGroup {
  name: string;
  description: string;
}

export interface JoinGroup {
  groupId: string;
  userId: string;
  userName: string;
}

export interface LeaveGroup {
  groupId: string;
  userId: string;
}
