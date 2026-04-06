using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace FamilyMeet.Permissions
{
    public class PermissionAppService : ApplicationService, IPermissionAppService
    {
        private readonly IRepository<Permission, Guid> _permissionRepository;
        private readonly IRepository<PermissionGroup, Guid> _permissionGroupRepository;
        private readonly IRepository<UserPermission, Guid> _userPermissionRepository;

        public PermissionAppService(
            IRepository<Permission, Guid> permissionRepository,
            IRepository<PermissionGroup, Guid> permissionGroupRepository,
            IRepository<UserPermission, Guid> userPermissionRepository)
        {
            _permissionRepository = permissionRepository;
            _permissionGroupRepository = permissionGroupRepository;
            _userPermissionRepository = userPermissionRepository;
        }

        public async Task<PagedResultDto<PermissionDto>> GetListAsync(GetPermissionsInput input)
        {
            var queryable = await _permissionRepository.GetQueryableAsync();

            queryable = queryable
                .WhereIf(!input.GroupName.IsNullOrEmpty(), x => x.GroupName == input.GroupName)
                .WhereIf(!input.Category.IsNullOrEmpty(), x => x.Category == input.Category)
                .WhereIf(input.IsEnabled.HasValue, x => x.IsEnabled == input.IsEnabled.Value)
                .WhereIf(input.IsStatic.HasValue, x => x.IsStatic == input.IsStatic.Value)
                .WhereIf(!input.Scope.IsNullOrEmpty(), x => x.Scope == input.Scope)
                .WhereIf(!input.Filter.IsNullOrEmpty(), x => 
                    x.Name.Contains(input.Filter) || 
                    x.DisplayName.Contains(input.Filter) ||
                    x.Description.Contains(input.Filter));

            var totalCount = await queryable.CountAsync();
            var items = await queryable
                .OrderBy(x => x.Sort)
                .ThenBy(x => x.GroupName)
                .ThenBy(x => x.Name)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount)
                .ToListAsync();

            return new PagedResultDto<PermissionDto>(
                totalCount,
                ObjectMapper.Map<List<Permission>, List<PermissionDto>>(items)
            );
        }

        public async Task<PermissionDto> GetAsync(Guid id)
        {
            var permission = await _permissionRepository.GetAsync(id);
            return ObjectMapper.Map<Permission, PermissionDto>(permission);
        }

        public async Task<PermissionDto> GetByNameAsync(string name)
        {
            var queryable = await _permissionRepository.GetQueryableAsync();
            var permission = await queryable.FirstOrDefaultAsync(x => x.Name == name);
            
            if (permission == null)
            {
                throw new BusinessException("Permission:NotFound", $"Permission '{name}' not found");
            }

            return ObjectMapper.Map<Permission, PermissionDto>(permission);
        }

        public async Task<PermissionDto> CreateAsync(CreatePermissionDto input)
        {
            var existingPermission = await _permissionRepository.FirstOrDefaultAsync(x => x.Name == input.Name);
            if (existingPermission != null)
            {
                throw new BusinessException("Permission:AlreadyExists", $"Permission '{input.Name}' already exists");
            }

            var permission = new Permission(
                GuidGenerator.Create(),
                input.Name,
                input.DisplayName,
                input.Description,
                input.GroupName,
                input.Category,
                input.IsEnabled ?? true,
                input.IsStatic ?? false,
                input.ResourceName,
                input.Action,
                input.Scope ?? "Global"
            );

            permission.TenantId = CurrentTenant.Id?.ToString();
            permission.Sort = input.Sort ?? 0;
            permission.ParentPermissionName = input.ParentPermissionName;

            await _permissionRepository.InsertAsync(permission);
            return ObjectMapper.Map<Permission, PermissionDto>(permission);
        }

        public async Task<PermissionDto> UpdateAsync(Guid id, UpdatePermissionDto input)
        {
            var permission = await _permissionRepository.GetAsync(id);

            permission.DisplayName = input.DisplayName;
            permission.Description = input.Description;
            permission.GroupName = input.GroupName;
            permission.Category = input.Category;
            permission.IsEnabled = input.IsEnabled;
            permission.ResourceName = input.ResourceName;
            permission.Action = input.Action;
            permission.Scope = input.Scope;
            permission.Sort = input.Sort;

            await _permissionRepository.UpdateAsync(permission);
            return ObjectMapper.Map<Permission, PermissionDto>(permission);
        }

        public async Task DeleteAsync(Guid id)
        {
            await _permissionRepository.DeleteAsync(id);
        }

        public async Task<PermissionDto> EnableAsync(Guid id)
        {
            var permission = await _permissionRepository.GetAsync(id);
            permission.Enable();
            await _permissionRepository.UpdateAsync(permission);
            return ObjectMapper.Map<Permission, PermissionDto>(permission);
        }

        public async Task<PermissionDto> DisableAsync(Guid id)
        {
            var permission = await _permissionRepository.GetAsync(id);
            permission.Disable();
            await _permissionRepository.UpdateAsync(permission);
            return ObjectMapper.Map<Permission, PermissionDto>(permission);
        }

        public async Task<bool> CheckPermissionAsync(string userId, string permissionName, string scope = null)
        {
            var queryable = await _permissionRepository.GetQueryableAsync();
            var permission = await queryable.FirstOrDefaultAsync(x => x.Name == permissionName && x.IsEnabled);

            if (permission == null)
                return false;

            // Check user-specific permission
            var userPermissionQueryable = await _userPermissionRepository.GetQueryableAsync();
            var userPermission = await userPermissionQueryable
                .Where(x => x.UserId == userId && x.PermissionName == permissionName)
                .FirstOrDefaultAsync();

            if (userPermission != null)
                return userPermission.IsGranted;

            // Default to permission's enabled status
            return permission.IsEnabled;
        }

        public async Task<List<PermissionGroupDto>> GetGroupsAsync()
        {
            var queryable = await _permissionGroupRepository.GetQueryableAsync();
            var groups = await queryable
                .Where(x => x.IsEnabled)
                .OrderBy(x => x.Sort)
                .ThenBy(x => x.Name)
                .ToListAsync();

            return ObjectMapper.Map<List<PermissionGroup>, List<PermissionGroupDto>>(groups);
        }

        public async Task<List<PermissionDto>> GetByGroupAsync(string groupName)
        {
            var queryable = await _permissionRepository.GetQueryableAsync();
            var permissions = await queryable
                .Where(x => x.GroupName == groupName && x.IsEnabled)
                .OrderBy(x => x.Sort)
                .ThenBy(x => x.DisplayName)
                .ToListAsync();

            return ObjectMapper.Map<List<Permission>, List<PermissionDto>>(permissions);
        }

        public async Task<List<UserPermissionDto>> GetUserPermissionsAsync(string userId)
        {
            var queryable = await _userPermissionRepository.GetQueryableAsync();
            var userPermissions = await queryable
                .Where(x => x.UserId == userId)
                .ToListAsync();

            return ObjectMapper.Map<List<UserPermission>, List<UserPermissionDto>>(userPermissions);
        }

        public async Task<UserPermissionDto> GrantPermissionAsync(string userId, string permissionName, string providerName = "User")
        {
            var queryable = await _userPermissionRepository.GetQueryableAsync();
            var existingUserPermission = await queryable
                .Where(x => x.UserId == userId && x.PermissionName == permissionName)
                .FirstOrDefaultAsync();

            if (existingUserPermission != null)
            {
                existingUserPermission.Grant();
                await _userPermissionRepository.UpdateAsync(existingUserPermission);
                return ObjectMapper.Map<UserPermission, UserPermissionDto>(existingUserPermission);
            }

            var userPermission = new UserPermission(
                GuidGenerator.Create(),
                userId,
                permissionName,
                true,
                providerName
            );

            userPermission.TenantId = CurrentTenant.Id?.ToString();

            await _userPermissionRepository.InsertAsync(userPermission);
            return ObjectMapper.Map<UserPermission, UserPermissionDto>(userPermission);
        }

        public async Task<UserPermissionDto> RevokePermissionAsync(string userId, string permissionName)
        {
            var queryable = await _userPermissionRepository.GetQueryableAsync();
            var userPermission = await queryable
                .Where(x => x.UserId == userId && x.PermissionName == permissionName)
                .FirstOrDefaultAsync();

            if (userPermission == null)
            {
                throw new BusinessException("UserPermission:NotFound", $"User permission '{permissionName}' not found for user '{userId}'");
            }

            userPermission.Revoke();
            await _userPermissionRepository.UpdateAsync(userPermission);
            return ObjectMapper.Map<UserPermission, UserPermissionDto>(userPermission);
        }

        public async Task<PermissionStatisticsDto> GetStatisticsAsync()
        {
            var permissionQueryable = await _permissionRepository.GetQueryableAsync();
            var userPermissionQueryable = await _userPermissionRepository.GetQueryableAsync();

            var totalPermissions = await permissionQueryable.CountAsync();
            var enabledPermissions = await permissionQueryable.CountAsync(x => x.IsEnabled);
            var staticPermissions = await permissionQueryable.CountAsync(x => x.IsStatic);
            var totalUserPermissions = await userPermissionQueryable.CountAsync();
            var grantedUserPermissions = await userPermissionQueryable.CountAsync(x => x.IsGranted);

            return new PermissionStatisticsDto
            {
                TotalPermissions = totalPermissions,
                EnabledPermissions = enabledPermissions,
                DisabledPermissions = totalPermissions - enabledPermissions,
                StaticPermissions = staticPermissions,
                DynamicPermissions = totalPermissions - staticPermissions,
                TotalUserPermissions = totalUserPermissions,
                GrantedUserPermissions = grantedUserPermissions,
                RevokedUserPermissions = totalUserPermissions - grantedUserPermissions
            };
        }
    }
}
