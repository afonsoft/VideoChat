using System;
using System.Collections.Generic;
using Volo.Abp.Domain.Entities.Auditing;

namespace FamilyMeet.Domain.Chat
{
    public class ChatGroup : FullAuditedAggregateRoot<Guid>
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Avatar { get; set; }
        public bool IsPrivate { get; set; }
        public bool IsActive { get; set; }
        public Guid CreatedById { get; set; }
        public DateTime? LastMessageAt { get; set; }
        public int MemberCount { get; set; }
        public string GroupType { get; set; } // Family, Friends, Work, etc.
        public string Settings { get; set; } // JSON string for group settings

        // Navigation properties
        public virtual ICollection<ChatGroupMember> Members { get; set; }
        public virtual ICollection<ChatMessage> Messages { get; set; }

        protected ChatGroup()
        {
            Members = new List<ChatGroupMember>();
            Messages = new List<ChatMessage>();
        }

        public ChatGroup(
            Guid id,
            string name,
            string description,
            bool isPrivate,
            Guid createdById,
            string groupType = "Family"
        ) : base(id)
        {
            Name = name;
            Description = description;
            IsPrivate = isPrivate;
            CreatedById = createdById;
            GroupType = groupType;
            IsActive = true;
            MemberCount = 0;
            Members = new List<ChatGroupMember>();
            Messages = new List<ChatMessage>();
        }

        public void UpdateInfo(string name, string description, string avatar)
        {
            Name = name;
            Description = description;
            Avatar = avatar;
        }

        public void AddMember(Guid userId, string role = "Member")
        {
            if (!HasMember(userId))
            {
                var member = new ChatGroupMember(Guid.NewGuid(), Id, userId, role);
                Members.Add(member);
                MemberCount++;
            }
        }

        public void RemoveMember(Guid userId)
        {
            var member = GetMember(userId);
            if (member != null)
            {
                Members.Remove(member);
                MemberCount--;
            }
        }

        public bool HasMember(Guid userId)
        {
            return Members.Any(m => m.UserId == userId);
        }

        public ChatGroupMember GetMember(Guid userId)
        {
            return Members.FirstOrDefault(m => m.UserId == userId);
        }

        public void UpdateMemberRole(Guid userId, string role)
        {
            var member = GetMember(userId);
            if (member != null)
            {
                member.UpdateRole(role);
            }
        }

        public bool IsAdmin(Guid userId)
        {
            var member = GetMember(userId);
            return member != null && member.Role == "Admin";
        }

        public bool IsOwner(Guid userId)
        {
            var member = GetMember(userId);
            return member != null && member.Role == "Owner";
        }

        public void UpdateLastMessageTime()
        {
            LastMessageAt = DateTime.UtcNow;
        }

        public void Activate()
        {
            IsActive = true;
        }

        public void Deactivate()
        {
            IsActive = false;
        }

        public void UpdateSettings(string settings)
        {
            Settings = settings;
        }

        public List<Guid> GetMemberIds()
        {
            return Members.Select(m => m.UserId).ToList();
        }

        public List<ChatGroupMember> GetActiveMembers()
        {
            return Members.Where(m => m.IsActive).ToList();
        }

        public bool CanUserJoin(Guid userId)
        {
            return IsActive && !HasMember(userId) && (!IsPrivate || IsInvited(userId));
        }

        private bool IsInvited(Guid userId)
        {
            // Implementation depends on invitation system
            return false;
        }
    }
}
