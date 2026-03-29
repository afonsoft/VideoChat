import { ParticipantStatus, CallParticipant } from './chat-group.model';

export interface WebRTCSignal {
  type: string; // offer, answer, ice-candidate
  sdp: string;
  candidate: string;
  sdpMid: string;
  sdpMLineIndex?: number;
  fromUserId: string;
  toUserId: string;
  roomId: string;
}

export interface JoinCall {
  groupId: string;
  userId: string;
  userName: string;
  hasAudio: boolean;
  hasVideo: boolean;
}

export interface LeaveCall {
  groupId: string;
  userId: string;
}

export interface UpdateParticipantStatus {
  groupId: string;
  userId: string;
  status: ParticipantStatus;
  hasAudio: boolean;
  hasVideo: boolean;
}

export interface CallInfo {
  groupId: string;
  groupName: string;
  participants: CallParticipant[];
  isActive: boolean;
  startedAt: string;
  maxParticipants: number;
}
