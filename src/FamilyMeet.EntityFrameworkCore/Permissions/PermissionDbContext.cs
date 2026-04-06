using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace FamilyMeet.Permissions
{
    public class PermissionDbContext : AbpDbContext<PermissionDbContext>
    {
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<PermissionGroup> PermissionGroups { get; set; }
        public DbSet<UserPermission> UserPermissions { get; set; }

        public PermissionDbContext(DbContextOptions<PermissionDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.ConfigurePermissions();
        }
    }
}
