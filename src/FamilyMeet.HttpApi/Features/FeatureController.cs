using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.Application.Dtos;

namespace FamilyMeet.Features
{
    [ApiController]
    [Route("api/features")]
    public class FeatureController : AbpControllerBase
    {
        private readonly IFeatureAppService _featureAppService;

        public FeatureController(IFeatureAppService featureAppService)
        {
            _featureAppService = featureAppService;
        }

        [HttpGet]
        public async Task<PagedResultDto<FeatureDto>> GetListAsync([FromQuery] GetFeaturesInput input)
        {
            return await _featureAppService.GetListAsync(input);
        }

        [HttpGet("{id}")]
        public async Task<FeatureDto> GetAsync(Guid id)
        {
            return await _featureAppService.GetAsync(id);
        }

        [HttpGet("by-name/{name}")]
        public async Task<FeatureDto> GetByNameAsync(string name)
        {
            return await _featureAppService.GetByNameAsync(name);
        }

        [HttpPost]
        public async Task<FeatureDto> CreateAsync([FromBody] CreateFeatureDto input)
        {
            return await _featureAppService.CreateAsync(input);
        }

        [HttpPut("{id}")]
        public async Task<FeatureDto> UpdateAsync(Guid id, [FromBody] UpdateFeatureDto input)
        {
            return await _featureAppService.UpdateAsync(id, input);
        }

        [HttpDelete("{id}")]
        public async Task DeleteAsync(Guid id)
        {
            await _featureAppService.DeleteAsync(id);
        }

        [HttpPost("{id}/enable")]
        public async Task<FeatureDto> EnableAsync(Guid id)
        {
            return await _featureAppService.EnableAsync(id);
        }

        [HttpPost("{id}/disable")]
        public async Task<FeatureDto> DisableAsync(Guid id)
        {
            return await _featureAppService.DisableAsync(id);
        }

        [HttpPut("{id}/value")]
        public async Task<FeatureDto> UpdateValueAsync(Guid id, [FromBody] FeatureValueUpdateDto input)
        {
            return await _featureAppService.UpdateValueAsync(id, input.Value);
        }

        [HttpGet("groups")]
        public async Task<List<FeatureGroupDto>> GetGroupsAsync()
        {
            return await _featureAppService.GetGroupsAsync();
        }

        [HttpGet("by-group/{groupName}")]
        public async Task<List<FeatureDto>> GetByGroupAsync(string groupName)
        {
            return await _featureAppService.GetByGroupAsync(groupName);
        }

        [HttpGet("value/{name}")]
        public async Task<FeatureValueDto> GetValueAsync(string name, [FromQuery] string providerName = null, [FromQuery] string tenantId = null)
        {
            return await _featureAppService.GetValueAsync(name, providerName, tenantId);
        }

        [HttpGet("is-enabled/{name}")]
        public async Task<bool> IsEnabledAsync(string name, [FromQuery] string providerName = null, [FromQuery] string tenantId = null)
        {
            return await _featureAppService.IsEnabledAsync(name, providerName, tenantId);
        }

        [HttpGet("statistics")]
        public async Task<FeatureStatisticsDto> GetStatisticsAsync()
        {
            return await _featureAppService.GetStatisticsAsync();
        }

        [HttpPost("batch-toggle")]
        public async Task<List<FeatureDto>> BatchToggleAsync([FromBody] List<FeatureToggleDto> input)
        {
            var results = new List<FeatureDto>();
            
            foreach (var toggle in input)
            {
                if (toggle.IsEnabled)
                {
                    results.Add(await _featureAppService.EnableAsync(toggle.Id));
                }
                else
                {
                    results.Add(await _featureAppService.DisableAsync(toggle.Id));
                }
            }
            
            return results;
        }

        [HttpPost("reset-to-defaults")]
        public async Task<List<FeatureDto>> ResetToDefaultsAsync([FromBody] List<Guid> featureIds)
        {
            var results = new List<FeatureDto>();
            
            foreach (var id in featureIds)
            {
                // TODO: Implement reset to default values
                // This would require storing original default values
                var feature = await _featureAppService.GetAsync(id);
                results.Add(feature);
            }
            
            return results;
        }

        [HttpGet("export")]
        public async Task<FileResult> ExportAsync([FromQuery] string format = "json")
        {
            // TODO: Implement feature export functionality
            var features = await _featureAppService.GetListAsync(new GetFeaturesInput { MaxResultCount = 1000 });
            
            if (format.ToLower() == "json")
            {
                var json = System.Text.Json.JsonSerializer.Serialize(features.Items);
                var bytes = System.Text.Encoding.UTF8.GetBytes(json);
                return File(bytes, "application/json", "features.json");
            }
            
            throw new NotSupportedException($"Export format '{format}' is not supported");
        }

        [HttpPost("import")]
        public async Task<List<FeatureDto>> ImportAsync([FromQuery] bool overwrite = false)
        {
            // TODO: Implement feature import functionality
            // This would handle file upload and parsing
            return new List<FeatureDto>();
        }
    }
}
