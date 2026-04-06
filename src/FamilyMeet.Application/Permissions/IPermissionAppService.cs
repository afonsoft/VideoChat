using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace FamilyMeet.Permissions
{
    public interface IPermissionAppService : IApplicationService
    {
        Task<PagedResultDto<PermissionDto>> GetListAsync(GetPermissionsInput input);
        Task<PermissionDto> GetAsync(Guid id);
        Task<PermissionDto> GetByNameAsync(string name);
        Task<PermissionDto> CreateAsync(CreatePermissionDto input);
        Task<PermissionDto> UpdateAsync(Guid id, UpdatePermissionDto input);
        Task DeleteAsync(Guid id);
        Task<PermissionDto> EnableAsync(Guid id);
        Task<PermissionDto> DisableAsync(Guid id);
        Task<bool> CheckPermissionAsync(string userId, string permissionName, string scope = null);
        Task<List<PermissionGroupDto>> GetGroupsAsync();
        Task<List<PermissionDto>> GetByGroupAsync(string groupName);
        Task<List<UserPermissionDto>> GetUserPermissionsAsync(string userId);
        Task<UserPermissionDto> GrantPermissionAsync(string userId, string permissionName, string providerName = "User");
        Task<UserPermissionDto> RevokePermissionAsync(string userId, string permissionName);
        Task<PermissionStatisticsDto> GetStatisticsAsync();
    }
}
