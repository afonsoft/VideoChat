import { MessageType } from './enums';

export interface ChatMessage {
  id: string;
  content: string;
  senderId: string;
  senderName: string;
  chatGroupId: string;
  type: MessageType;
  createdAt: string;
  editedAt?: string;
  isDeleted: boolean;
  deletedAt?: string;
  replyToMessageId?: string;
  replyToMessage?: ChatMessage;
  attachments: ChatMessageAttachment[];
}

export interface ChatMessageAttachment {
  id: string;
  fileName: string;
  fileUrl: string;
  fileSize: number;
  mimeType: string;
  uploadedAt: string;
}

export interface SendMessage {
  content: string;
  chatGroupId: string;
  type: MessageType;
  replyToMessageId?: string;
}

export interface EditMessage {
  content: string;
}

export interface GetMessages {
  chatGroupId: string;
  pageNumber: number;
  pageSize: number;
  beforeDate?: string;
  afterDate?: string;
}

export interface MessagePagedResult {
  items: ChatMessage[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  hasNextPage: boolean;
}
