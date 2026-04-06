using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace FamilyMeet.Features
{
    public interface IFeatureAppService : IApplicationService
    {
        Task<PagedResultDto<FeatureDto>> GetListAsync(GetFeaturesInput input);
        Task<FeatureDto> GetAsync(Guid id);
        Task<FeatureDto> GetByNameAsync(string name);
        Task<FeatureDto> CreateAsync(CreateFeatureDto input);
        Task<FeatureDto> UpdateAsync(Guid id, UpdateFeatureDto input);
        Task DeleteAsync(Guid id);
        Task<FeatureDto> EnableAsync(Guid id);
        Task<FeatureDto> DisableAsync(Guid id);
        Task<FeatureDto> UpdateValueAsync(Guid id, string newValue);
        Task<List<FeatureGroupDto>> GetGroupsAsync();
        Task<List<FeatureDto>> GetByGroupAsync(string groupName);
        Task<FeatureValueDto> GetValueAsync(string name, string providerName = null, string tenantId = null);
        Task<bool> IsEnabledAsync(string name, string providerName = null, string tenantId = null);
        Task<FeatureStatisticsDto> GetStatisticsAsync();
    }
}
