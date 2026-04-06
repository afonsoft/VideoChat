using Microsoft.EntityFrameworkCore;
using Volo.Abp;

namespace FamilyMeet.AuditLogs
{
    public static class EnhancedAuditLogDbContextModelCreatingExtensions
    {
        public static void ConfigureEnhancedAuditLogs(this ModelBuilder builder)
        {
            Check.NotNull(builder, nameof(builder));

            builder.Entity<EnhancedAuditLog>(b =>
            {
                b.ToTable("EnhancedAuditLogs");

                b.ConfigureByConvention();

                b.Property(x => x.ApplicationName)
                    .IsRequired()
                    .HasMaxLength(256);

                b.Property(x => x.UserId)
                    .IsRequired()
                    .HasMaxLength(256);

                b.Property(x => x.UserName)
                    .IsRequired()
                    .HasMaxLength(256);

                b.Property(x => x.TenantId)
                    .HasMaxLength(256);

                b.Property(x => x.ExecutionTime)
                    .IsRequired()
                    .HasMaxLength(50);

                b.Property(x => x.ExecutionDuration)
                    .HasMaxLength(50);

                b.Property(x => x.ClientIpAddress)
                    .HasMaxLength(64);

                b.Property(x => x.ClientName)
                    .HasMaxLength(256);

                b.Property(x => x.CorrelationId)
                    .HasMaxLength(128);

                b.Property(x => x.RequestMethod)
                    .IsRequired()
                    .HasMaxLength(16);

                b.Property(x => x.RequestUrl)
                    .HasMaxLength(2048);

                b.Property(x => x.Action)
                    .IsRequired()
                    .HasMaxLength(256);

                b.Property(x => x.ControllerName)
                    .IsRequired()
                    .HasMaxLength(256);

                b.Property(x => x.Exception)
                    .HasMaxLength(512);

                b.Property(x => x.ExceptionMessage)
                    .HasMaxLength(2000);

                b.Property(x => x.ExceptionStackTrace)
                    .HasMaxLength(4000);

                b.Property(x => x.Category)
                    .IsRequired()
                    .HasMaxLength(256);

                b.Property(x => x.LogLevel)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasDefaultValue("Information");

                b.Property(x => x.Message)
                    .IsRequired()
                    .HasMaxLength(4000);

                b.Property(x => x.AdditionalData)
                    .HasMaxLength(4000);

                b.Property(x => x.BrowserInfo)
                    .HasMaxLength(512);

                b.Property(x => x.OperatingSystem)
                    .HasMaxLength(256);

                b.Property(x => x.DeviceInfo)
                    .HasMaxLength(256);

                b.Property(x => x.SessionId)
                    .HasMaxLength(128);

                b.Property(x => x.Severity)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasDefaultValue("Normal");

                b.Property(x => x.Component)
                    .HasMaxLength(256);

                b.Property(x => x.Module)
                    .HasMaxLength(256);

                b.Property(x => x.Feature)
                    .HasMaxLength(256);

                b.Property(x => x.Tags)
                    .HasMaxLength(1000);

                b.Property(x => x.RetentionPolicy)
                    .HasMaxLength(50)
                    .HasDefaultValue("Monthly");

                // Índices para performance
                b.HasIndex(x => x.ApplicationName);
                b.HasIndex(x => x.UserId);
                b.HasIndex(x => x.TenantId);
                b.HasIndex(x => x.ExecutionTime);
                b.HasIndex(x => x.Category);
                b.HasIndex(x => x.LogLevel);
                b.HasIndex(x => x.Action);
                b.HasIndex(x => x.IsSuccess);
                b.HasIndex(x => x.HasException);
                b.HasIndex(x => x.Severity);
                b.HasIndex(x => x.IsArchived);
                b.HasIndex(x => new { x.ApplicationName, x.Category, x.ExecutionTime });
            });

            builder.Entity<AuditLogCategory>(b =>
            {
                b.ToTable("AuditLogCategories");

                b.ConfigureByConvention();

                b.Property(x => x.Name)
                    .IsRequired()
                    .HasMaxLength(256);

                b.Property(x => x.DisplayName)
                    .IsRequired()
                    .HasMaxLength(256);

                b.Property(x => x.Description)
                    .HasMaxLength(1000);

                b.Property(x => x.Color)
                    .HasMaxLength(50)
                    .HasDefaultValue("#1890ff");

                b.Property(x => x.Icon)
                    .HasMaxLength(256)
                    .HasDefaultValue("audit");

                b.Property(x => x.TenantId)
                    .HasMaxLength(256);

                // Índices para performance
                b.HasIndex(x => x.Name).IsUnique();
                b.HasIndex(x => x.IsEnabled);
                b.HasIndex(x => x.TenantId);
                b.HasIndex(x => x.Sort);
            });

            builder.Entity<AuditLogRetention>(b =>
            {
                b.ToTable("AuditLogRetentions");

                b.ConfigureByConvention();

                b.Property(x => x.PolicyName)
                    .IsRequired()
                    .HasMaxLength(256);

                b.Property(x => x.Description)
                    .HasMaxLength(1000);

                b.Property(x => x.RetentionDays)
                    .IsRequired();

                b.Property(x => x.CategoryName)
                    .HasMaxLength(256);

                b.Property(x => x.LogLevel)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasDefaultValue("Information");

                b.Property(x => x.TenantId)
                    .HasMaxLength(256);

                // Índices para performance
                b.HasIndex(x => x.PolicyName).IsUnique();
                b.HasIndex(x => x.IsEnabled);
                b.HasIndex(x => x.TenantId);
                b.HasIndex(x => x.CategoryName);
            });
        }
    }
}
