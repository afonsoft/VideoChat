import { Injectable, OnDestroy } from '@angular/core';
import { BehaviorSubject, Observable, Subject } from 'rxjs';

export interface PeerConnectionState {
  userId: string;
  userName: string;
  stream: MediaStream | null;
  connectionState: RTCPeerConnectionState;
  hasAudio: boolean;
  hasVideo: boolean;
}

export interface VideoCallState {
  callId: string;
  groupId: string;
  isInCall: boolean;
  isInitiator: boolean;
  localStream: MediaStream | null;
  participants: PeerConnectionState[];
  isMuted: boolean;
  isCameraOff: boolean;
  isScreenSharing: boolean;
}

const ICE_SERVERS: RTCConfiguration = {
  iceServers: [
    { urls: 'stun:stun.l.google.com:19302' },
    { urls: 'stun:stun1.l.google.com:19302' },
    { urls: 'stun:stun2.l.google.com:19302' }
  ]
};

@Injectable({
  providedIn: 'root'
})
export class WebRTCService implements OnDestroy {
  private peerConnections = new Map<string, RTCPeerConnection>();
  private localStream: MediaStream | null = null;
  private screenStream: MediaStream | null = null;

  private callStateSubject = new BehaviorSubject<VideoCallState>({
    callId: '',
    groupId: '',
    isInCall: false,
    isInitiator: false,
    localStream: null,
    participants: [],
    isMuted: false,
    isCameraOff: false,
    isScreenSharing: false
  });

  private remoteStreamSubject = new Subject<{ userId: string; stream: MediaStream }>();
  private iceCandidateSubject = new Subject<{ userId: string; candidate: RTCIceCandidate }>();
  private offerSubject = new Subject<{ userId: string; offer: RTCSessionDescriptionInit }>();
  private answerSubject = new Subject<{ userId: string; answer: RTCSessionDescriptionInit }>();

  callState$ = this.callStateSubject.asObservable();
  remoteStream$ = this.remoteStreamSubject.asObservable();
  iceCandidate$ = this.iceCandidateSubject.asObservable();
  offer$ = this.offerSubject.asObservable();
  answer$ = this.answerSubject.asObservable();

  get currentState(): VideoCallState {
    return this.callStateSubject.value;
  }

  async initializeLocalStream(videoEnabled = true, audioEnabled = true): Promise<MediaStream> {
    try {
      this.localStream = await navigator.mediaDevices.getUserMedia({
        video: videoEnabled ? { width: { ideal: 1280 }, height: { ideal: 720 }, facingMode: 'user' } : false,
        audio: audioEnabled ? { echoCancellation: true, noiseSuppression: true, autoGainControl: true } : false
      });

      this.updateState({ localStream: this.localStream });
      return this.localStream;
    } catch (error) {
      console.error('Error accessing media devices:', error);
      throw error;
    }
  }

  async createPeerConnection(userId: string, userName: string): Promise<RTCPeerConnection> {
    if (this.peerConnections.has(userId)) {
      this.peerConnections.get(userId)?.close();
    }

    const pc = new RTCPeerConnection(ICE_SERVERS);

    if (this.localStream) {
      this.localStream.getTracks().forEach(track => {
        pc.addTrack(track, this.localStream!);
      });
    }

    pc.ontrack = (event: RTCTrackEvent) => {
      if (event.streams && event.streams[0]) {
        this.remoteStreamSubject.next({ userId, stream: event.streams[0] });
        this.updateParticipant(userId, { stream: event.streams[0] });
      }
    };

    pc.onicecandidate = (event: RTCPeerConnectionIceEvent) => {
      if (event.candidate) {
        this.iceCandidateSubject.next({ userId, candidate: event.candidate });
      }
    };

    pc.onconnectionstatechange = () => {
      this.updateParticipant(userId, { connectionState: pc.connectionState });

      if (pc.connectionState === 'failed' || pc.connectionState === 'disconnected') {
        console.warn(`Peer connection with ${userId} ${pc.connectionState}`);
      }
    };

    pc.oniceconnectionstatechange = () => {
      console.log(`ICE connection state for ${userId}: ${pc.iceConnectionState}`);
    };

    this.peerConnections.set(userId, pc);

    const currentState = this.callStateSubject.value;
    const existingParticipant = currentState.participants.find(p => p.userId === userId);
    if (!existingParticipant) {
      this.updateState({
        participants: [...currentState.participants, {
          userId,
          userName,
          stream: null,
          connectionState: 'new',
          hasAudio: true,
          hasVideo: true
        }]
      });
    }

    return pc;
  }

  async createOffer(userId: string): Promise<RTCSessionDescriptionInit> {
    const pc = this.peerConnections.get(userId);
    if (!pc) {
      throw new Error(`No peer connection found for user ${userId}`);
    }

    const offer = await pc.createOffer({
      offerToReceiveAudio: true,
      offerToReceiveVideo: true
    });

    await pc.setLocalDescription(offer);
    return offer;
  }

  async handleOffer(userId: string, userName: string, offer: RTCSessionDescriptionInit): Promise<RTCSessionDescriptionInit> {
    const pc = await this.createPeerConnection(userId, userName);

    await pc.setRemoteDescription(new RTCSessionDescription(offer));

    const answer = await pc.createAnswer();
    await pc.setLocalDescription(answer);

    return answer;
  }

  async handleAnswer(userId: string, answer: RTCSessionDescriptionInit): Promise<void> {
    const pc = this.peerConnections.get(userId);
    if (!pc) {
      throw new Error(`No peer connection found for user ${userId}`);
    }

    await pc.setRemoteDescription(new RTCSessionDescription(answer));
  }

  async handleIceCandidate(userId: string, candidate: RTCIceCandidateInit): Promise<void> {
    const pc = this.peerConnections.get(userId);
    if (!pc) {
      console.warn(`No peer connection found for user ${userId}, queueing ICE candidate`);
      return;
    }

    try {
      await pc.addIceCandidate(new RTCIceCandidate(candidate));
    } catch (error) {
      console.error('Error adding ICE candidate:', error);
    }
  }

  toggleMute(): boolean {
    if (!this.localStream) return false;

    const audioTracks = this.localStream.getAudioTracks();
    const isMuted = !this.callStateSubject.value.isMuted;

    audioTracks.forEach(track => {
      track.enabled = !isMuted;
    });

    this.updateState({ isMuted });
    return isMuted;
  }

  toggleCamera(): boolean {
    if (!this.localStream) return false;

    const videoTracks = this.localStream.getVideoTracks();
    const isCameraOff = !this.callStateSubject.value.isCameraOff;

    videoTracks.forEach(track => {
      track.enabled = !isCameraOff;
    });

    this.updateState({ isCameraOff });
    return isCameraOff;
  }

  async startScreenShare(): Promise<MediaStream | null> {
    try {
      this.screenStream = await navigator.mediaDevices.getDisplayMedia({
        video: { width: { ideal: 1920 }, height: { ideal: 1080 } },
        audio: false
      });

      const screenTrack = this.screenStream.getVideoTracks()[0];

      this.peerConnections.forEach((pc) => {
        const sender = pc.getSenders().find(s => s.track?.kind === 'video');
        if (sender) {
          sender.replaceTrack(screenTrack);
        }
      });

      screenTrack.onended = () => {
        this.stopScreenShare();
      };

      this.updateState({ isScreenSharing: true });
      return this.screenStream;
    } catch (error) {
      console.error('Error starting screen share:', error);
      return null;
    }
  }

  async stopScreenShare(): Promise<void> {
    if (this.screenStream) {
      this.screenStream.getTracks().forEach(track => track.stop());
      this.screenStream = null;
    }

    if (this.localStream) {
      const videoTrack = this.localStream.getVideoTracks()[0];
      if (videoTrack) {
        this.peerConnections.forEach((pc) => {
          const sender = pc.getSenders().find(s => s.track?.kind === 'video');
          if (sender) {
            sender.replaceTrack(videoTrack);
          }
        });
      }
    }

    this.updateState({ isScreenSharing: false });
  }

  startCall(callId: string, groupId: string, isInitiator: boolean): void {
    this.updateState({
      callId,
      groupId,
      isInCall: true,
      isInitiator
    });
  }

  endCall(): void {
    this.peerConnections.forEach((pc) => {
      pc.close();
    });
    this.peerConnections.clear();

    if (this.localStream) {
      this.localStream.getTracks().forEach(track => track.stop());
      this.localStream = null;
    }

    if (this.screenStream) {
      this.screenStream.getTracks().forEach(track => track.stop());
      this.screenStream = null;
    }

    this.callStateSubject.next({
      callId: '',
      groupId: '',
      isInCall: false,
      isInitiator: false,
      localStream: null,
      participants: [],
      isMuted: false,
      isCameraOff: false,
      isScreenSharing: false
    });
  }

  removePeer(userId: string): void {
    const pc = this.peerConnections.get(userId);
    if (pc) {
      pc.close();
      this.peerConnections.delete(userId);
    }

    const currentState = this.callStateSubject.value;
    this.updateState({
      participants: currentState.participants.filter(p => p.userId !== userId)
    });
  }

  private updateState(partial: Partial<VideoCallState>): void {
    this.callStateSubject.next({
      ...this.callStateSubject.value,
      ...partial
    });
  }

  private updateParticipant(userId: string, update: Partial<PeerConnectionState>): void {
    const currentState = this.callStateSubject.value;
    const participants = currentState.participants.map(p =>
      p.userId === userId ? { ...p, ...update } : p
    );
    this.updateState({ participants });
  }

  ngOnDestroy(): void {
    this.endCall();
  }
}
