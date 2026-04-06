using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace FamilyMeet.Features
{
    public class FeatureAppService : ApplicationService, IFeatureAppService
    {
        private readonly IRepository<Feature, Guid> _featureRepository;

        public FeatureAppService(IRepository<Feature, Guid> featureRepository)
        {
            _featureRepository = featureRepository;
        }

        public async Task<PagedResultDto<FeatureDto>> GetListAsync(GetFeaturesInput input)
        {
            var queryable = await _featureRepository.GetQueryableAsync();

            queryable = queryable
                .WhereIf(!input.GroupName.IsNullOrEmpty(), x => x.GroupName == input.GroupName)
                .WhereIf(!input.Category.IsNullOrEmpty(), x => x.Category == input.Category)
                .WhereIf(!input.ProviderName.IsNullOrEmpty(), x => x.ProviderName == input.ProviderName)
                .WhereIf(input.IsEnabled.HasValue, x => x.IsEnabled == input.IsEnabled.Value)
                .WhereIf(input.IsVisible.HasValue, x => x.IsVisible == input.IsVisible.Value)
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

            return new PagedResultDto<FeatureDto>(
                totalCount,
                ObjectMapper.Map<List<Feature>, List<FeatureDto>>(items)
            );
        }

        public async Task<FeatureDto> GetAsync(Guid id)
        {
            var feature = await _featureRepository.GetAsync(id);
            return ObjectMapper.Map<Feature, FeatureDto>(feature);
        }

        public async Task<FeatureDto> GetByNameAsync(string name)
        {
            var queryable = await _featureRepository.GetQueryableAsync();
            var feature = await queryable.FirstOrDefaultAsync(x => x.Name == name);
            
            if (feature == null)
            {
                throw new BusinessException("Feature:NotFound", $"Feature '{name}' not found");
            }

            return ObjectMapper.Map<Feature, FeatureDto>(feature);
        }

        public async Task<FeatureDto> CreateAsync(CreateFeatureDto input)
        {
            var existingFeature = await _featureRepository.FirstOrDefaultAsync(x => x.Name == input.Name);
            if (existingFeature != null)
            {
                throw new BusinessException("Feature:AlreadyExists", $"Feature '{input.Name}' already exists");
            }

            var feature = new Feature(
                GuidGenerator.Create(),
                input.Name,
                input.DisplayName,
                input.Description,
                input.IsEnabled ?? true,
                input.ValueType ?? "Boolean",
                input.DefaultValue ?? "true",
                input.ProviderName ?? "Global",
                input.GroupName,
                input.Category,
                input.Sort ?? 0
            );

            feature.TenantId = CurrentTenant.Id?.ToString();
            feature.IsVisible = input.IsVisible ?? true;
            feature.RequiresTenant = input.RequiresTenant ?? false;

            if (!string.IsNullOrEmpty(input.AllowedValues))
            {
                feature.AllowedValues = input.AllowedValues;
            }

            await _featureRepository.InsertAsync(feature);
            return ObjectMapper.Map<Feature, FeatureDto>(feature);
        }

        public async Task<FeatureDto> UpdateAsync(Guid id, UpdateFeatureDto input)
        {
            var feature = await _featureRepository.GetAsync(id);

            feature.DisplayName = input.DisplayName;
            feature.Description = input.Description;
            feature.IsEnabled = input.IsEnabled;
            feature.ValueType = input.ValueType;
            feature.DefaultValue = input.DefaultValue;
            feature.AllowedValues = input.AllowedValues;
            feature.ProviderName = input.ProviderName;
            feature.GroupName = input.GroupName;
            feature.Category = input.Category;
            feature.Sort = input.Sort;
            feature.IsVisible = input.IsVisible;
            feature.RequiresTenant = input.RequiresTenant;

            await _featureRepository.UpdateAsync(feature);
            return ObjectMapper.Map<Feature, FeatureDto>(feature);
        }

        public async Task DeleteAsync(Guid id)
        {
            await _featureRepository.DeleteAsync(id);
        }

        public async Task<FeatureDto> EnableAsync(Guid id)
        {
            var feature = await _featureRepository.GetAsync(id);
            feature.Enable();
            await _featureRepository.UpdateAsync(feature);
            return ObjectMapper.Map<Feature, FeatureDto>(feature);
        }

        public async Task<FeatureDto> DisableAsync(Guid id)
        {
            var feature = await _featureRepository.GetAsync(id);
            feature.Disable();
            await _featureRepository.UpdateAsync(feature);
            return ObjectMapper.Map<Feature, FeatureDto>(feature);
        }

        public async Task<FeatureDto> UpdateValueAsync(Guid id, string newValue)
        {
            var feature = await _featureRepository.GetAsync(id);
            feature.UpdateValue(newValue);
            await _featureRepository.UpdateAsync(feature);
            return ObjectMapper.Map<Feature, FeatureDto>(feature);
        }

        public async Task<List<FeatureGroupDto>> GetGroupsAsync()
        {
            var queryable = await _featureRepository.GetQueryableAsync();
            var groups = await queryable
                .Where(x => x.IsVisible)
                .GroupBy(x => x.GroupName)
                .Select(g => new FeatureGroupDto
                {
                    Name = g.Key,
                    DisplayName = g.Key,
                    FeatureCount = g.Count(),
                    EnabledCount = g.Count(x => x.IsEnabled)
                })
                .OrderBy(x => x.Name)
                .ToListAsync();

            return groups;
        }

        public async Task<List<FeatureDto>> GetByGroupAsync(string groupName)
        {
            var queryable = await _featureRepository.GetQueryableAsync();
            var features = await queryable
                .Where(x => x.GroupName == groupName && x.IsVisible)
                .OrderBy(x => x.Sort)
                .ThenBy(x => x.DisplayName)
                .ToListAsync();

            return ObjectMapper.Map<List<Feature>, List<FeatureDto>>(features);
        }

        public async Task<FeatureValueDto> GetValueAsync(string name, string providerName = null, string tenantId = null)
        {
            var queryable = await _featureRepository.GetQueryableAsync();
            
            var feature = await queryable
                .Where(x => x.Name == name && x.IsEnabled)
                .FirstOrDefaultAsync();

            if (feature == null)
            {
                throw new BusinessException("Feature:NotFound", $"Feature '{name}' not found");
            }

            return new FeatureValueDto
            {
                Name = feature.Name,
                Value = feature.DefaultValue,
                ValueType = feature.ValueType,
                IsEnabled = feature.IsEnabled
            };
        }

        public async Task<bool> IsEnabledAsync(string name, string providerName = null, string tenantId = null)
        {
            var queryable = await _featureRepository.GetQueryableAsync();
            var feature = await queryable
                .Where(x => x.Name == name && x.IsEnabled)
                .FirstOrDefaultAsync();

            return feature != null && feature.IsEnabled;
        }

        public async Task<FeatureStatisticsDto> GetStatisticsAsync()
        {
            var queryable = await _featureRepository.GetQueryableAsync();
            var total = await queryable.CountAsync();
            var enabled = await queryable.CountAsync(x => x.IsEnabled);
            var disabled = total - enabled;
            var visible = await queryable.CountAsync(x => x.IsVisible);
            var hidden = total - visible;

            return new FeatureStatisticsDto
            {
                TotalFeatures = total,
                EnabledFeatures = enabled,
                DisabledFeatures = disabled,
                VisibleFeatures = visible,
                HiddenFeatures = hidden
            };
        }
    }
}
