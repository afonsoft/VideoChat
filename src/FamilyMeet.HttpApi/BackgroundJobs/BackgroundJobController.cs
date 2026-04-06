using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.Application.Dtos;

namespace FamilyMeet.BackgroundJobs
{
    [ApiController]
    [Route("api/background-jobs")]
    public class BackgroundJobController : AbpControllerBase
    {
        private readonly IBackgroundJobAppService _backgroundJobAppService;

        public BackgroundJobController(IBackgroundJobAppService backgroundJobAppService)
        {
            _backgroundJobAppService = backgroundJobAppService;
        }

        [HttpGet]
        public async Task<PagedResultDto<BackgroundJobDto>> GetListAsync([FromQuery] GetBackgroundJobsInput input)
        {
            return await _backgroundJobAppService.GetListAsync(input);
        }

        [HttpGet("{id}")]
        public async Task<BackgroundJobDto> GetAsync(Guid id)
        {
            return await _backgroundJobAppService.GetAsync(id);
        }

        [HttpPost]
        public async Task<BackgroundJobDto> CreateAsync([FromBody] CreateBackgroundJobDto input)
        {
            return await _backgroundJobAppService.CreateAsync(input);
        }

        [HttpPut("{id}")]
        public async Task<BackgroundJobDto> UpdateAsync(Guid id, [FromBody] UpdateBackgroundJobDto input)
        {
            return await _backgroundJobAppService.UpdateAsync(id, input);
        }

        [HttpDelete("{id}")]
        public async Task DeleteAsync(Guid id)
        {
            await _backgroundJobAppService.DeleteAsync(id);
        }

        [HttpPost("{id}/start")]
        public async Task<BackgroundJobDto> StartAsync(Guid id)
        {
            return await _backgroundJobAppService.StartAsync(id);
        }

        [HttpPost("{id}/complete")]
        public async Task<BackgroundJobDto> CompleteAsync(Guid id, [FromBody] BackgroundJobActionDto input)
        {
            return await _backgroundJobAppService.CompleteAsync(id, input?.Parameters);
        }

        [HttpPost("{id}/fail")]
        public async Task<BackgroundJobDto> FailAsync(Guid id, [FromBody] BackgroundJobActionDto input)
        {
            return await _backgroundJobAppService.FailAsync(id, input?.Parameters);
        }

        [HttpPost("{id}/retry")]
        public async Task<BackgroundJobDto> RetryAsync(Guid id)
        {
            return await _backgroundJobAppService.RetryAsync(id);
        }

        [HttpPost("{id}/cancel")]
        public async Task<BackgroundJobDto> CancelAsync(Guid id)
        {
            return await _backgroundJobAppService.CancelAsync(id);
        }

        [HttpGet("pending")]
        public async Task<List<BackgroundJobDto>> GetPendingJobsAsync()
        {
            return await _backgroundJobAppService.GetPendingJobsAsync();
        }

        [HttpGet("statistics")]
        public async Task<BackgroundJobStatisticsDto> GetStatisticsAsync()
        {
            // TODO: Implement statistics calculation
            return new BackgroundJobStatisticsDto
            {
                TotalJobs = 0,
                PendingJobs = 0,
                RunningJobs = 0,
                CompletedJobs = 0,
                FailedJobs = 0,
                CancelledJobs = 0
            };
        }
    }

    public class BackgroundJobStatisticsDto
    {
        public int TotalJobs { get; set; }
        public int PendingJobs { get; set; }
        public int RunningJobs { get; set; }
        public int CompletedJobs { get; set; }
        public int FailedJobs { get; set; }
        public int CancelledJobs { get; set; }
    }
}
