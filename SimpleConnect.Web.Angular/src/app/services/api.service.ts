import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { 
  ChatGroup, 
  CreateChatGroup, 
  UpdateChatGroup, 
  JoinGroup, 
  LeaveGroup,
  ChatMessage,
  SendMessage,
  EditMessage,
  GetMessages,
  MessagePagedResult,
  CallInfo,
  JoinCall,
  LeaveCall,
  UpdateParticipantStatus,
  CallParticipant
} from '../models';

@Injectable({
  providedIn: 'root'
})
export class ApiService {
  private readonly apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  // Chat Groups
  createGroup(group: CreateChatGroup): Observable<ChatGroup> {
    return this.http.post<ChatGroup>(`${this.apiUrl}/chat/groups`, group);
  }

  getGroup(id: string): Observable<ChatGroup> {
    return this.http.get<ChatGroup>(`${this.apiUrl}/chat/groups/${id}`);
  }

  updateGroup(id: string, group: UpdateChatGroup): Observable<ChatGroup> {
    return this.http.put<ChatGroup>(`${this.apiUrl}/chat/groups/${id}`, group);
  }

  deleteGroup(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/chat/groups/${id}`);
  }

  getUserGroups(userId: string): Observable<ChatGroup[]> {
    return this.http.get<ChatGroup[]>(`${this.apiUrl}/chat/groups/my-groups/${userId}`);
  }

  joinGroup(joinGroup: JoinGroup): Observable<ChatGroup> {
    return this.http.post<ChatGroup>(`${this.apiUrl}/chat/groups/join`, joinGroup);
  }

  leaveGroup(leaveGroup: LeaveGroup): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/chat/groups/leave`, leaveGroup);
  }

  getGroupMembers(groupId: string): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/chat/groups/${groupId}/members`);
  }

  // Chat Messages
  sendMessage(message: SendMessage): Observable<ChatMessage> {
    return this.http.post<ChatMessage>(`${this.apiUrl}/messages`, message);
  }

  getMessage(id: string): Observable<ChatMessage> {
    return this.http.get<ChatMessage>(`${this.apiUrl}/messages/${id}`);
  }

  editMessage(id: string, editMessage: EditMessage): Observable<ChatMessage> {
    return this.http.put<ChatMessage>(`${this.apiUrl}/messages/${id}`, editMessage);
  }

  deleteMessage(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/messages/${id}`);
  }

  getMessages(params: GetMessages): Observable<MessagePagedResult> {
    let httpParams = new HttpParams()
      .set('chatGroupId', params.chatGroupId)
      .set('pageNumber', params.pageNumber.toString())
      .set('pageSize', params.pageSize.toString());

    if (params.beforeDate) {
      httpParams = httpParams.set('beforeDate', params.beforeDate);
    }

    if (params.afterDate) {
      httpParams = httpParams.set('afterDate', params.afterDate);
    }

    return this.http.get<MessagePagedResult>(`${this.apiUrl}/messages`, { params: httpParams });
  }

  // Video Call
  joinCall(joinCall: JoinCall): Observable<CallInfo> {
    return this.http.post<CallInfo>(`${this.apiUrl}/videocall/join`, joinCall);
  }

  leaveCall(leaveCall: LeaveCall): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/videocall/leave`, leaveCall);
  }

  getCallInfo(groupId: string): Observable<CallInfo> {
    return this.http.get<CallInfo>(`${this.apiUrl}/videocall/${groupId}/info`);
  }

  updateParticipantStatus(status: UpdateParticipantStatus): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/videocall/participant/status`, status);
  }

  getCallParticipants(groupId: string): Observable<CallParticipant[]> {
    return this.http.get<CallParticipant[]>(`${this.apiUrl}/videocall/${groupId}/participants`);
  }
}
