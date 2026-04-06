using Microsoft.EntityFrameworkCore;
using Volo.Abp;

namespace FamilyMeet.Permissions
{
    public static class PermissionDbContextModelCreatingExtensions
    {
        public static void ConfigurePermissions(this ModelBuilder builder)
        {
            Check.NotNull(builder, nameof(builder));

            builder.Entity<Permission>(b =>
            {
                b.ToTable("Permissions");

                b.ConfigureByConvention();

                b.Property(x => x.Name)
                    .IsRequired()
                    .HasMaxLength(256);

                b.Property(x => x.DisplayName)
                    .IsRequired()
                    .HasMaxLength(256);

                b.Property(x => x.Description)
                    .HasMaxLength(1000);

                b.Property(x => x.GroupName)
                    .HasMaxLength(256);

                b.Property(x => x.Category)
                    .HasMaxLength(256);

                b.Property(x => x.TenantId)
                    .HasMaxLength(256);

                b.Property(x => x.ParentPermissionName)
                    .HasMaxLength(256);

                b.Property(x => x.ResourceName)
                    .HasMaxLength(256);

                b.Property(x => x.Action)
                    .HasMaxLength(128);

                b.Property(x => x.Scope)
                    .IsRequired()
                    .HasMaxLength(128)
                    .HasDefaultValue("Global");

                // Índices para performance
                b.HasIndex(x => x.Name).IsUnique();
                b.HasIndex(x => x.IsEnabled);
                b.HasIndex(x => x.GroupName);
                b.HasIndex(x => x.Category);
                b.HasIndex(x => x.TenantId);
                b.HasIndex(x => x.ResourceName);
                b.HasIndex(x => x.Action);
                b.HasIndex(x => x.Scope);
            });

            builder.Entity<PermissionGroup>(b =>
            {
                b.ToTable("PermissionGroups");

                b.ConfigureByConvention();

                b.Property(x => x.Name)
                    .IsRequired()
                    .HasMaxLength(256);

                b.Property(x => x.DisplayName)
                    .IsRequired()
                    .HasMaxLength(256);

                b.Property(x => x.Description)
                    .HasMaxLength(1000);

                b.Property(x => x.TenantId)
                    .HasMaxLength(256);

                b.Property(x => x.Icon)
                    .HasMaxLength(256);

                // Índices para performance
                b.HasIndex(x => x.Name).IsUnique();
                b.HasIndex(x => x.IsEnabled);
                b.HasIndex(x => x.TenantId);
            });

            builder.Entity<UserPermission>(b =>
            {
                b.ToTable("UserPermissions");

                b.ConfigureByConvention();

                b.Property(x => x.UserId)
                    .IsRequired()
                    .HasMaxLength(256);

                b.Property(x => x.PermissionName)
                    .IsRequired()
                    .HasMaxLength(256);

                b.Property(x => x.TenantId)
                    .HasMaxLength(256);

                b.Property(x => x.ProviderName)
                    .IsRequired()
                    .HasMaxLength(128)
                    .HasDefaultValue("User");

                b.Property(x => x.ProviderKey)
                    .HasMaxLength(256);

                // Índices para performance
                b.HasIndex(x => new { x.UserId, x.PermissionName, x.TenantId }).IsUnique();
                b.HasIndex(x => x.UserId);
                b.HasIndex(x => x.PermissionName);
                b.HasIndex(x => x.TenantId);
                b.HasIndex(x => x.ProviderName);
                b.HasIndex(x => x.IsGranted);
            });
        }
    }
}
