import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { Subject } from 'rxjs';

import { VideoCallComponent } from './video-call.component';
import { WebRTCService, VideoCallState } from '../../services/webrtc.service';
import { SignalRService } from '../../services/signalr.service';
import { AuthService, User } from '../../services/auth.service';

describe('VideoCallComponent', () => {
  let component: VideoCallComponent;
  let fixture: ComponentFixture<VideoCallComponent>;
  let webrtcServiceSpy: jasmine.SpyObj<WebRTCService>;
  let signalrServiceSpy: jasmine.SpyObj<SignalRService>;
  let authServiceSpy: jasmine.SpyObj<AuthService>;
  let routerSpy: jasmine.SpyObj<Router>;

  let callStateSubject: Subject<VideoCallState>;
  let remoteStreamSubject: Subject<{ userId: string; stream: MediaStream }>;
  let iceCandidateSubject: Subject<{ userId: string; candidate: RTCIceCandidate }>;
  let incomingVideoCallSubject: Subject<unknown>;
  let videoCallRequestSubject: Subject<unknown>;
  let videoCallAcceptedSubject: Subject<unknown>;
  let videoCallRejectedSubject: Subject<unknown>;
  let videoCallEndedSubject: Subject<unknown>;
  let videoCallErrorSubject: Subject<unknown>;
  let startWebRTCExchangeSubject: Subject<unknown>;
  let webrtcOfferSubject: Subject<unknown>;
  let webrtcAnswerSubject: Subject<unknown>;
  let webrtcIceCandidateSubject: Subject<unknown>;

  const mockCallState: VideoCallState = {
    callId: '',
    groupId: '',
    isInCall: false,
    isInitiator: false,
    localStream: null,
    participants: [],
    isMuted: false,
    isCameraOff: false,
    isScreenSharing: false
  };

  beforeEach(async () => {
    callStateSubject = new Subject<VideoCallState>();
    remoteStreamSubject = new Subject();
    iceCandidateSubject = new Subject();
    incomingVideoCallSubject = new Subject();
    videoCallRequestSubject = new Subject();
    videoCallAcceptedSubject = new Subject();
    videoCallRejectedSubject = new Subject();
    videoCallEndedSubject = new Subject();
    videoCallErrorSubject = new Subject();
    startWebRTCExchangeSubject = new Subject();
    webrtcOfferSubject = new Subject();
    webrtcAnswerSubject = new Subject();
    webrtcIceCandidateSubject = new Subject();

    const webrtcSpy = jasmine.createSpyObj('WebRTCService', [
      'initializeLocalStream', 'createPeerConnection', 'createOffer',
      'handleOffer', 'handleAnswer', 'handleIceCandidate',
      'toggleMute', 'toggleCamera', 'startScreenShare', 'stopScreenShare',
      'startCall', 'endCall', 'removePeer'
    ], {
      callState$: callStateSubject.asObservable(),
      remoteStream$: remoteStreamSubject.asObservable(),
      iceCandidate$: iceCandidateSubject.asObservable(),
      currentState: mockCallState
    });

    const signalrSpy = jasmine.createSpyObj('SignalRService', [
      'startVideoCall', 'acceptVideoCall', 'rejectVideoCall', 'endVideoCall',
      'sendWebRTCOffer', 'sendWebRTCAnswer', 'sendWebRTCIceCandidate',
      'startScreenShare', 'stopScreenShare'
    ], {
      incomingVideoCall$: incomingVideoCallSubject.asObservable(),
      videoCallRequest$: videoCallRequestSubject.asObservable(),
      videoCallAccepted$: videoCallAcceptedSubject.asObservable(),
      videoCallRejected$: videoCallRejectedSubject.asObservable(),
      videoCallEnded$: videoCallEndedSubject.asObservable(),
      videoCallError$: videoCallErrorSubject.asObservable(),
      startWebRTCExchange$: startWebRTCExchangeSubject.asObservable(),
      webrtcOffer$: webrtcOfferSubject.asObservable(),
      webrtcAnswer$: webrtcAnswerSubject.asObservable(),
      webrtcIceCandidate$: webrtcIceCandidateSubject.asObservable()
    });

    const authSpy = jasmine.createSpyObj('AuthService', ['logout'], {
      currentUser: { id: '1', email: 'test@test.com', name: 'Test User' }
    });

    const router = jasmine.createSpyObj('Router', ['navigate']);

    await TestBed.configureTestingModule({
      imports: [VideoCallComponent],
      providers: [
        { provide: WebRTCService, useValue: webrtcSpy },
        { provide: SignalRService, useValue: signalrSpy },
        { provide: AuthService, useValue: authSpy },
        { provide: Router, useValue: router }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(VideoCallComponent);
    component = fixture.componentInstance;
    webrtcServiceSpy = TestBed.inject(WebRTCService) as jasmine.SpyObj<WebRTCService>;
    signalrServiceSpy = TestBed.inject(SignalRService) as jasmine.SpyObj<SignalRService>;
    authServiceSpy = TestBed.inject(AuthService) as jasmine.SpyObj<AuthService>;
    routerSpy = TestBed.inject(Router) as jasmine.SpyObj<Router>;
  });

  it('should create', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });

  it('should initialize with default state', () => {
    fixture.detectChanges();
    expect(component.callDuration).toBe('00:00');
    expect(component.showIncomingCallModal).toBe(false);
    expect(component.isConnecting).toBe(false);
    expect(component.errorMessage).toBe('');
  });

  it('should subscribe to call state on init', () => {
    fixture.detectChanges();
    callStateSubject.next(mockCallState);
    expect(component.callState).toEqual(mockCallState);
  });

  describe('endCall', () => {
    it('should call webrtc endCall and navigate to chat', async () => {
      component.callId = 'call-123';
      signalrServiceSpy.endVideoCall.and.returnValue(Promise.resolve());
      fixture.detectChanges();

      await component.endCall();

      expect(signalrServiceSpy.endVideoCall).toHaveBeenCalledWith('call-123');
      expect(webrtcServiceSpy.endCall).toHaveBeenCalled();
      expect(routerSpy.navigate).toHaveBeenCalledWith(['/chat']);
    });
  });

  describe('toggleMute', () => {
    it('should call webrtc toggleMute', () => {
      fixture.detectChanges();
      component.toggleMute();
      expect(webrtcServiceSpy.toggleMute).toHaveBeenCalled();
    });
  });

  describe('toggleCamera', () => {
    it('should call webrtc toggleCamera', () => {
      fixture.detectChanges();
      component.toggleCamera();
      expect(webrtcServiceSpy.toggleCamera).toHaveBeenCalled();
    });
  });

  describe('incoming call', () => {
    it('should show incoming call modal on incoming video call', () => {
      fixture.detectChanges();
      const callData = { callId: 'call-1', callerId: 'user-2', callerName: 'John', groupId: 'group-1' };
      incomingVideoCallSubject.next(callData);

      expect(component.showIncomingCallModal).toBe(true);
      expect(component.incomingCallData).toEqual(callData);
    });

    it('should show incoming call modal on video call request', () => {
      fixture.detectChanges();
      const callData = { callId: 'call-2', callerId: 'user-3', callerName: 'Jane', groupId: 'group-2' };
      videoCallRequestSubject.next(callData);

      expect(component.showIncomingCallModal).toBe(true);
      expect(component.incomingCallData?.callerName).toBe('Jane');
    });

    it('should reject incoming call', () => {
      fixture.detectChanges();
      component.incomingCallData = { callId: 'call-1', callerId: 'user-2', callerName: 'John', groupId: 'group-1' };
      component.showIncomingCallModal = true;

      signalrServiceSpy.rejectVideoCall.and.returnValue(Promise.resolve());
      component.rejectIncomingCall();

      expect(signalrServiceSpy.rejectVideoCall).toHaveBeenCalledWith('call-1');
      expect(component.showIncomingCallModal).toBe(false);
      expect(component.incomingCallData).toBeNull();
    });
  });

  describe('call events', () => {
    it('should handle call rejected event', () => {
      fixture.detectChanges();
      videoCallRejectedSubject.next({ reason: 'User busy' });

      expect(component.errorMessage).toContain('Chamada rejeitada');
      expect(component.isConnecting).toBe(false);
    });

    it('should handle call ended event', () => {
      fixture.detectChanges();
      videoCallEndedSubject.next({});

      expect(webrtcServiceSpy.endCall).toHaveBeenCalled();
      expect(routerSpy.navigate).toHaveBeenCalledWith(['/chat']);
    });

    it('should handle call error event', () => {
      fixture.detectChanges();
      videoCallErrorSubject.next({ error: 'Connection failed' });

      expect(component.errorMessage).toBe('Connection failed');
      expect(component.isConnecting).toBe(false);
    });
  });

  describe('cleanup', () => {
    it('should unsubscribe on destroy', () => {
      fixture.detectChanges();
      component.ngOnDestroy();
      // Should not throw
    });
  });
});
