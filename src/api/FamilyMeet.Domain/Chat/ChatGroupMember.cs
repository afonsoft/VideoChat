using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace FamilyMeet.Domain.Chat
{
    public class ChatGroupMember : FullAuditedEntity<Guid>
    {
        public Guid ChatGroupId { get; set; }
        public Guid UserId { get; set; }
        public string Role { get; set; } = "Member"; // Owner, Admin, Member
        public bool IsActive { get; set; }
        public DateTime? JoinedAt { get; set; }
        public DateTime? LastSeenAt { get; set; }
        public string Nickname { get; set; } = string.Empty;

        protected ChatGroupMember()
        {
        }

        public ChatGroupMember(
            Guid id,
            Guid chatGroupId,
            Guid userId,
            string role = "Member"
        ) : base(id)
        {
            ChatGroupId = chatGroupId;
            UserId = userId;
            Role = role;
            IsActive = true;
            JoinedAt = DateTime.UtcNow;
        }

        public void UpdateRole(string role)
        {
            Role = role;
        }

        public void Activate()
        {
            IsActive = true;
        }

        public void Deactivate()
        {
            IsActive = false;
        }

        public void UpdateLastSeen()
        {
            LastSeenAt = DateTime.UtcNow;
        }

        public void UpdateNickname(string nickname)
        {
            Nickname = nickname;
        }

        public bool IsOwner()
        {
            return Role == "Owner";
        }

        public bool IsAdmin()
        {
            return Role == "Admin" || Role == "Owner";
        }

        public bool CanManageGroup()
        {
            return Role == "Owner" || Role == "Admin";
        }

        public bool CanKickMembers()
        {
            return Role == "Owner" || Role == "Admin";
        }

        public bool CanManageMessages()
        {
            return Role == "Owner" || Role == "Admin";
        }
    }
}
