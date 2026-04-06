using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace FamilyMeet.BackgroundJobs
{
    public class BackgroundJob : FullAuditedAggregateRoot<Guid>
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

        protected BackgroundJob()
        {
        }

        public BackgroundJob(
            Guid id,
            string jobName,
            string jobType,
            string jobParameters = null,
            string priority = "Normal",
            int maxRetryCount = 3)
            : base(id)
        {
            JobName = jobName;
            JobType = jobType;
            JobParameters = jobParameters;
            Status = "Pending";
            Priority = priority;
            MaxRetryCount = maxRetryCount;
            RetryCount = 0;
        }

        public void Schedule(DateTime scheduledTime)
        {
            Status = "Scheduled";
            ScheduledTime = scheduledTime;
        }

        public void Start()
        {
            Status = "Running";
            StartTime = DateTime.UtcNow;
        }

        public void Complete(string result = null)
        {
            Status = "Completed";
            EndTime = DateTime.UtcNow;
            Result = result;
        }

        public void Fail(string errorMessage)
        {
            Status = "Failed";
            EndTime = DateTime.UtcNow;
            ErrorMessage = errorMessage;
            RetryCount++;
        }

        public void Retry()
        {
            Status = "Pending";
            ErrorMessage = null;
        }

        public void Cancel()
        {
            Status = "Cancelled";
            EndTime = DateTime.UtcNow;
        }
    }
}
