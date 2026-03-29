namespace SimpleConnect.Domain.Shared.ValueObjects;

public record WebRTCMessage
{
    public string Type { get; init; } = string.Empty; // offer, answer, ice-candidate
    public string Sdp { get; init; } = string.Empty;
    public string Candidate { get; init; } = string.Empty;
    public string SdpMid { get; init; } = string.Empty;
    public int? SdpMLineIndex { get; init; }
    public Guid FromUserId { get; init; }
    public Guid ToUserId { get; init; }
    public string RoomId { get; init; } = string.Empty;
    
    public WebRTCMessage(string type, Guid fromUserId, Guid toUserId, string roomId)
    {
        Type = type;
        FromUserId = fromUserId;
        ToUserId = toUserId;
        RoomId = roomId;
    }
    
    public static WebRTCMessage CreateOffer(string sdp, Guid fromUserId, Guid toUserId, string roomId)
    {
        return new WebRTCMessage("offer", fromUserId, toUserId, roomId) { Sdp = sdp };
    }
    
    public static WebRTCMessage CreateAnswer(string sdp, Guid fromUserId, Guid toUserId, string roomId)
    {
        return new WebRTCMessage("answer", fromUserId, toUserId, roomId) { Sdp = sdp };
    }
    
    public static WebRTCMessage CreateIceCandidate(string candidate, string sdpMid, int sdpMLineIndex, Guid fromUserId, Guid toUserId, string roomId)
    {
        return new WebRTCMessage("ice-candidate", fromUserId, toUserId, roomId)
        {
            Candidate = candidate,
            SdpMid = sdpMid,
            SdpMLineIndex = sdpMLineIndex
        };
    }
}
