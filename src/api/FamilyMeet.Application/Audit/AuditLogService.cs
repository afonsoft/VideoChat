using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Services;
using Volo.Abp.Settings;
using Volo.Abp.Users;
using Volo.Abp.Uow;
using FamilyMeet.Domain.Audit;
using FamilyMeet.Settings;
using System.Text.Json;

namespace FamilyMeet.Application.Audit
{
    public interface IAuditLogService
    {
        Task LogAsync(AuditLogAction action, string details = null, Dictionary<string, object> parameters = null);
        Task LogSecurityAsync(string action, string details = null, Dictionary<string, object> parameters = null);
        Task LogEntityChangeAsync(string entityType, string entityId, string operation, Dictionary<string, object> oldValues = null, Dictionary<string, object> newValues = null);
        Task LogRequestAsync(string method, string url, string userAgent, string ipAddress, int? statusCode, long? responseTime, string error = null);
        Task<List<AuditLog>> GetAuditLogsAsync(DateTime? startDate = null, DateTime? endDate = null, string action = null, string userId = null, int maxResultCount = 50);
        Task CleanupOldAuditLogsAsync();
    }

    public class AuditLogService : DomainService, IAuditLogService, ITransientDependency
    {
        private readonly ILogger<AuditLogService> _logger;
        private readonly ISettingManager _settingManager;

        public AuditLogService(
            ILogger<AuditLogService> logger,
            ISettingManager settingManager)
        {
            _logger = logger;
            _settingManager = settingManager;
        }

        public async Task LogAsync(AuditLogAction action, string details = null, Dictionary<string, object> parameters = null)
        {
            if (!await IsAuditLoggingEnabledAsync())
            {
                return;
            }

            try
            {
                var auditLog = new AuditLog(
                    GuidGenerator.Create(),
                    action.ToString(),
                    details,
                    CurrentUser.Id,
                    CurrentUser.UserName,
                    DateTime.UtcNow,
                    GetClientIpAddress(),
                    parameters != null ? JsonSerializer.Serialize(parameters) : null
                );

                await Repository.InsertAsync(auditLog);

                _logger.LogInformation("Audit log created: {Action} by {User} at {Time}", 
                    action, CurrentUser.UserName, DateTime.UtcNow);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating audit log for action: {Action}", action);
            }
        }

        public async Task LogSecurityAsync(string action, string details = null, Dictionary<string, object> parameters = null)
        {
            if (!await IsSecurityLoggingEnabledAsync())
            {
                return;
            }

            try
            {
                var auditLog = new AuditLog(
                    GuidGenerator.Create(),
                    "SECURITY",
                    $"{action}: {details}",
                    CurrentUser.Id,
                    CurrentUser.UserName,
                    DateTime.UtcNow,
                    GetClientIpAddress(),
                    parameters != null ? JsonSerializer.Serialize(parameters) : null
                );

                auditLog.SetSecurityLog(true);
                await Repository.InsertAsync(auditLog);

                _logger.LogWarning("Security audit log: {Action} by {User} at {Time}", 
                    action, CurrentUser.UserName, DateTime.UtcNow);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating security audit log for action: {Action}", action);
            }
        }

        public async Task LogEntityChangeAsync(string entityType, string entityId, string operation, Dictionary<string, object> oldValues = null, Dictionary<string, object> newValues = null)
        {
            if (!await IsEntityChangeTrackingEnabledAsync())
            {
                return;
            }

            try
            {
                var details = $"Entity: {entityType}, ID: {entityId}, Operation: {operation}";
                var parameters = new Dictionary<string, object>
                {
                    ["EntityType"] = entityType,
                    ["EntityId"] = entityId,
                    ["Operation"] = operation
                };

                if (oldValues != null)
                {
                    parameters["OldValues"] = oldValues;
                }

                if (newValues != null)
                {
                    parameters["NewValues"] = newValues;
                }

                var auditLog = new AuditLog(
                    GuidGenerator.Create(),
                    "ENTITY_CHANGE",
                    details,
                    CurrentUser.Id,
                    CurrentUser.UserName,
                    DateTime.UtcNow,
                    GetClientIpAddress(),
                    JsonSerializer.Serialize(parameters)
                );

                auditLog.SetEntityChangeLog(true);
                await Repository.InsertAsync(auditLog);

                _logger.LogInformation("Entity change audit log: {EntityType} {Operation} by {User}", 
                    entityType, operation, CurrentUser.UserName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating entity change audit log for: {EntityType} {Operation}", 
                    entityType, operation);
            }
        }

        public async Task LogRequestAsync(string method, string url, string userAgent, string ipAddress, int? statusCode, long? responseTime, string error = null)
        {
            if (!await IsRequestLoggingEnabledAsync())
            {
                return;
            }

            try
            {
                var details = $"{method} {url} - {statusCode}";
                if (!string.IsNullOrEmpty(error))
                {
                    details += $" - Error: {error}";
                }

                var parameters = new Dictionary<string, object>
                {
                    ["Method"] = method,
                    ["Url"] = url,
                    ["UserAgent"] = userAgent,
                    ["StatusCode"] = statusCode,
                    ["ResponseTime"] = responseTime
                };

                var auditLog = new AuditLog(
                    GuidGenerator.Create(),
                    "REQUEST",
                    details,
                    CurrentUser.Id,
                    CurrentUser.UserName,
                    DateTime.UtcNow,
                    ipAddress,
                    JsonSerializer.Serialize(parameters)
                );

                auditLog.SetRequestLog(true);
                await Repository.InsertAsync(auditLog);

                _logger.LogDebug("Request audit log: {Method} {Url} by {User}", 
                    method, url, CurrentUser.UserName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating request audit log for: {Method} {Url}", method, url);
            }
        }

        public async Task<List<AuditLog>> GetAuditLogsAsync(DateTime? startDate = null, DateTime? endDate = null, string action = null, string userId = null, int maxResultCount = 50)
        {
            var queryable = await Repository.GetQueryableAsync();

            var query = queryable.AsQueryable();

            if (startDate.HasValue)
            {
                query = query.Where(x => x.CreationTime >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(x => x.CreationTime <= endDate.Value);
            }

            if (!string.IsNullOrEmpty(action))
            {
                query = query.Where(x => x.Action.Contains(action));
            }

            if (!string.IsNullOrEmpty(userId))
            {
                var userIdGuid = Guid.Parse(userId);
                query = query.Where(x => x.UserId == userIdGuid);
            }

            var logs = await AsyncExecuter.ToListAsync(
                query.OrderByDescending(x => x.CreationTime)
                      .Take(maxResultCount)
            );

            return logs;
        }

        public async Task CleanupOldAuditLogsAsync()
        {
            var retentionDays = await GetAuditLogRetentionDaysAsync();
            var cutoffDate = DateTime.UtcNow.AddDays(-retentionDays);

            try
            {
                var queryable = await Repository.GetQueryableAsync();
                var oldLogs = await AsyncExecuter.ToListAsync(
                    queryable.Where(x => x.CreationTime < cutoffDate)
                );

                if (oldLogs.Any())
                {
                    await Repository.DeleteManyAsync(oldLogs);
                    _logger.LogInformation("Cleaned up {Count} old audit logs", oldLogs.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up old audit logs");
            }
        }

        private async Task<bool> IsAuditLoggingEnabledAsync()
        {
            var setting = await _settingManager.GetOrNullAsync(FamilyMeetSettings.Audit.EnableAuditLogging);
            return bool.TryParse(setting, out var enabled) && enabled;
        }

        private async Task<bool> IsSecurityLoggingEnabledAsync()
        {
            var setting = await _settingManager.GetOrNullAsync(FamilyMeetSettings.Audit.EnableSecurityLogging);
            return bool.TryParse(setting, out var enabled) && enabled;
        }

        private async Task<bool> IsEntityChangeTrackingEnabledAsync()
        {
            var setting = await _settingManager.GetOrNullAsync(FamilyMeetSettings.Audit.EnableEntityChangeTracking);
            return bool.TryParse(setting, out var enabled) && enabled;
        }

        private async Task<bool> IsRequestLoggingEnabledAsync()
        {
            var setting = await _settingManager.GetOrNullAsync(FamilyMeetSettings.Audit.EnableRequestLogging);
            return bool.TryParse(setting, out var enabled) && enabled;
        }

        private async Task<int> GetAuditLogRetentionDaysAsync()
        {
            var setting = await _settingManager.GetOrNullAsync(FamilyMeetSettings.Audit.AuditLogRetentionDays);
            return int.TryParse(setting, out var days) ? days : 90;
        }

        private string GetClientIpAddress()
        {
            // This would typically be injected from HttpContext
            // For now, return a placeholder
            return "127.0.0.1";
        }
    }

    public enum AuditLogAction
    {
        USER_LOGIN,
        USER_LOGOUT,
        USER_REGISTER,
        USER_UPDATE_PROFILE,
        USER_CHANGE_PASSWORD,
        USER_LOCKOUT,
        USER_UNLOCK,
        USER_ENABLE_2FA,
        USER_DISABLE_2FA,
        USER_LINK_SOCIAL,
        USER_UNLINK_SOCIAL,
        
        GROUP_CREATE,
        GROUP_UPDATE,
        GROUP_DELETE,
        GROUP_ADD_MEMBER,
        GROUP_REMOVE_MEMBER,
        GROUP_UPDATE_MEMBER_ROLE,
        
        MESSAGE_SEND,
        MESSAGE_UPDATE,
        MESSAGE_DELETE,
        
        CALL_START,
        CALL_END,
        CALL_JOIN,
        CALL_LEAVE,
        CALL_RECORDING_START,
        CALL_RECORDING_STOP,
        
        FILE_UPLOAD,
        FILE_DOWNLOAD,
        FILE_DELETE,
        
        SETTINGS_UPDATE,
        ROLE_CREATE,
        ROLE_UPDATE,
        ROLE_DELETE,
        
        SYSTEM_BACKUP,
        SYSTEM_RESTORE,
        SYSTEM_MAINTENANCE
    }
}
