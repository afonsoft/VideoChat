using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace FamilyMeet.Features
{
    public class FeatureDbContext : AbpDbContext<FeatureDbContext>
    {
        public DbSet<Feature> Features { get; set; }

        public FeatureDbContext(DbContextOptions<FeatureDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.ConfigureFeatures();
        }
    }
}
