using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace FamilyMeet.AuditLogs
{
    public interface IEnhancedAuditLogAppService : IApplicationService
    {
        Task<PagedResultDto<EnhancedAuditLogDto>> GetListAsync(GetEnhancedAuditLogsInput input);
        Task<EnhancedAuditLogDto> GetAsync(Guid id);
        Task<EnhancedAuditLogDto> CreateAsync(CreateEnhancedAuditLogDto input);
        Task<List<EnhancedAuditLogDto>> GetByCategoryAsync(string category);
        Task<List<EnhancedAuditLogDto>> GetByUserAsync(string userId);
        Task<List<EnhancedAuditLogDto>> GetErrorsAsync(int? days = 7);
        Task<List<EnhancedAuditLogDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<EnhancedAuditLogStatisticsDto> GetStatisticsAsync();
        Task<FileResult> ExportAsync(ExportEnhancedAuditLogsInput input);
        Task<bool> ArchiveOldLogsAsync();
        Task<List<AuditLogCategoryDto>> GetCategoriesAsync();
        Task<List<AuditLogRetentionDto>> GetRetentionPoliciesAsync();
    }
}
