using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace FamilyMeet.AuditLogs
{
    public class EnhancedAuditLogAppService : ApplicationService, IEnhancedAuditLogAppService
    {
        private readonly IRepository<EnhancedAuditLog, Guid> _enhancedAuditLogRepository;
        private readonly IRepository<AuditLogCategory, Guid> _auditLogCategoryRepository;
        private readonly IRepository<AuditLogRetention, Guid> _auditLogRetentionRepository;

        public EnhancedAuditLogAppService(
            IRepository<EnhancedAuditLog, Guid> enhancedAuditLogRepository,
            IRepository<AuditLogCategory, Guid> auditLogCategoryRepository,
            IRepository<AuditLogRetention, Guid> auditLogRetentionRepository)
        {
            _enhancedAuditLogRepository = enhancedAuditLogRepository;
            _auditLogCategoryRepository = auditLogCategoryRepository;
            _auditLogRetentionRepository = auditLogRetentionRepository;
        }

        public async Task<PagedResultDto<EnhancedAuditLogDto>> GetListAsync(GetEnhancedAuditLogsInput input)
        {
            var queryable = await _enhancedAuditLogRepository.GetQueryableAsync();

            queryable = queryable
                .WhereIf(!input.ApplicationName.IsNullOrEmpty(), x => x.ApplicationName == input.ApplicationName)
                .WhereIf(!input.UserId.IsNullOrEmpty(), x => x.UserId == input.UserId)
                .WhereIf(!input.TenantId.IsNullOrEmpty(), x => x.TenantId == input.TenantId)
                .WhereIf(!input.Category.IsNullOrEmpty(), x => x.Category == input.Category)
                .WhereIf(!input.LogLevel.IsNullOrEmpty(), x => x.LogLevel == input.LogLevel)
                .WhereIf(!input.Action.IsNullOrEmpty(), x => x.Action.Contains(input.Action))
                .WhereIf(!input.Component.IsNullOrEmpty(), x => x.Component.Contains(input.Component))
                .WhereIf(!input.Module.IsNullOrEmpty(), x => x.Module.Contains(input.Module))
                .WhereIf(!input.Feature.IsNullOrEmpty(), x => x.Feature.Contains(input.Feature))
                .WhereIf(input.IsSuccess.HasValue, x => x.IsSuccess == input.IsSuccess.Value)
                .WhereIf(input.HasException.HasValue, x => x.HasException == input.HasException.Value)
                .WhereIf(input.IsArchived.HasValue, x => x.IsArchived == input.IsArchived.Value)
                .WhereIf(!input.Filter.IsNullOrEmpty(), x => 
                    x.Message.Contains(input.Filter) || 
                    x.Action.Contains(input.Filter) ||
                    x.Component.Contains(input.Filter) ||
                    x.Module.Contains(input.Filter) ||
                    x.Feature.Contains(input.Filter) ||
                    x.UserName.Contains(input.Filter));

            var totalCount = await queryable.CountAsync();
            var items = await queryable
                .OrderByDescending(x => x.ExecutionTime)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount)
                .ToListAsync();

            return new PagedResultDto<EnhancedAuditLogDto>(
                totalCount,
                ObjectMapper.Map<List<EnhancedAuditLog>, List<EnhancedAuditLogDto>>(items)
            );
        }

        public async Task<EnhancedAuditLogDto> GetAsync(Guid id)
        {
            var auditLog = await _enhancedAuditLogRepository.GetAsync(id);
            return ObjectMapper.Map<EnhancedAuditLog, EnhancedAuditLogDto>(auditLog);
        }

        public async Task<EnhancedAuditLogDto> CreateAsync(CreateEnhancedAuditLogDto input)
        {
            var auditLog = new EnhancedAuditLog(
                GuidGenerator.Create(),
                input.ApplicationName,
                input.UserId,
                input.UserName,
                input.Action,
                input.Message,
                input.LogLevel ?? "Information",
                input.Category,
                input.Component,
                input.Module,
                input.Feature
            );

            auditLog.SetMetadata(input.BrowserInfo, input.OperatingSystem, input.DeviceInfo);
            auditLog.SetAdditionalData("RequestData", input.RequestData);
            auditLog.SetRetentionPolicy(input.RetentionPolicy ?? "Monthly");

            if (!string.IsNullOrEmpty(input.ExceptionMessage))
            {
                auditLog.MarkAsException(new Exception(input.ExceptionMessage), input.Severity ?? "Error");
            }

            await _enhancedAuditLogRepository.InsertAsync(auditLog);
            return ObjectMapper.Map<EnhancedAuditLog, EnhancedAuditLogDto>(auditLog);
        }

        public async Task<List<EnhancedAuditLogDto>> GetByCategoryAsync(string category)
        {
            var queryable = await _enhancedAuditLogRepository.GetQueryableAsync();
            var auditLogs = await queryable
                .Where(x => x.Category == category)
                .OrderByDescending(x => x.ExecutionTime)
                .Take(1000)
                .ToListAsync();

            return ObjectMapper.Map<List<EnhancedAuditLog>, List<EnhancedAuditLogDto>>(auditLogs);
        }

        public async Task<List<EnhancedAuditLogDto>> GetByUserAsync(string userId)
        {
            var queryable = await _enhancedAuditLogRepository.GetQueryableAsync();
            var auditLogs = await queryable
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.ExecutionTime)
                .Take(1000)
                .ToListAsync();

            return ObjectMapper.Map<List<EnhancedAuditLog>, List<EnhancedAuditLogDto>>(auditLogs);
        }

        public async Task<List<EnhancedAuditLogDto>> GetErrorsAsync(int? days = 7)
        {
            var startDate = DateTime.UtcNow.AddDays(-days.Value);
            var queryable = await _enhancedAuditLogRepository.GetQueryableAsync();
            var errorLogs = await queryable
                .Where(x => x.HasException && x.ExecutionTime >= startDate)
                .OrderByDescending(x => x.ExecutionTime)
                .Take(1000)
                .ToListAsync();

            return ObjectMapper.Map<List<EnhancedAuditLog>, List<EnhancedAuditLogDto>>(errorLogs);
        }

        public async Task<List<EnhancedAuditLogDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var queryable = await _enhancedAuditLogRepository.GetQueryableAsync();
            var auditLogs = await queryable
                .Where(x => x.ExecutionTime >= startDate && x.ExecutionTime <= endDate)
                .OrderByDescending(x => x.ExecutionTime)
                .Take(10000)
                .ToListAsync();

            return ObjectMapper.Map<List<EnhancedAuditLog>, List<EnhancedAuditLogDto>>(auditLogs);
        }

        public async Task<EnhancedAuditLogStatisticsDto> GetStatisticsAsync()
        {
            var queryable = await _enhancedAuditLogRepository.GetQueryableAsync();
            var totalCount = await queryable.CountAsync();
            var successCount = await queryable.CountAsync(x => x.IsSuccess && !x.HasException);
            var errorCount = await queryable.CountAsync(x => x.HasException);
            var todayCount = await queryable.CountAsync(x => x.ExecutionTime >= DateTime.UtcNow.Date);
            var last24HoursCount = await queryable.CountAsync(x => x.ExecutionTime >= DateTime.UtcNow.AddHours(-24));

            return new EnhancedAuditLogStatisticsDto
            {
                TotalLogs = totalCount,
                SuccessLogs = successCount,
                ErrorLogs = errorCount,
                TodayLogs = todayCount,
                Last24HoursLogs = last24HoursCount,
                TopErrors = await GetTopErrorsAsync(),
                TopActions = await GetTopActionsAsync()
            };
        }

        private async Task<List<string>> GetTopErrorsAsync()
        {
            var queryable = await _enhancedAuditLogRepository.GetQueryableAsync();
            var errors = await queryable
                .Where(x => x.HasException)
                .GroupBy(x => x.Exception)
                .Select(g => new { Exception = g.Key, Count = g.Count() })
                .OrderByDescending(g => g.Count)
                .Take(10)
                .ToListAsync();

            return errors.Select(e => e.Exception).ToList();
        }

        private async Task<List<string>> GetTopActionsAsync()
        {
            var queryable = await _enhancedAuditLogRepository.GetQueryableAsync();
            var actions = await queryable
                .Where(x => !x.HasException)
                .GroupBy(x => x.Action)
                .Select(g => new { Action = g.Key, Count = g.Count() })
                .OrderByDescending(g => g.Count)
                .Take(10)
                .ToListAsync();

            return actions.Select(a => a.Action).ToList();
        }

        public async Task<FileResult> ExportAsync(ExportEnhancedAuditLogsInput input)
        {
            var queryable = await _enhancedAuditLogRepository.GetQueryableAsync();
            
            // Apply filters
            if (!string.IsNullOrEmpty(input.ApplicationName))
                queryable = queryable.Where(x => x.ApplicationName == input.ApplicationName);
            
            if (!string.IsNullOrEmpty(input.UserId))
                queryable = queryable.Where(x => x.UserId == input.UserId);
            
            if (!string.IsNullOrEmpty(input.Category))
                queryable = queryable.Where(x => x.Category == input.Category);
            
            if (!string.IsNullOrEmpty(input.LogLevel))
                queryable = queryable.Where(x => x.LogLevel == input.LogLevel);

            if (input.StartDate.HasValue)
                queryable = queryable.Where(x => x.ExecutionTime >= input.StartDate.Value);
            
            if (input.EndDate.HasValue)
                queryable = queryable.Where(x => x.ExecutionTime <= input.EndDate.Value);

            var auditLogs = await queryable
                .OrderByDescending(x => x.ExecutionTime)
                .Take(input.MaxRecords ?? 10000)
                .ToListAsync();

            var exportData = ObjectMapper.Map<List<EnhancedAuditLog>, List<EnhancedAuditLogExportDto>>(auditLogs);

            if (input.Format.ToLower() == "json")
            {
                var json = System.Text.Json.JsonSerializer.Serialize(exportData);
                var bytes = System.Text.Encoding.UTF8.GetBytes(json);
                return File(bytes, "application/json", $"enhanced-audit-logs-{DateTime.Now:yyyyMMddHHmmss}.json");
            }
            
            if (input.Format.ToLower() == "csv")
            {
                var csv = GenerateCsvExport(exportData);
                var bytes = System.Text.Encoding.UTF8.GetBytes(csv);
                return File(bytes, "text/csv", $"enhanced-audit-logs-{DateTime.Now:yyyyMMddHHmmss}.csv");
            }

            throw new NotSupportedException($"Export format '{input.Format}' is not supported");
        }

        private string GenerateCsvExport(List<EnhancedAuditLogExportDto> auditLogs)
        {
            var csv = "ExecutionTime,ApplicationName,UserId,UserName,Action,Message,LogLevel,Category,Component,Module,Feature,ClientIpAddress,ClientName,CorrelationId,IsSuccess,HasException,Exception,Severity,ExecutionDuration\n";
            
            foreach (var log in auditLogs)
            {
                var row = $"{log.ExecutionTime},{log.ApplicationName},{log.UserId},{log.UserName},{log.Action},{EscapeCsvField(log.Message)},{log.LogLevel},{log.Category},{log.Component},{log.Module},{log.Feature},{log.ClientIpAddress},{log.ClientName},{log.CorrelationId},{log.IsSuccess},{log.HasException},{EscapeCsvField(log.Exception)},{log.Severity},{log.ExecutionDuration}";
                csv += row + "\n";
            }
            
            return csv;
        }

        private string EscapeCsvField(string field)
        {
            if (string.IsNullOrEmpty(field))
                return "";
            
            return field.Replace("\"", "\"\"");
        }

        public async Task<bool> ArchiveOldLogsAsync()
        {
            var queryable = await _enhancedAuditLogRepository.GetQueryableAsync();
            var retentionPolicies = await _auditLogRetentionRepository.GetListAsync();
            
            foreach (var policy in retentionPolicies.Where(x => x.IsEnabled))
            {
                var cutoffDate = DateTime.UtcNow.AddDays(-policy.RetentionDays);
                var oldLogs = await queryable
                    .Where(x => !x.IsArchived && x.ExecutionTime < cutoffDate)
                    .ToListAsync();

                foreach (var log in oldLogs)
                {
                    log.Archive();
                    await _enhancedAuditLogRepository.UpdateAsync(log);
                }
            }

            return true;
        }

        public async Task<List<AuditLogCategoryDto>> GetCategoriesAsync()
        {
            var queryable = await _auditLogCategoryRepository.GetQueryableAsync();
            var categories = await queryable
                .Where(x => x.IsEnabled)
                .OrderBy(x => x.Sort)
                .ThenBy(x => x.Name)
                .ToListAsync();

            return ObjectMapper.Map<List<AuditLogCategory>, List<AuditLogCategoryDto>>(categories);
        }

        public async Task<List<AuditLogRetentionDto>> GetRetentionPoliciesAsync()
        {
            var queryable = await _auditLogRetentionRepository.GetQueryableAsync();
            var policies = await queryable
                .Where(x => x.IsEnabled)
                .OrderBy(x => x.PolicyName)
                .ToListAsync();

            return ObjectMapper.Map<List<AuditLogRetention>, List<AuditLogRetentionDto>>(policies);
        }
    }
}
