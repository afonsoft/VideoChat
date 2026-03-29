namespace SimpleConnect.Mobile.Models;

public class WebRTCSignal
{
    public string Type { get; set; } = string.Empty; // offer, answer, ice-candidate
    public string Sdp { get; set; } = string.Empty;
    public string Candidate { get; set; } = string.Empty;
    public string SdpMid { get; set; } = string.Empty;
    public int? SdpMLineIndex { get; set; }
    public Guid FromUserId { get; set; }
    public Guid ToUserId { get; set; }
    public string RoomId { get; set; } = string.Empty;
}

public class CallInfo
{
    public Guid GroupId { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public List<CallParticipant> Participants { get; set; } = new();
    public bool IsActive { get; set; }
    public DateTime StartedAt { get; set; }
    public int MaxParticipants { get; set; }
}
