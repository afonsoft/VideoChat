using System.ComponentModel.DataAnnotations;
using SimpleConnect.Domain.Shared.Constants;
using SimpleConnect.Domain.Shared.Enums;
using SimpleConnect.Domain.Shared.ValueObjects;

namespace SimpleConnect.Domain.Entities;

public class ChatGroup
{
    public Guid Id { get; private set; }
    
    [Required]
    [MaxLength(SimpleConnectConsts.MaxGroupNameLength)]
    public string Name { get; private set; } = string.Empty;
    
    public string Description { get; private set; } = string.Empty;
    
    public GroupType Type { get; private set; }
    
    public Guid CreatorId { get; private set; }
    
    public DateTime CreatedAt { get; private set; }
    
    public DateTime? LastActivityAt { get; private set; }
    
    public bool IsActive { get; private set; }
    
    public int MaxParticipants { get; private set; }
    
    private readonly List<ChatGroupMember> _members = new();
    public IReadOnlyCollection<ChatGroupMember> Members => _members.AsReadOnly();
    
    private readonly List<ChatMessage> _messages = new();
    public IReadOnlyCollection<ChatMessage> Messages => _messages.AsReadOnly();
    
    private readonly List<CallParticipant> _activeCallParticipants = new();
    public IReadOnlyCollection<CallParticipant> ActiveCallParticipants => _activeCallParticipants.AsReadOnly();
    
    public ChatGroup()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
        IsActive = true;
        MaxParticipants = SimpleConnectConsts.MaxVideoCallParticipants;
    }
    
    public ChatGroup(string name, string description, GroupType type, Guid creatorId, int maxParticipants = SimpleConnectConsts.MaxVideoCallParticipants)
        : this()
    {
        Name = name;
        Description = description;
        Type = type;
        CreatorId = creatorId;
        MaxParticipants = maxParticipants;
        
        AddMember(creatorId, "Creator", isCreator: true);
    }
    
    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Group name cannot be empty", nameof(name));
            
        if (name.Length > SimpleConnectConsts.MaxGroupNameLength)
            throw new ArgumentException($"Group name cannot exceed {SimpleConnectConsts.MaxGroupNameLength} characters", nameof(name));
            
        Name = name;
        UpdateLastActivity();
    }
    
    public void AddMember(Guid userId, string userName, bool isCreator = false)
    {
        if (_members.Any(m => m.UserId == userId))
            throw new InvalidOperationException("User is already a member of this group");
            
        if (_members.Count >= MaxParticipants)
            throw new InvalidOperationException($"Group cannot have more than {MaxParticipants} participants");
            
        var member = new ChatGroupMember(userId, userName, isCreator);
        _members.Add(member);
        UpdateLastActivity();
    }
    
    public void RemoveMember(Guid userId)
    {
        var member = _members.FirstOrDefault(m => m.UserId == userId);
        if (member == null)
            throw new InvalidOperationException("User is not a member of this group");
            
        if (member.IsCreator && _members.Count(m => m.IsCreator) == 1)
            throw new InvalidOperationException("Cannot remove the creator from the group");
            
        _members.Remove(member);
        
        var callParticipant = _activeCallParticipants.FirstOrDefault(p => p.UserId == userId);
        if (callParticipant != null)
            _activeCallParticipants.Remove(callParticipant);
            
        UpdateLastActivity();
    }
    
    public void AddCallParticipant(CallParticipant participant)
    {
        if (_activeCallParticipants.Any(p => p.UserId == participant.UserId))
        {
            var existing = _activeCallParticipants.First(p => p.UserId == participant.UserId);
            _activeCallParticipants.Remove(existing);
        }
        
        if (_activeCallParticipants.Count >= MaxParticipants)
            throw new InvalidOperationException($"Video call cannot have more than {MaxParticipants} participants");
            
        _activeCallParticipants.Add(participant);
        UpdateLastActivity();
    }
    
    public void RemoveCallParticipant(Guid userId)
    {
        var participant = _activeCallParticipants.FirstOrDefault(p => p.UserId == userId);
        if (participant != null)
        {
            _activeCallParticipants.Remove(participant);
            UpdateLastActivity();
        }
    }
    
    public void UpdateParticipantStatus(Guid userId, ParticipantStatus status)
    {
        var participant = _activeCallParticipants.FirstOrDefault(p => p.UserId == userId);
        if (participant != null)
        {
            _activeCallParticipants.Remove(participant);
            _activeCallParticipants.Add(participant.WithStatus(status));
            UpdateLastActivity();
        }
    }
    
    public bool IsMember(Guid userId)
    {
        return _members.Any(m => m.UserId == userId);
    }
    
    public bool CanUserJoinCall(Guid userId)
    {
        return IsMember(userId) && 
               _activeCallParticipants.Count < MaxParticipants &&
               !_activeCallParticipants.Any(p => p.UserId == userId);
    }
    
    private void UpdateLastActivity()
    {
        LastActivityAt = DateTime.UtcNow;
    }
    
    public void Deactivate()
    {
        IsActive = false;
        _activeCallParticipants.Clear();
        UpdateLastActivity();
    }
}
