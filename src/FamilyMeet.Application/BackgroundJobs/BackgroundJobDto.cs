using System;
using Volo.Abp.Application.Dtos;

namespace FamilyMeet.BackgroundJobs
{
    public class BackgroundJobDto : EntityDto<Guid>
    {
        public string JobName { get; set; }
        public string JobType { get; set; }
        public string JobParameters { get; set; }
        public string Status { get; set; }
        public DateTime? ScheduledTime { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string Result { get; set; }
        public string ErrorMessage { get; set; }
        public int RetryCount { get; set; }
        public int MaxRetryCount { get; set; }
        public string Priority { get; set; }
        public string CreatedByUserId { get; set; }
        public string TenantId { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime? LastModificationTime { get; set; }
    }

    public class CreateBackgroundJobDto
    {
        public string JobName { get; set; }
        public string JobType { get; set; }
        public string JobParameters { get; set; }
        public string Priority { get; set; }
        public int? MaxRetryCount { get; set; }
        public DateTime? ScheduledTime { get; set; }
    }

    public class UpdateBackgroundJobDto
    {
        public string JobName { get; set; }
        public string JobType { get; set; }
        public string JobParameters { get; set; }
        public string Priority { get; set; }
        public int MaxRetryCount { get; set; }
        public DateTime? ScheduledTime { get; set; }
    }

    public class GetBackgroundJobsInput : PagedAndSortedResultRequestDto
    {
        public string Status { get; set; }
        public string JobType { get; set; }
        public string JobName { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class BackgroundJobActionDto
    {
        public Guid Id { get; set; }
        public string Action { get; set; }
        public string Parameters { get; set; }
    }

    public class BackgroundJobResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public BackgroundJobDto Job { get; set; }
    }
}
