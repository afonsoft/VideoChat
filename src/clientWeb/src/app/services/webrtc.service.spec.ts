import { TestBed } from '@angular/core/testing';
import { WebRTCService, VideoCallState } from './webrtc.service';

describe('WebRTCService', () => {
  let service: WebRTCService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [WebRTCService]
    });
    service = TestBed.inject(WebRTCService);
  });

  afterEach(() => {
    service.endCall();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('initial state', () => {
    it('should have default call state', () => {
      const state = service.currentState;
      expect(state.isInCall).toBe(false);
      expect(state.callId).toBe('');
      expect(state.groupId).toBe('');
      expect(state.isInitiator).toBe(false);
      expect(state.localStream).toBeNull();
      expect(state.participants).toEqual([]);
      expect(state.isMuted).toBe(false);
      expect(state.isCameraOff).toBe(false);
      expect(state.isScreenSharing).toBe(false);
    });

    it('should emit initial state via observable', (done) => {
      service.callState$.subscribe(state => {
        expect(state.isInCall).toBe(false);
        done();
      });
    });
  });

  describe('startCall', () => {
    it('should update call state when starting a call', (done) => {
      service.startCall('call-123', 'group-456', true);

      service.callState$.subscribe(state => {
        expect(state.callId).toBe('call-123');
        expect(state.groupId).toBe('group-456');
        expect(state.isInCall).toBe(true);
        expect(state.isInitiator).toBe(true);
        done();
      });
    });

    it('should set isInitiator to false for non-initiators', (done) => {
      service.startCall('call-789', 'group-012', false);

      service.callState$.subscribe(state => {
        expect(state.isInitiator).toBe(false);
        done();
      });
    });
  });

  describe('endCall', () => {
    it('should reset call state when ending a call', (done) => {
      service.startCall('call-123', 'group-456', true);
      service.endCall();

      service.callState$.subscribe(state => {
        expect(state.isInCall).toBe(false);
        expect(state.callId).toBe('');
        expect(state.groupId).toBe('');
        expect(state.localStream).toBeNull();
        expect(state.participants).toEqual([]);
        done();
      });
    });
  });

  describe('toggleMute', () => {
    it('should return false when no local stream', () => {
      const result = service.toggleMute();
      expect(result).toBe(false);
    });
  });

  describe('toggleCamera', () => {
    it('should return false when no local stream', () => {
      const result = service.toggleCamera();
      expect(result).toBe(false);
    });
  });

  describe('createPeerConnection', () => {
    it('should create a peer connection and add participant', (done) => {
      service.createPeerConnection('user-1', 'Test User').then(pc => {
        expect(pc).toBeTruthy();
        expect(pc instanceof RTCPeerConnection).toBe(true);

        service.callState$.subscribe(state => {
          const participant = state.participants.find(p => p.userId === 'user-1');
          expect(participant).toBeTruthy();
          expect(participant?.userName).toBe('Test User');
          expect(participant?.stream).toBeNull();
          done();
        });
      });
    });
  });

  describe('removePeer', () => {
    it('should remove peer from participants', async () => {
      await service.createPeerConnection('user-1', 'User 1');
      await service.createPeerConnection('user-2', 'User 2');

      service.removePeer('user-1');

      const state = service.currentState;
      expect(state.participants.length).toBe(1);
      expect(state.participants[0].userId).toBe('user-2');
    });
  });

  describe('createOffer', () => {
    it('should throw error when no peer connection exists', async () => {
      try {
        await service.createOffer('non-existent-user');
        fail('Should have thrown an error');
      } catch (error) {
        expect((error as Error).message).toContain('No peer connection found');
      }
    });

    it('should create an offer for existing peer connection', async () => {
      await service.createPeerConnection('user-1', 'User 1');
      const offer = await service.createOffer('user-1');
      expect(offer).toBeTruthy();
      expect(offer.type).toBe('offer');
      expect(offer.sdp).toBeDefined();
    });
  });

  describe('handleAnswer', () => {
    it('should throw error when no peer connection exists', async () => {
      try {
        await service.handleAnswer('non-existent-user', { type: 'answer', sdp: 'test' });
        fail('Should have thrown an error');
      } catch (error) {
        expect((error as Error).message).toContain('No peer connection found');
      }
    });
  });

  describe('handleIceCandidate', () => {
    it('should not throw when no peer connection exists (queues candidate)', async () => {
      // Should not throw, just warn
      await service.handleIceCandidate('non-existent-user', {
        candidate: 'test',
        sdpMid: '0',
        sdpMLineIndex: 0
      });
    });
  });

  describe('iceCandidate$ observable', () => {
    it('should emit ICE candidates', (done) => {
      service.iceCandidate$.subscribe(({ userId, candidate }) => {
        expect(userId).toBeDefined();
        expect(candidate).toBeTruthy();
        done();
      });

      // Trigger by creating a connection - this may emit candidates
      service.createPeerConnection('user-1', 'User 1');
    });
  });

  describe('ngOnDestroy', () => {
    it('should clean up on destroy', () => {
      service.startCall('call-1', 'group-1', true);
      service.ngOnDestroy();

      const state = service.currentState;
      expect(state.isInCall).toBe(false);
      expect(state.participants).toEqual([]);
    });
  });
});
