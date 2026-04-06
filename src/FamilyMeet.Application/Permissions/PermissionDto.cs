using System;
using Volo.Abp.Application.Dtos;

namespace FamilyMeet.Permissions
{
    public class PermissionDto : EntityDto<Guid>
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
        public DateTime CreationTime { get; set; }
        public DateTime? LastModificationTime { get; set; }
    }

    public class PermissionGroupDto : EntityDto<Guid>
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public bool IsEnabled { get; set; }
        public string TenantId { get; set; }
        public int Sort { get; set; }
        public string Icon { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime? LastModificationTime { get; set; }
    }

    public class UserPermissionDto : EntityDto<Guid>
    {
        public string UserId { get; set; }
        public string PermissionName { get; set; }
        public bool IsGranted { get; set; }
        public string TenantId { get; set; }
        public string ProviderName { get; set; }
        public string ProviderKey { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime? LastModificationTime { get; set; }
    }

    public class CreatePermissionDto
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public string GroupName { get; set; }
        public string Category { get; set; }
        public bool? IsEnabled { get; set; }
        public bool? IsStatic { get; set; }
        public string ResourceName { get; set; }
        public string Action { get; set; }
        public string Scope { get; set; }
        public int? Sort { get; set; }
        public string ParentPermissionName { get; set; }
    }

    public class UpdatePermissionDto
    {
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public string GroupName { get; set; }
        public string Category { get; set; }
        public bool IsEnabled { get; set; }
        public string ResourceName { get; set; }
        public string Action { get; set; }
        public string Scope { get; set; }
        public int Sort { get; set; }
    }

    public class GetPermissionsInput : PagedAndSortedResultRequestDto
    {
        public string GroupName { get; set; }
        public string Category { get; set; }
        public bool? IsEnabled { get; set; }
        public bool? IsStatic { get; set; }
        public string Scope { get; set; }
        public string Filter { get; set; }
    }

    public class PermissionStatisticsDto
    {
        public int TotalPermissions { get; set; }
        public int EnabledPermissions { get; set; }
        public int DisabledPermissions { get; set; }
        public int StaticPermissions { get; set; }
        public int DynamicPermissions { get; set; }
        public int TotalUserPermissions { get; set; }
        public int GrantedUserPermissions { get; set; }
        public int RevokedUserPermissions { get; set; }
    }

    public class PermissionCheckDto
    {
        public string UserId { get; set; }
        public string PermissionName { get; set; }
        public string Scope { get; set; }
    }

    public class PermissionGrantDto
    {
        public string UserId { get; set; }
        public string PermissionName { get; set; }
        public bool IsGranted { get; set; }
        public string ProviderName { get; set; }
    }

    public class BatchPermissionGrantDto
    {
        public string UserId { get; set; }
        public List<string> PermissionNames { get; set; }
        public bool IsGranted { get; set; }
        public string ProviderName { get; set; }
    }
}
