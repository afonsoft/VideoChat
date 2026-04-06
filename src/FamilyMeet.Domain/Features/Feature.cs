using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace FamilyMeet.Features
{
    public class Feature : FullAuditedAggregateRoot<Guid>
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

        protected Feature()
        {
        }

        public Feature(
            Guid id,
            string name,
            string displayName,
            string description = null,
            bool isEnabled = true,
            string valueType = "Boolean",
            string defaultValue = "true",
            string providerName = "Global",
            string groupName = null,
            string category = null,
            int sort = 0)
            : base(id)
        {
            Name = name;
            DisplayName = displayName;
            Description = description;
            IsEnabled = isEnabled;
            ValueType = valueType;
            DefaultValue = defaultValue;
            ProviderName = providerName;
            GroupName = groupName;
            Category = category;
            Sort = sort;
            IsVisible = true;
            RequiresTenant = false;
        }

        public void Enable()
        {
            IsEnabled = true;
        }

        public void Disable()
        {
            IsEnabled = false;
        }

        public void UpdateValue(string newValue)
        {
            if (!IsValidValue(newValue))
            {
                throw new ArgumentException($"Invalid value '{newValue}' for feature '{Name}'");
            }

            DefaultValue = newValue;
        }

        public bool IsValidValue(string value)
        {
            if (string.IsNullOrEmpty(AllowedValues))
                return true;

            return AllowedValues.Split(',').Contains(value.Trim());
        }

        public void UpdateDetails(string displayName, string description, string category = null)
        {
            DisplayName = displayName;
            Description = description;
            if (!string.IsNullOrEmpty(category))
            {
                Category = category;
            }
        }

        public void SetVisibility(bool isVisible)
        {
            IsVisible = isVisible;
        }

        public void SetTenantRequirement(bool requiresTenant)
        {
            RequiresTenant = requiresTenant;
        }

        public void SetSortOrder(int sort)
        {
            Sort = sort;
        }

        public void SetProvider(string providerName)
        {
            ProviderName = providerName;
        }
    }
}
