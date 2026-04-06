using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace FamilyMeet.BackgroundJobs
{
    public interface IBackgroundJobAppService : IApplicationService
    {
        Task<PagedResultDto<BackgroundJobDto>> GetListAsync(GetBackgroundJobsInput input);
        Task<BackgroundJobDto> GetAsync(Guid id);
        Task<BackgroundJobDto> CreateAsync(CreateBackgroundJobDto input);
        Task<BackgroundJobDto> UpdateAsync(Guid id, UpdateBackgroundJobDto input);
        Task DeleteAsync(Guid id);
        Task<BackgroundJobDto> StartAsync(Guid id);
        Task<BackgroundJobDto> CompleteAsync(Guid id, string result = null);
        Task<BackgroundJobDto> FailAsync(Guid id, string errorMessage);
        Task<BackgroundJobDto> RetryAsync(Guid id);
        Task<BackgroundJobDto> CancelAsync(Guid id);
        Task<List<BackgroundJobDto>> GetPendingJobsAsync();
    }
}
