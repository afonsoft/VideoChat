using System;
using Volo.Abp.Application.Dtos;

namespace FamilyMeet.Features
{
    public class FeatureDto : EntityDto<Guid>
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public bool IsEnabled { get; set; }
        public string ValueType { get; set; }
        public string DefaultValue { get; set; }
        public string AllowedValues { get; set; }
        public string ProviderName { get; set; }
        public string TenantId { get; set; }
        public string GroupName { get; set; }
        public string Category { get; set; }
        public int Sort { get; set; }
        public bool IsVisible { get; set; }
        public bool RequiresTenant { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime? LastModificationTime { get; set; }
    }

    public class CreateFeatureDto
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public bool? IsEnabled { get; set; }
        public string ValueType { get; set; }
        public string DefaultValue { get; set; }
        public string AllowedValues { get; set; }
        public string ProviderName { get; set; }
        public string GroupName { get; set; }
        public string Category { get; set; }
        public int? Sort { get; set; }
        public bool? IsVisible { get; set; }
        public bool? RequiresTenant { get; set; }
    }

    public class UpdateFeatureDto
    {
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public bool IsEnabled { get; set; }
        public string ValueType { get; set; }
        public string DefaultValue { get; set; }
        public string AllowedValues { get; set; }
        public string ProviderName { get; set; }
        public string GroupName { get; set; }
        public string Category { get; set; }
        public int Sort { get; set; }
        public bool IsVisible { get; set; }
        public bool RequiresTenant { get; set; }
    }

    public class GetFeaturesInput : PagedAndSortedResultRequestDto
    {
        public string GroupName { get; set; }
        public string Category { get; set; }
        public string ProviderName { get; set; }
        public bool? IsEnabled { get; set; }
        public bool? IsVisible { get; set; }
        public string Filter { get; set; }
    }

    public class FeatureGroupDto
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public int FeatureCount { get; set; }
        public int EnabledCount { get; set; }
    }

    public class FeatureValueDto
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public string ValueType { get; set; }
        public bool IsEnabled { get; set; }
    }

    public class FeatureStatisticsDto
    {
        public int TotalFeatures { get; set; }
        public int EnabledFeatures { get; set; }
        public int DisabledFeatures { get; set; }
        public int VisibleFeatures { get; set; }
        public int HiddenFeatures { get; set; }
    }

    public class FeatureToggleDto
    {
        public Guid Id { get; set; }
        public bool IsEnabled { get; set; }
    }

    public class FeatureValueUpdateDto
    {
        public string Value { get; set; }
    }
}
