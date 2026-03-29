import { Injectable } from '@angular/core';
import { Subject, Observable } from 'rxjs';
import { WebRTCSignal, CallParticipant } from '../models';

export interface RemoteStream {
  userId: string;
  userName: string;
  stream: MediaStream;
}

@Injectable({
  providedIn: 'root'
})
export class WebRTCService {
  private peerConnections: Map<string, RTCPeerConnection> = new Map();
  private localStream: MediaStream | undefined;
  private remoteStreamsSubject = new Subject<RemoteStream[]>();
  private localStreamSubject = new Subject<MediaStream | undefined>();
  
  public remoteStreams$ = this.remoteStreamsSubject.asObservable();
  public localStream$ = this.localStreamSubject.asObservable();
  
  private readonly iceServers = [
    { urls: 'stun:stun.l.google.com:19302' },
    { urls: 'stun:stun1.l.google.com:19302' }
  ];

  constructor() {}

  async initializeLocalStream(audio: boolean = true, video: boolean = true): Promise<MediaStream> {
    try {
      const constraints = {
        audio: audio,
        video: video ? { width: 1280, height: 720 } : false
      };

      this.localStream = await navigator.mediaDevices.getUserMedia(constraints);
      this.localStreamSubject.next(this.localStream);
      
      return this.localStream;
    } catch (error) {
      console.error('Error accessing media devices:', error);
      throw error;
    }
  }

  async createPeerConnection(userId: string, isInitiator: boolean = false): Promise<RTCPeerConnection> {
    const pc = new RTCPeerConnection({ iceServers: this.iceServers });
    
    // Add local stream tracks to peer connection
    if (this.localStream) {
      this.localStream.getTracks().forEach(track => {
        pc.addTrack(track, this.localStream!);
      });
    }

    // Handle remote stream
    pc.ontrack = (event) => {
      const remoteStream = event.streams[0];
      if (remoteStream) {
        this.updateRemoteStreams(userId, remoteStream);
      }
    };

    // Handle ICE candidates
    pc.onicecandidate = (event) => {
      if (event.candidate) {
        // This will be sent through SignalR
        this.onIceCandidate(userId, event.candidate);
      }
    };

    // Handle connection state changes
    pc.onconnectionstatechange = () => {
      console.log(`Connection state with ${userId}:`, pc.connectionState);
      
      if (pc.connectionState === 'disconnected' || pc.connectionState === 'failed') {
        this.removePeerConnection(userId);
      }
    };

    this.peerConnections.set(userId, pc);

    if (isInitiator) {
      await this.createOffer(userId);
    }

    return pc;
  }

  private async createOffer(userId: string): Promise<void> {
    const pc = this.peerConnections.get(userId);
    if (!pc) return;

    try {
      const offer = await pc.createOffer();
      await pc.setLocalDescription(offer);
      
      // Send offer through SignalR (this will be handled by the calling component)
      this.onOfferCreated(userId, offer);
    } catch (error) {
      console.error('Error creating offer:', error);
    }
  }

  async handleOffer(userId: string, offer: RTCSessionDescriptionInit): Promise<void> {
    let pc = this.peerConnections.get(userId);
    
    if (!pc) {
      pc = await this.createPeerConnection(userId, false);
    }

    try {
      await pc.setRemoteDescription(offer);
      const answer = await pc.createAnswer();
      await pc.setLocalDescription(answer);
      
      // Send answer through SignalR
      this.onAnswerCreated(userId, answer);
    } catch (error) {
      console.error('Error handling offer:', error);
    }
  }

  async handleAnswer(userId: string, answer: RTCSessionDescriptionInit): Promise<void> {
    const pc = this.peerConnections.get(userId);
    if (!pc) return;

    try {
      await pc.setRemoteDescription(answer);
    } catch (error) {
      console.error('Error handling answer:', error);
    }
  }

  async handleIceCandidate(userId: string, candidate: RTCIceCandidateInit): Promise<void> {
    const pc = this.peerConnections.get(userId);
    if (!pc) return;

    try {
      await pc.addIceCandidate(candidate);
    } catch (error) {
      console.error('Error adding ICE candidate:', error);
    }
  }

  private updateRemoteStreams(userId: string, stream: MediaStream): void {
    const remoteStreams: RemoteStream[] = [];
    
    this.peerConnections.forEach((pc, key) => {
      pc.getReceivers().forEach(receiver => {
        if (receiver.track && receiver.track.readyState === 'live') {
          const existingStream = remoteStreams.find(rs => rs.userId === key);
          if (!existingStream) {
            // Create a new stream for this remote user
            const newStream = new MediaStream();
            newStream.addTrack(receiver.track);
            remoteStreams.push({
              userId: key,
              userName: `User ${key}`, // This should be updated with actual username
              stream: newStream
            });
          }
        }
      });
    });

    this.remoteStreamsSubject.next(remoteStreams);
  }

  private removePeerConnection(userId: string): void {
    const pc = this.peerConnections.get(userId);
    if (pc) {
      pc.close();
      this.peerConnections.delete(userId);
      this.updateRemoteStreams(userId, new MediaStream());
    }
  }

  toggleAudio(): void {
    if (this.localStream) {
      const audioTrack = this.localStream.getAudioTracks()[0];
      if (audioTrack) {
        audioTrack.enabled = !audioTrack.enabled;
      }
    }
  }

  toggleVideo(): void {
    if (this.localStream) {
      const videoTrack = this.localStream.getVideoTracks()[0];
      if (videoTrack) {
        videoTrack.enabled = !videoTrack.enabled;
      }
    }
  }

  isAudioEnabled(): boolean {
    if (this.localStream) {
      const audioTrack = this.localStream.getAudioTracks()[0];
      return audioTrack?.enabled ?? false;
    }
    return false;
  }

  isVideoEnabled(): boolean {
    if (this.localStream) {
      const videoTrack = this.localStream.getVideoTracks()[0];
      return videoTrack?.enabled ?? false;
    }
    return false;
  }

  disconnect(): void {
    // Stop local stream
    if (this.localStream) {
      this.localStream.getTracks().forEach(track => track.stop());
      this.localStream = undefined;
      this.localStreamSubject.next(undefined);
    }

    // Close all peer connections
    this.peerConnections.forEach((pc) => {
      pc.close();
    });
    this.peerConnections.clear();
    this.remoteStreamsSubject.next([]);
  }

  // These methods will be called by the component using SignalR
  private onOfferCreated(userId: string, offer: RTCSessionDescriptionInit): void {
    // This will be implemented in the component
  }

  private onAnswerCreated(userId: string, answer: RTCSessionDescriptionInit): void {
    // This will be implemented in the component
  }

  private onIceCandidate(userId: string, candidate: RTCIceCandidate): void {
    // This will be implemented in the component
  }

  // Setters for event handlers
  setOnOfferCreated(handler: (userId: string, offer: RTCSessionDescriptionInit) => void): void {
    this.onOfferCreated = handler;
  }

  setOnAnswerCreated(handler: (userId: string, answer: RTCSessionDescriptionInit) => void): void {
    this.onAnswerCreated = handler;
  }

  setOnIceCandidate(handler: (userId: string, candidate: RTCIceCandidate) => void): void {
    this.onIceCandidate = handler;
  }
}
