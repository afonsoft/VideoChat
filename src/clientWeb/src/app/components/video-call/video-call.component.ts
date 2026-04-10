import { Component, OnInit, OnDestroy, ViewChild, ElementRef, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { WebRTCService, VideoCallState } from '../../services/webrtc.service';
import { SignalRService } from '../../services/signalr.service';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-video-call',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './video-call.component.html',
  styleUrls: ['./video-call.component.scss']
})
export class VideoCallComponent implements OnInit, OnDestroy {
  @ViewChild('localVideo') localVideoRef!: ElementRef<HTMLVideoElement>;
  @ViewChild('remoteVideoContainer') remoteVideoContainer!: ElementRef<HTMLDivElement>;

  @Input() groupId = '';
  @Input() groupName = '';
  @Input() targetUserId = '';
  @Input() callId = '';
  @Input() isIncoming = false;

  callState: VideoCallState | null = null;
  callDuration = '00:00';
  showIncomingCallModal = false;
  incomingCallData: { callId: string; callerId: string; callerName: string; groupId: string } | null = null;
  isConnecting = false;
  errorMessage = '';

  private subscriptions: Subscription[] = [];
  private callTimer: ReturnType<typeof setInterval> | null = null;
  private callStartTime: Date | null = null;

  constructor(
    private webrtcService: WebRTCService,
    private signalrService: SignalRService,
    private authService: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.setupSignalRListeners();
    this.setupWebRTCListeners();

    const stateSub = this.webrtcService.callState$.subscribe(state => {
      this.callState = state;
    });
    this.subscriptions.push(stateSub);

    if (!this.isIncoming && this.targetUserId) {
      this.initiateCall();
    }
  }

  ngOnDestroy(): void {
    this.subscriptions.forEach(sub => sub.unsubscribe());
    this.stopCallTimer();
  }

  async initiateCall(): Promise<void> {
    this.isConnecting = true;
    this.errorMessage = '';

    try {
      await this.webrtcService.initializeLocalStream();
      this.setLocalVideo();

      await this.signalrService.startVideoCall(this.groupId, this.targetUserId);

      this.webrtcService.startCall(this.callId || crypto.randomUUID(), this.groupId, true);
    } catch (error) {
      this.errorMessage = 'Falha ao iniciar a chamada. Verifique suas permissoes de camera e microfone.';
      this.isConnecting = false;
      console.error('Error initiating call:', error);
    }
  }

  async acceptIncomingCall(): Promise<void> {
    if (!this.incomingCallData) return;

    this.showIncomingCallModal = false;
    this.isConnecting = true;
    this.callId = this.incomingCallData.callId;
    this.groupId = this.incomingCallData.groupId;

    try {
      await this.webrtcService.initializeLocalStream();
      this.setLocalVideo();

      this.webrtcService.startCall(this.callId, this.groupId, false);
      await this.signalrService.acceptVideoCall(this.callId);
    } catch (error) {
      this.errorMessage = 'Falha ao aceitar a chamada.';
      this.isConnecting = false;
      console.error('Error accepting call:', error);
    }
  }

  rejectIncomingCall(): void {
    if (this.incomingCallData) {
      this.signalrService.rejectVideoCall(this.incomingCallData.callId);
      this.showIncomingCallModal = false;
      this.incomingCallData = null;
    }
  }

  async endCall(): Promise<void> {
    const callId = this.callState?.callId || this.callId;
    if (callId) {
      await this.signalrService.endVideoCall(callId);
    }
    this.webrtcService.endCall();
    this.stopCallTimer();
    this.router.navigate(['/chat']);
  }

  toggleMute(): void {
    this.webrtcService.toggleMute();
  }

  toggleCamera(): void {
    this.webrtcService.toggleCamera();
  }

  async toggleScreenShare(): Promise<void> {
    const callId = this.callState?.callId || this.callId;
    if (this.callState?.isScreenSharing) {
      await this.webrtcService.stopScreenShare();
      if (callId) {
        await this.signalrService.stopScreenShare(callId);
      }
    } else {
      const stream = await this.webrtcService.startScreenShare();
      if (stream && callId) {
        await this.signalrService.startScreenShare(callId);
      }
    }
  }

  private setLocalVideo(): void {
    setTimeout(() => {
      if (this.localVideoRef?.nativeElement && this.callState?.localStream) {
        this.localVideoRef.nativeElement.srcObject = this.callState.localStream;
      }
    }, 100);
  }

  private setupSignalRListeners(): void {
    // Incoming video call (via video hub)
    const incomingSub = this.signalrService.incomingVideoCall$.subscribe((data: unknown) => {
      const callData = data as { callId: string; callerId: string; callerName: string; groupId: string };
      this.incomingCallData = callData;
      this.showIncomingCallModal = true;
    });
    this.subscriptions.push(incomingSub);

    // Video call request (via chat hub)
    const requestSub = this.signalrService.videoCallRequest$.subscribe((data: unknown) => {
      const callData = data as { callId: string; callerId: string; callerName: string; groupId: string };
      this.incomingCallData = {
        callId: callData.callId,
        callerId: callData.callerId,
        callerName: callData.callerName,
        groupId: callData.groupId
      };
      this.showIncomingCallModal = true;
    });
    this.subscriptions.push(requestSub);

    // Call accepted
    const acceptedSub = this.signalrService.videoCallAccepted$.subscribe(() => {
      this.isConnecting = false;
      this.startCallTimer();
    });
    this.subscriptions.push(acceptedSub);

    // Call rejected
    const rejectedSub = this.signalrService.videoCallRejected$.subscribe((data: unknown) => {
      const rejData = data as { reason: string };
      this.errorMessage = `Chamada rejeitada: ${rejData.reason || 'Usuario recusou'}`;
      this.isConnecting = false;
      setTimeout(() => this.router.navigate(['/chat']), 3000);
    });
    this.subscriptions.push(rejectedSub);

    // Call ended
    const endedSub = this.signalrService.videoCallEnded$.subscribe(() => {
      this.webrtcService.endCall();
      this.stopCallTimer();
      this.router.navigate(['/chat']);
    });
    this.subscriptions.push(endedSub);

    // Call error
    const errorSub = this.signalrService.videoCallError$.subscribe((data: unknown) => {
      const errData = data as { error: string };
      this.errorMessage = errData.error || 'Erro na chamada';
      this.isConnecting = false;
    });
    this.subscriptions.push(errorSub);

    // WebRTC Exchange start
    const exchangeSub = this.signalrService.startWebRTCExchange$.subscribe(async (data: unknown) => {
      const exchangeData = data as { callId: string; peerUserId: string; isInitiator: boolean };
      this.callId = exchangeData.callId;
      this.isConnecting = false;
      this.startCallTimer();

      if (exchangeData.isInitiator) {
        const pc = await this.webrtcService.createPeerConnection(exchangeData.peerUserId, 'Peer');
        const offer = await this.webrtcService.createOffer(exchangeData.peerUserId);
        await this.signalrService.sendWebRTCOffer(exchangeData.callId, offer);
      }
    });
    this.subscriptions.push(exchangeSub);

    // WebRTC Offer received
    const offerSub = this.signalrService.webrtcOffer$.subscribe(async (data: unknown) => {
      const offerData = data as { callId: string; fromUserId: string; offer: RTCSessionDescriptionInit };
      const answer = await this.webrtcService.handleOffer(offerData.fromUserId, 'Peer', offerData.offer);
      await this.signalrService.sendWebRTCAnswer(offerData.callId, answer);
    });
    this.subscriptions.push(offerSub);

    // WebRTC Answer received
    const answerSub = this.signalrService.webrtcAnswer$.subscribe(async (data: unknown) => {
      const answerData = data as { callId: string; fromUserId: string; answer: RTCSessionDescriptionInit };
      await this.webrtcService.handleAnswer(answerData.fromUserId, answerData.answer);
    });
    this.subscriptions.push(answerSub);

    // ICE Candidate received
    const iceSub = this.signalrService.webrtcIceCandidate$.subscribe(async (data: unknown) => {
      const iceData = data as { callId: string; fromUserId: string; candidate: RTCIceCandidateInit };
      await this.webrtcService.handleIceCandidate(iceData.fromUserId, iceData.candidate);
    });
    this.subscriptions.push(iceSub);
  }

  private setupWebRTCListeners(): void {
    // ICE candidates to send
    const iceSub = this.webrtcService.iceCandidate$.subscribe(async ({ userId, candidate }) => {
      const callId = this.callState?.callId || this.callId;
      if (callId) {
        await this.signalrService.sendWebRTCIceCandidate(callId, candidate.toJSON());
      }
    });
    this.subscriptions.push(iceSub);

    // Remote streams
    const streamSub = this.webrtcService.remoteStream$.subscribe(({ userId, stream }) => {
      this.addRemoteVideo(userId, stream);
    });
    this.subscriptions.push(streamSub);
  }

  private addRemoteVideo(userId: string, stream: MediaStream): void {
    setTimeout(() => {
      if (!this.remoteVideoContainer?.nativeElement) return;

      let videoEl = this.remoteVideoContainer.nativeElement.querySelector(`#remote-video-${userId}`) as HTMLVideoElement;
      if (!videoEl) {
        videoEl = document.createElement('video');
        videoEl.id = `remote-video-${userId}`;
        videoEl.autoplay = true;
        videoEl.playsInline = true;
        videoEl.className = 'remote-video';
        this.remoteVideoContainer.nativeElement.appendChild(videoEl);
      }
      videoEl.srcObject = stream;
    }, 100);
  }

  private startCallTimer(): void {
    this.callStartTime = new Date();
    this.callTimer = setInterval(() => {
      if (this.callStartTime) {
        const elapsed = Math.floor((Date.now() - this.callStartTime.getTime()) / 1000);
        const minutes = Math.floor(elapsed / 60).toString().padStart(2, '0');
        const seconds = (elapsed % 60).toString().padStart(2, '0');
        this.callDuration = `${minutes}:${seconds}`;
      }
    }, 1000);
  }

  private stopCallTimer(): void {
    if (this.callTimer) {
      clearInterval(this.callTimer);
      this.callTimer = null;
    }
    this.callDuration = '00:00';
    this.callStartTime = null;
  }
}
