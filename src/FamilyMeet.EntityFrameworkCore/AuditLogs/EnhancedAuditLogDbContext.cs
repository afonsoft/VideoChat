using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;

namespace FamilyMeet.AuditLogs
{
    public class EnhancedAuditLogDbContext : AbpDbContext<EnhancedAuditLogDbContext>
    {
        public DbSet<EnhancedAuditLog> EnhancedAuditLogs { get; set; }
        public DbSet<AuditLogCategory> AuditLogCategories { get; set; }
        public DbSet<AuditLogRetention> AuditLogRetentions { get; set; }

        public EnhancedAuditLogDbContext(DbContextOptions<EnhancedAuditLogDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.ConfigureEnhancedAuditLogs();
        }
    }
}
