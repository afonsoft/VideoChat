using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace FamilyMeet.BackgroundJobs
{
    public class BackgroundJobAppService : ApplicationService, IBackgroundJobAppService
    {
        private readonly IRepository<BackgroundJob, Guid> _backgroundJobRepository;

        public BackgroundJobAppService(IRepository<BackgroundJob, Guid> backgroundJobRepository)
        {
            _backgroundJobRepository = backgroundJobRepository;
        }

        public async Task<PagedResultDto<BackgroundJobDto>> GetListAsync(GetBackgroundJobsInput input)
        {
            var queryable = await _backgroundJobRepository.GetQueryableAsync();

            queryable = queryable
                .WhereIf(!input.Status.IsNullOrEmpty(), x => x.Status == input.Status)
                .WhereIf(!input.JobType.IsNullOrEmpty(), x => x.JobType.Contains(input.JobType))
                .WhereIf(!input.JobName.IsNullOrEmpty(), x => x.JobName.Contains(input.JobName))
                .WhereIf(input.StartDate.HasValue, x => x.CreationTime >= input.StartDate.Value)
                .WhereIf(input.EndDate.HasValue, x => x.CreationTime <= input.EndDate.Value);

            var totalCount = await queryable.CountAsync();
            var items = await queryable
                .OrderByDescending(x => x.CreationTime)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount)
                .ToListAsync();

            return new PagedResultDto<BackgroundJobDto>(
                totalCount,
                ObjectMapper.Map<List<BackgroundJob>, List<BackgroundJobDto>>(items)
            );
        }

        public async Task<BackgroundJobDto> GetAsync(Guid id)
        {
            var backgroundJob = await _backgroundJobRepository.GetAsync(id);
            return ObjectMapper.Map<BackgroundJob, BackgroundJobDto>(backgroundJob);
        }

        public async Task<BackgroundJobDto> CreateAsync(CreateBackgroundJobDto input)
        {
            var backgroundJob = new BackgroundJob(
                GuidGenerator.Create(),
                input.JobName,
                input.JobType,
                input.JobParameters,
                input.Priority ?? "Normal",
                input.MaxRetryCount ?? 3
            );

            backgroundJob.SetCreatedByUserId(CurrentUser.Id?.ToString());
            backgroundJob.TenantId = CurrentTenant.Id?.ToString();

            if (input.ScheduledTime.HasValue)
            {
                backgroundJob.Schedule(input.ScheduledTime.Value);
            }

            await _backgroundJobRepository.InsertAsync(backgroundJob);
            return ObjectMapper.Map<BackgroundJob, BackgroundJobDto>(backgroundJob);
        }

        public async Task<BackgroundJobDto> UpdateAsync(Guid id, UpdateBackgroundJobDto input)
        {
            var backgroundJob = await _backgroundJobRepository.GetAsync(id);

            backgroundJob.JobName = input.JobName;
            backgroundJob.JobType = input.JobType;
            backgroundJob.JobParameters = input.JobParameters;
            backgroundJob.Priority = input.Priority;
            backgroundJob.MaxRetryCount = input.MaxRetryCount;

            if (input.ScheduledTime.HasValue && backgroundJob.Status == "Pending")
            {
                backgroundJob.Schedule(input.ScheduledTime.Value);
            }

            await _backgroundJobRepository.UpdateAsync(backgroundJob);
            return ObjectMapper.Map<BackgroundJob, BackgroundJobDto>(backgroundJob);
        }

        public async Task DeleteAsync(Guid id)
        {
            await _backgroundJobRepository.DeleteAsync(id);
        }

        public async Task<BackgroundJobDto> StartAsync(Guid id)
        {
            var backgroundJob = await _backgroundJobRepository.GetAsync(id);
            
            if (backgroundJob.Status != "Pending" && backgroundJob.Status != "Failed")
            {
                throw new BusinessException("BackgroundJob:InvalidStatus", "Job can only be started from Pending or Failed status");
            }

            backgroundJob.Start();
            await _backgroundJobRepository.UpdateAsync(backgroundJob);
            return ObjectMapper.Map<BackgroundJob, BackgroundJobDto>(backgroundJob);
        }

        public async Task<BackgroundJobDto> CompleteAsync(Guid id, string result = null)
        {
            var backgroundJob = await _backgroundJobRepository.GetAsync(id);
            
            if (backgroundJob.Status != "Running")
            {
                throw new BusinessException("BackgroundJob:InvalidStatus", "Job can only be completed from Running status");
            }

            backgroundJob.Complete(result);
            await _backgroundJobRepository.UpdateAsync(backgroundJob);
            return ObjectMapper.Map<BackgroundJob, BackgroundJobDto>(backgroundJob);
        }

        public async Task<BackgroundJobDto> FailAsync(Guid id, string errorMessage)
        {
            var backgroundJob = await _backgroundJobRepository.GetAsync(id);
            
            if (backgroundJob.Status != "Running")
            {
                throw new BusinessException("BackgroundJob:InvalidStatus", "Job can only be failed from Running status");
            }

            backgroundJob.Fail(errorMessage);
            await _backgroundJobRepository.UpdateAsync(backgroundJob);
            return ObjectMapper.Map<BackgroundJob, BackgroundJobDto>(backgroundJob);
        }

        public async Task<BackgroundJobDto> RetryAsync(Guid id)
        {
            var backgroundJob = await _backgroundJobRepository.GetAsync(id);
            
            if (backgroundJob.Status != "Failed")
            {
                throw new BusinessException("BackgroundJob:InvalidStatus", "Job can only be retried from Failed status");
            }

            if (backgroundJob.RetryCount >= backgroundJob.MaxRetryCount)
            {
                throw new BusinessException("BackgroundJob:MaxRetryReached", "Maximum retry count reached");
            }

            backgroundJob.Retry();
            await _backgroundJobRepository.UpdateAsync(backgroundJob);
            return ObjectMapper.Map<BackgroundJob, BackgroundJobDto>(backgroundJob);
        }

        public async Task<BackgroundJobDto> CancelAsync(Guid id)
        {
            var backgroundJob = await _backgroundJobRepository.GetAsync(id);
            
            if (backgroundJob.Status == "Completed" || backgroundJob.Status == "Cancelled")
            {
                throw new BusinessException("BackgroundJob:InvalidStatus", "Job cannot be cancelled from Completed or Cancelled status");
            }

            backgroundJob.Cancel();
            await _backgroundJobRepository.UpdateAsync(backgroundJob);
            return ObjectMapper.Map<BackgroundJob, BackgroundJobDto>(backgroundJob);
        }

        public async Task<List<BackgroundJobDto>> GetPendingJobsAsync()
        {
            var queryable = await _backgroundJobRepository.GetQueryableAsync();
            var pendingJobs = await queryable
                .Where(x => x.Status == "Pending" && (!x.ScheduledTime.HasValue || x.ScheduledTime.Value <= DateTime.UtcNow))
                .OrderBy(x => x.Priority == "High" ? 0 : x.Priority == "Normal" ? 1 : 2)
                .ThenBy(x => x.CreationTime)
                .ToListAsync();

            return ObjectMapper.Map<List<BackgroundJob>, List<BackgroundJobDto>>(pendingJobs);
        }
    }
}
