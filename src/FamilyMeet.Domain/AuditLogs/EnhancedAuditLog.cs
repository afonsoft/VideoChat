using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace FamilyMeet.AuditLogs
{
    public class EnhancedAuditLog : FullAuditedAggregateRoot<Guid>
    {
        public string ApplicationName { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string TenantId { get; set; }
        public string ExecutionTime { get; set; }
        public string ExecutionDuration { get; set; }
        public string ClientIpAddress { get; set; }
        public string ClientName { get; set; }
        public string CorrelationId { get; set; }
        public string RequestMethod { get; set; }
        public string RequestUrl { get; set; }
        public string Action { get; set; }
        public string ControllerName { get; set; }
        public string Exception { get; set; }
        public string ExceptionMessage { get; set; }
        public string ExceptionStackTrace { get; set; }
        public string Category { get; set; }
        public string LogLevel { get; set; }
        public string Message { get; set; }
        public string AdditionalData { get; set; }
        public string BrowserInfo { get; set; }
        public string OperatingSystem { get; set; }
        public string DeviceInfo { get; set; }
        public string SessionId { get; set; }
        public bool IsSuccess { get; set; }
        public bool HasException { get; set; }
        public string Severity { get; set; }
        public string Component { get; set; }
        public string Module { get; set; }
        public string Feature { get; set; }
        public long? ResponseSize { get; set; }
        public int? ResponseStatusCode { get; set; }
        public string Tags { get; set; }
        public DateTime? ArchiveDate { get; set; }
        public bool IsArchived { get; set; }
        public string RetentionPolicy { get; set; }

        protected EnhancedAuditLog()
        {
        }

        public EnhancedAuditLog(
            Guid id,
            string applicationName,
            string userId,
            string userName,
            string action,
            string message,
            string level = "Information",
            string category = null,
            string component = null,
            string module = null,
            string feature = null)
            : base(id)
        {
            ApplicationName = applicationName;
            UserId = userId;
            UserName = userName;
            Action = action;
            Message = message;
            LogLevel = level;
            Category = category;
            Component = component;
            Module = module;
            Feature = feature;
            IsSuccess = true;
            HasException = false;
            Severity = "Normal";
            ExecutionTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff");
            IsArchived = false;
        }

        public void MarkAsSuccess()
        {
            IsSuccess = true;
            HasException = false;
            Exception = null;
            ExceptionMessage = null;
            ExceptionStackTrace = null;
            Severity = "Success";
        }

        public void MarkAsException(Exception exception, string severity = "Error")
        {
            IsSuccess = false;
            HasException = true;
            Exception = exception.GetType().Name;
            ExceptionMessage = exception.Message;
            ExceptionStackTrace = exception.StackTrace;
            Severity = severity;
        }

        public void SetAdditionalData(string key, object value)
        {
            var data = System.Text.Json.JsonSerializer.Serialize(new { key = value });
            AdditionalData = data;
        }

        public void SetMetadata(string browserInfo, string operatingSystem, string deviceInfo)
        {
            BrowserInfo = browserInfo;
            OperatingSystem = operatingSystem;
            DeviceInfo = deviceInfo;
        }

        public void Archive()
        {
            IsArchived = true;
            ArchiveDate = DateTime.UtcNow;
        }

        public void SetRetentionPolicy(string policy)
        {
            RetentionPolicy = policy;
        }

        public bool IsExpired(string retentionPolicy)
        {
            if (string.IsNullOrEmpty(retentionPolicy) || IsArchived != true)
                return false;

            var retentionDays = GetRetentionDays(retentionPolicy);
            if (!retentionDays.HasValue)
                return false;

            return CreationTime.AddDays(retentionDays.Value) < DateTime.UtcNow;
        }

        private int? GetRetentionDays(string policy)
        {
            return policy.ToLower() switch
            {
                "daily" => 1,
                "weekly" => 7,
                "monthly" => 30,
                "quarterly" => 90,
                "yearly" => 365,
                "never" => null,
                _ => 30 // default
            };
        }
    }

    public class AuditLogCategory : FullAuditedAggregateRoot<Guid>
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public string Color { get; set; }
        public string Icon { get; set; }
        public bool IsEnabled { get; set; }
        public int Sort { get; set; }
        public string TenantId { get; set; }

        protected AuditLogCategory()
        {
        }

        public AuditLogCategory(
            Guid id,
            string name,
            string displayName,
            string description = null,
            string color = "#1890ff",
            string icon = "audit",
            bool isEnabled = true)
            : base(id)
        {
            Name = name;
            DisplayName = displayName;
            Description = description;
            Color = color;
            Icon = icon;
            IsEnabled = isEnabled;
            Sort = 0;
        }

        public void Enable()
        {
            IsEnabled = true;
        }

        public void Disable()
        {
            IsEnabled = false;
        }

        public void UpdateDetails(string displayName, string description, string color = null, string icon = null)
        {
            DisplayName = displayName;
            Description = description;
            if (!string.IsNullOrEmpty(color))
            {
                Color = color;
            }
            if (!string.IsNullOrEmpty(icon))
            {
                Icon = icon;
            }
        }

        public void SetSortOrder(int sort)
        {
            Sort = sort;
        }
    }

    public class AuditLogRetention : FullAuditedAggregateRoot<Guid>
    {
        public string PolicyName { get; set; }
        public string Description { get; set; }
        public int RetentionDays { get; set; }
        public bool IsEnabled { get; set; }
        public string TenantId { get; set; }
        public string CategoryName { get; set; }
        public string LogLevel { get; set; }

        protected AuditLogRetention()
        {
        }

        public AuditLogRetention(
            Guid id,
            string policyName,
            string description,
            int retentionDays,
            bool isEnabled = true,
            string categoryName = null,
            string logLevel = "Information")
            : base(id)
        {
            PolicyName = policyName;
            Description = description;
            RetentionDays = retentionDays;
            IsEnabled = isEnabled;
            CategoryName = categoryName;
            LogLevel = logLevel;
        }

        public void Enable()
        {
            IsEnabled = true;
        }

        public void Disable()
        {
            IsEnabled = false;
        }

        public void UpdatePolicy(string description, int retentionDays, string categoryName = null, string logLevel = "Information")
        {
            Description = description;
            RetentionDays = retentionDays;
            CategoryName = categoryName;
            LogLevel = logLevel;
        }
    }
}
