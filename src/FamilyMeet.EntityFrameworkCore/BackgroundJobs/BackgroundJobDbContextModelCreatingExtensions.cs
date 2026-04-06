using Microsoft.EntityFrameworkCore;
using Volo.Abp;

namespace FamilyMeet.BackgroundJobs
{
    public static class BackgroundJobDbContextModelCreatingExtensions
    {
        public static void ConfigureBackgroundJobs(this ModelBuilder builder)
        {
            Check.NotNull(builder, nameof(builder));

            builder.Entity<BackgroundJob>(b =>
            {
                b.ToTable("BackgroundJobs");

                b.ConfigureByConvention();

                b.Property(x => x.JobName)
                    .IsRequired()
                    .HasMaxLength(256);

                b.Property(x => x.JobType)
                    .IsRequired()
                    .HasMaxLength(256);

                b.Property(x => x.JobParameters)
                    .HasMaxLength(4000);

                b.Property(x => x.Status)
                    .IsRequired()
                    .HasMaxLength(50);

                b.Property(x => x.Result)
                    .HasMaxLength(4000);

                b.Property(x => x.ErrorMessage)
                    .HasMaxLength(4000);

                b.Property(x => x.Priority)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasDefaultValue("Normal");

                b.Property(x => x.CreatedByUserId)
                    .IsRequired()
                    .HasMaxLength(256);

                b.Property(x => x.TenantId)
                    .HasMaxLength(256);

                b.HasIndex(x => x.Status);
                b.HasIndex(x => x.ScheduledTime);
                b.HasIndex(x => x.JobType);
                b.HasIndex(x => x.CreatedByUserId);
            });
        }
    }
}
