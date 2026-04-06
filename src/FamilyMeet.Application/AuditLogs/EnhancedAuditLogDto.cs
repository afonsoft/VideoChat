using System;
using Volo.Abp.Application.Dtos;

namespace FamilyMeet.AuditLogs
{
    public class EnhancedAuditLogDto : EntityDto<Guid>
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
        public DateTime CreationTime { get; set; }
        public DateTime? LastModificationTime { get; set; }
    }

    public class AuditLogCategoryDto : EntityDto<Guid>
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public string Color { get; set; }
        public string Icon { get; set; }
        public bool IsEnabled { get; set; }
        public int Sort { get; set; }
        public string TenantId { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime? LastModificationTime { get; set; }
    }

    public class AuditLogRetentionDto : EntityDto<Guid>
    {
        public string PolicyName { get; set; }
        public string Description { get; set; }
        public int RetentionDays { get; set; }
        public bool IsEnabled { get; set; }
        public string TenantId { get; set; }
        public string CategoryName { get; set; }
        public string LogLevel { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime? LastModificationTime { get; set; }
    }

    public class CreateEnhancedAuditLogDto
    {
        public string ApplicationName { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Action { get; set; }
        public string Message { get; set; }
        public string LogLevel { get; set; }
        public string Category { get; set; }
        public string Component { get; set; }
        public string Module { get; set; }
        public string Feature { get; set; }
        public string ExceptionMessage { get; set; }
        public string Severity { get; set; }
        public string BrowserInfo { get; set; }
        public string OperatingSystem { get; set; }
        public string DeviceInfo { get; set; }
        public string RequestData { get; set; }
        public string RetentionPolicy { get; set; }
    }

    public class GetEnhancedAuditLogsInput : PagedAndSortedResultRequestDto
    {
        public string ApplicationName { get; set; }
        public string UserId { get; set; }
        public string TenantId { get; set; }
        public string Category { get; set; }
        public string LogLevel { get; set; }
        public string Action { get; set; }
        public string Component { get; set; }
        public string Module { get; set; }
        public string Feature { get; set; }
        public bool? IsSuccess { get; set; }
        public bool? HasException { get; set; }
        public bool? IsArchived { get; set; }
        public string Filter { get; set; }
    }

    public class EnhancedAuditLogStatisticsDto
    {
        public int TotalLogs { get; set; }
        public int SuccessLogs { get; set; }
        public int ErrorLogs { get; set; }
        public int TodayLogs { get; set; }
        public int Last24HoursLogs { get; set; }
        public List<string> TopErrors { get; set; }
        public List<string> TopActions { get; set; }
    }

    public class ExportEnhancedAuditLogsInput
    {
        public string ApplicationName { get; set; }
        public string UserId { get; set; }
        public string Category { get; set; }
        public string LogLevel { get; set; }
        public string Format { get; set; } = "json";
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? MaxRecords { get; set; }
    }

    public class EnhancedAuditLogExportDto
    {
        public string ExecutionTime { get; set; }
        public string ApplicationName { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Action { get; set; }
        public string Message { get; set; }
        public string LogLevel { get; set; }
        public string Category { get; set; }
        public string Component { get; set; }
        public string Module { get; set; }
        public string Feature { get; set; }
        public string ClientIpAddress { get; set; }
        public string ClientName { get; set; }
        public string CorrelationId { get; set; }
        public bool IsSuccess { get; set; }
        public bool HasException { get; set; }
        public string Exception { get; set; }
        public string Severity { get; set; }
        public string ExecutionDuration { get; set; }
    }
}
