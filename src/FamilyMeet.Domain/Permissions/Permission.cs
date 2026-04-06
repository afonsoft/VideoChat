using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace FamilyMeet.Permissions
{
    public class Permission : FullAuditedAggregateRoot<Guid>
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public string GroupName { get; set; }
        public string Category { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsStatic { get; set; }
        public string TenantId { get; set; }
        public int Sort { get; set; }
        public string ParentPermissionName { get; set; }
        public string ResourceName { get; set; }
        public string Action { get; set; }
        public string Scope { get; set; }

        protected Permission()
        {
        }

        public Permission(
            Guid id,
            string name,
            string displayName,
            string description = null,
            string groupName = null,
            string category = null,
            bool isEnabled = true,
            bool isStatic = false,
            string resourceName = null,
            string action = null,
            string scope = "Global")
            : base(id)
        {
            Name = name;
            DisplayName = displayName;
            Description = description;
            GroupName = groupName;
            Category = category;
            IsEnabled = isEnabled;
            IsStatic = isStatic;
            ResourceName = resourceName;
            Action = action;
            Scope = scope;
            Sort = 0;
        }

        public void Enable()
        {
            if (IsStatic)
            {
                throw new InvalidOperationException("Cannot enable static permission");
            }
            IsEnabled = true;
        }

        public void Disable()
        {
            if (IsStatic)
            {
                throw new InvalidOperationException("Cannot disable static permission");
            }
            IsEnabled = false;
        }

        public void UpdateDetails(string displayName, string description, string category = null)
        {
            if (IsStatic)
            {
                throw new InvalidOperationException("Cannot update static permission");
            }

            DisplayName = displayName;
            Description = description;
            if (!string.IsNullOrEmpty(category))
            {
                Category = category;
            }
        }

        public void SetGroup(string groupName)
        {
            if (IsStatic)
            {
                throw new InvalidOperationException("Cannot change group of static permission");
            }
            GroupName = groupName;
        }

        public void SetSortOrder(int sort)
        {
            Sort = sort;
        }

        public void SetParent(string parentPermissionName)
        {
            if (IsStatic)
            {
                throw new InvalidOperationException("Cannot change parent of static permission");
            }
            ParentPermissionName = parentPermissionName;
        }

        public bool HasAccess(string action, string scope = null)
        {
            if (!IsEnabled)
                return false;

            if (!string.IsNullOrEmpty(scope) && Scope != scope)
                return false;

            if (!string.IsNullOrEmpty(action) && Action != action)
                return false;

            return true;
        }
    }

    public class PermissionGroup : FullAuditedAggregateRoot<Guid>
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public bool IsEnabled { get; set; }
        public string TenantId { get; set; }
        public int Sort { get; set; }
        public string Icon { get; set; }

        protected PermissionGroup()
        {
        }

        public PermissionGroup(
            Guid id,
            string name,
            string displayName,
            string description = null,
            bool isEnabled = true,
            string icon = null)
            : base(id)
        {
            Name = name;
            DisplayName = displayName;
            Description = description;
            IsEnabled = isEnabled;
            Icon = icon;
            Sort = 0;
        }

        public void Enable()
        {
            IsEnabled = true;
        }

        public void Disable()
        {
            IsEnabled = false;
        }

        public void UpdateDetails(string displayName, string description, string icon = null)
        {
            DisplayName = displayName;
            Description = description;
            if (!string.IsNullOrEmpty(icon))
            {
                Icon = icon;
            }
        }

        public void SetSortOrder(int sort)
        {
            Sort = sort;
        }
    }

    public class UserPermission : FullAuditedEntity<Guid>
    {
        public string UserId { get; set; }
        public string PermissionName { get; set; }
        public bool IsGranted { get; set; }
        public string TenantId { get; set; }
        public string ProviderName { get; set; }
        public string ProviderKey { get; set; }

        protected UserPermission()
        {
        }

        public UserPermission(
            Guid id,
            string userId,
            string permissionName,
            bool isGranted = true,
            string providerName = "User",
            string providerKey = null)
            : base(id)
        {
            UserId = userId;
            PermissionName = permissionName;
            IsGranted = isGranted;
            ProviderName = providerName;
            ProviderKey = providerKey;
        }

        public void Grant()
        {
            IsGranted = true;
        }

        public void Revoke()
        {
            IsGranted = false;
        }

        public void Toggle()
        {
            IsGranted = !IsGranted;
        }
    }
}
