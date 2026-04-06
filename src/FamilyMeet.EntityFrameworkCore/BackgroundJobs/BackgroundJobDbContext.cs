using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace FamilyMeet.BackgroundJobs
{
    public class BackgroundJobDbContext : AbpDbContext<BackgroundJobDbContext>
    {
        public DbSet<BackgroundJob> BackgroundJobs { get; set; }

        public BackgroundJobDbContext(DbContextOptions<BackgroundJobDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.ConfigureBackgroundJobs();
        }
    }
}
