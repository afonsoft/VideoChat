using Microsoft.EntityFrameworkCore;
using Volo.Abp;

namespace FamilyMeet.Features
{
    public static class FeatureDbContextModelCreatingExtensions
    {
        public static void ConfigureFeatures(this ModelBuilder builder)
        {
            Check.NotNull(builder, nameof(builder));

            builder.Entity<Feature>(b =>
            {
                b.ToTable("Features");

                b.ConfigureByConvention();

                b.Property(x => x.Name)
                    .IsRequired()
                    .HasMaxLength(256);

                b.Property(x => x.DisplayName)
                    .IsRequired()
                    .HasMaxLength(256);

                b.Property(x => x.Description)
                    .HasMaxLength(1000);

                b.Property(x => x.ValueType)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasDefaultValue("Boolean");

                b.Property(x => x.DefaultValue)
                    .IsRequired()
                    .HasMaxLength(4000);

                b.Property(x => x.AllowedValues)
                    .HasMaxLength(4000);

                b.Property(x => x.ProviderName)
                    .IsRequired()
                    .HasMaxLength(128)
                    .HasDefaultValue("Global");

                b.Property(x => x.TenantId)
                    .HasMaxLength(256);

                b.Property(x => x.GroupName)
                    .HasMaxLength(256);

                b.Property(x => x.Category)
                    .HasMaxLength(256);

                b.Property(x => x.Sort)
                    .HasDefaultValue(0);

                b.Property(x => x.IsVisible)
                    .HasDefaultValue(true);

                b.Property(x => x.RequiresTenant)
                    .HasDefaultValue(false);

                // Índices para performance
                b.HasIndex(x => x.Name);
                b.HasIndex(x => x.IsEnabled);
                b.HasIndex(x => x.ProviderName);
                b.HasIndex(x => x.TenantId);
                b.HasIndex(x => x.GroupName);
                b.HasIndex(x => x.Category);
                b.HasIndex(x => x.Sort);
            });
        }
    }
}
