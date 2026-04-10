import { TestBed } from '@angular/core/testing';
import { SignalRService } from './signalr.service';

describe('SignalRService', () => {
  let service: SignalRService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [SignalRService]
    });
    service = TestBed.inject(SignalRService);
  });

  afterEach(async () => {
    await service.disconnectAll();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('initial state', () => {
    it('should have disconnected connection status', (done) => {
      service.connectionStatus$.subscribe(status => {
        expect(status).toBe('disconnected');
        done();
      });
    });

    it('should have disconnected video connection status', (done) => {
      service.videoConnectionStatus$.subscribe(status => {
        expect(status).toBe('disconnected');
        done();
      });
    });
  });

  describe('observables', () => {
    it('should expose messageReceived$ observable', () => {
      expect(service.messageReceived$).toBeDefined();
    });

    it('should expose messageEdited$ observable', () => {
      expect(service.messageEdited$).toBeDefined();
    });

    it('should expose messageDeleted$ observable', () => {
      expect(service.messageDeleted$).toBeDefined();
    });

    it('should expose userJoined$ observable', () => {
      expect(service.userJoined$).toBeDefined();
    });

    it('should expose userLeft$ observable', () => {
      expect(service.userLeft$).toBeDefined();
    });

    it('should expose userTyping$ observable', () => {
      expect(service.userTyping$).toBeDefined();
    });

    it('should expose videoCallRequest$ observable', () => {
      expect(service.videoCallRequest$).toBeDefined();
    });

    it('should expose videoCallAccepted$ observable', () => {
      expect(service.videoCallAccepted$).toBeDefined();
    });

    it('should expose videoCallRejected$ observable', () => {
      expect(service.videoCallRejected$).toBeDefined();
    });

    it('should expose videoCallEnded$ observable', () => {
      expect(service.videoCallEnded$).toBeDefined();
    });

    it('should expose webrtcOffer$ observable', () => {
      expect(service.webrtcOffer$).toBeDefined();
    });

    it('should expose webrtcAnswer$ observable', () => {
      expect(service.webrtcAnswer$).toBeDefined();
    });

    it('should expose webrtcIceCandidate$ observable', () => {
      expect(service.webrtcIceCandidate$).toBeDefined();
    });

    it('should expose incomingVideoCall$ observable', () => {
      expect(service.incomingVideoCall$).toBeDefined();
    });

    it('should expose screenShareStarted$ observable', () => {
      expect(service.screenShareStarted$).toBeDefined();
    });

    it('should expose screenShareStopped$ observable', () => {
      expect(service.screenShareStopped$).toBeDefined();
    });
  });

  describe('disconnectAll', () => {
    it('should set connection status to disconnected', async () => {
      await service.disconnectAll();

      service.connectionStatus$.subscribe(status => {
        expect(status).toBe('disconnected');
      });
    });

    it('should set video connection status to disconnected', async () => {
      await service.disconnectAll();

      service.videoConnectionStatus$.subscribe(status => {
        expect(status).toBe('disconnected');
      });
    });
  });

  describe('hub methods (without connection)', () => {
    it('should not throw when sendMessage is called without connection', async () => {
      await service.sendMessage('group-1', 'test message');
    });

    it('should not throw when joinGroup is called without connection', async () => {
      await service.joinGroup('group-1');
    });

    it('should not throw when leaveGroup is called without connection', async () => {
      await service.leaveGroup('group-1');
    });

    it('should not throw when setTyping is called without connection', async () => {
      await service.setTyping('group-1', true);
    });

    it('should not throw when startVideoCall is called without connection', async () => {
      await service.startVideoCall('group-1', 'user-1');
    });

    it('should not throw when acceptVideoCall is called without connection', async () => {
      await service.acceptVideoCall('call-1');
    });

    it('should not throw when rejectVideoCall is called without connection', async () => {
      await service.rejectVideoCall('call-1', 'busy');
    });

    it('should not throw when endVideoCall is called without connection', async () => {
      await service.endVideoCall('call-1');
    });

    it('should not throw when sendWebRTCOffer is called without connection', async () => {
      await service.sendWebRTCOffer('call-1', { type: 'offer', sdp: 'test' });
    });

    it('should not throw when sendWebRTCAnswer is called without connection', async () => {
      await service.sendWebRTCAnswer('call-1', { type: 'answer', sdp: 'test' });
    });

    it('should not throw when sendWebRTCIceCandidate is called without connection', async () => {
      await service.sendWebRTCIceCandidate('call-1', { candidate: 'test', sdpMid: '0', sdpMLineIndex: 0 });
    });

    it('should not throw when startScreenShare is called without connection', async () => {
      await service.startScreenShare('call-1');
    });

    it('should not throw when stopScreenShare is called without connection', async () => {
      await service.stopScreenShare('call-1');
    });
  });

  describe('ngOnDestroy', () => {
    it('should call disconnectAll on destroy', async () => {
      spyOn(service, 'disconnectAll').and.callThrough();
      service.ngOnDestroy();
    });
  });
});
