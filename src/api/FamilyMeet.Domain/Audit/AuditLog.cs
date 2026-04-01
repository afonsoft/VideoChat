using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace FamilyMeet.Domain.Audit
{
    public class AuditLog : FullAuditedAggregateRoot<Guid>
    {
        public string Action { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
        public Guid? UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string ClientIpAddress { get; set; } = string.Empty;
        public string Parameters { get; set; } = string.Empty; // JSON string
        public bool IsSecurityLog { get; set; }
        public bool IsEntityChangeLog { get; set; }
        public bool IsRequestLog { get; set; }

        protected AuditLog()
        {
        }

        public AuditLog(
            Guid id,
            string action,
            string details,
            Guid? userId,
            string userName,
            DateTime creationTime,
            string clientIpAddress,
            string? parameters = null
        ) : base(id)
        {
            Action = action;
            Details = details;
            UserId = userId;
            UserName = userName;
            CreationTime = creationTime;
            ClientIpAddress = clientIpAddress;
            Parameters = parameters ?? string.Empty;
            IsSecurityLog = false;
            IsEntityChangeLog = false;
            IsRequestLog = false;
        }

        public void SetSecurityLog(bool isSecurityLog)
        {
            IsSecurityLog = isSecurityLog;
        }

        public void SetEntityChangeLog(bool isEntityChangeLog)
        {
            IsEntityChangeLog = isEntityChangeLog;
        }

        public void SetRequestLog(bool isRequestLog)
        {
            IsRequestLog = isRequestLog;
        }
    }
}
