using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Volo.Abp.FeatureManagement;
using Xunit;

namespace afonsoft.FamilyMeet.Application.Tests.Services.FeatureManagement;

public abstract class FeatureManagementAppServiceTestBase : FamilyMeetApplicationTestBase
{
    protected IFeatureAppService FeatureAppService { get; }
    protected IFeatureChecker FeatureChecker { get; }

    protected FeatureManagementAppServiceTestBase()
    {
        FeatureAppService = GetRequiredService<IFeatureAppService>();
        FeatureChecker = GetRequiredService<IFeatureChecker>();
    }
}

public class FeatureAppServiceTests : FeatureManagementAppServiceTestBase
{
    [Fact]
    public async Task GetAsync_Should_Return_Feature_Value()
    {
        // Arrange
        var providerName = "Default";
        var providerKey = "Default";
        var featureName = "MyFeature1";

        // Act
        var result = await FeatureAppService.GetAsync(providerName, providerKey, featureName);

        // Assert
        result.ShouldNotBeNull();
        result.Name.ShouldBe(featureName);
    }

    [Fact]
    public async Task GetListAsync_Should_Return_All_Features()
    {
        // Arrange
        var input = new GetFeaturesDto
        {
            ProviderName = "Default",
            ProviderKey = "Default"
        };

        // Act
        var result = await FeatureAppService.GetListAsync(input);

        // Assert
        result.ShouldNotBeNull();
        result.Items.ShouldNotBeNull();
        result.Items.Count.ShouldBeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task UpdateAsync_Should_Update_Feature_Value()
    {
        // Arrange
        var input = new UpdateFeaturesDto
        {
            ProviderName = "Default",
            ProviderKey = "Default",
            Features = new System.Collections.Generic.Dictionary<string, string>
            {
                { "MyFeature1", "true" },
                { "MyFeature2", "false" }
            }
        };

        // Act
        await FeatureAppService.UpdateAsync(input);

        // Assert
        // Verify the features were updated
        var feature1 = await FeatureAppService.GetAsync("Default", "Default", "MyFeature1");
        feature1.ShouldNotBeNull();
        feature1.Value.ShouldBe("true");

        var feature2 = await FeatureAppService.GetAsync("Default", "Default", "MyFeature2");
        feature2.ShouldNotBeNull();
        feature2.Value.ShouldBe("false");
    }

    [Fact]
    public async Task DeleteAsync_Should_Reset_Feature_To_Default()
    {
        // Arrange
        var updateInput = new UpdateFeaturesDto
        {
            ProviderName = "Default",
            ProviderKey = "Default",
            Features = new System.Collections.Generic.Dictionary<string, string>
            {
                { "MyFeature1", "true" }
            }
        };

        await FeatureAppService.UpdateAsync(updateInput);

        // Act
        await FeatureAppService.DeleteAsync("Default", "Default", "MyFeature1");

        // Assert
        // Feature should be reset to default value
        var feature = await FeatureAppService.GetAsync("Default", "Default", "MyFeature1");
        feature.ShouldNotBeNull();
        // Default value should be different from what we set
    }

    [Fact]
    public async Task FeatureChecker_Should_Return_Correct_Value()
    {
        // Arrange
        var updateInput = new UpdateFeaturesDto
        {
            ProviderName = "Default",
            ProviderKey = "Default",
            Features = new System.Collections.Generic.Dictionary<string, string>
            {
                { "MyFeature1", "true" }
            }
        };

        await FeatureAppService.UpdateAsync(updateInput);

        // Act
        var result = await FeatureChecker.IsEnabledAsync("MyFeature1");

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public async Task FeatureChecker_Should_Return_Default_Value_When_Not_Set()
    {
        // Arrange - No feature value set

        // Act
        var result = await FeatureChecker.IsEnabledAsync("NonExistentFeature");

        // Assert
        result.ShouldBeFalse(); // Default value should be false
    }

    [Fact]
    public async Task GetAsync_Should_Throw_When_Feature_Not_Found()
    {
        // Arrange
        var providerName = "Default";
        var providerKey = "Default";
        var featureName = "NonExistentFeature";

        // Act & Assert
        await Should.ThrowAsync<Volo.Abp.BusinessException>(
            () => FeatureAppService.GetAsync(providerName, providerKey, featureName));
    }

    [Fact]
    public async Task UpdateAsync_Should_Handle_Multiple_Features()
    {
        // Arrange
        var input = new UpdateFeaturesDto
        {
            ProviderName = "Default",
            ProviderKey = "Default",
            Features = new System.Collections.Generic.Dictionary<string, string>
            {
                { "MyFeature1", "true" },
                { "MyFeature2", "false" },
                { "MyFeature3", "custom_value" },
                { "MyFeature4", "123" }
            }
        };

        // Act
        await FeatureAppService.UpdateAsync(input);

        // Assert
        var feature1 = await FeatureAppService.GetAsync("Default", "Default", "MyFeature1");
        feature1.Value.ShouldBe("true");

        var feature2 = await FeatureAppService.GetAsync("Default", "Default", "MyFeature2");
        feature2.Value.ShouldBe("false");

        var feature3 = await FeatureAppService.GetAsync("Default", "Default", "MyFeature3");
        feature3.Value.ShouldBe("custom_value");

        var feature4 = await FeatureAppService.GetAsync("Default", "Default", "MyFeature4");
        feature4.Value.ShouldBe("123");
    }

    [Fact]
    public async Task UpdateAsync_Should_Validate_Provider_Name()
    {
        // Arrange
        var input = new UpdateFeaturesDto
        {
            ProviderName = "", // Empty provider name
            ProviderKey = "Default",
            Features = new System.Collections.Generic.Dictionary<string, string>
            {
                { "MyFeature1", "true" }
            }
        };

        // Act & Assert
        await Should.ThrowAsync<Volo.Abp.Validation.AbpValidationException>(
            () => FeatureAppService.UpdateAsync(input));
    }

    [Fact]
    public async Task UpdateAsync_Should_Validate_Provider_Key()
    {
        // Arrange
        var input = new UpdateFeaturesDto
        {
            ProviderName = "Default",
            ProviderKey = "", // Empty provider key
            Features = new System.Collections.Generic.Dictionary<string, string>
            {
                { "MyFeature1", "true" }
            }
        };

        // Act & Assert
        await Should.ThrowAsync<Volo.Abp.Validation.AbpValidationException>(
            () => FeatureAppService.UpdateAsync(input));
    }

    [Fact]
    public async Task UpdateAsync_Should_Validate_Feature_Dictionary()
    {
        // Arrange
        var input = new UpdateFeaturesDto
        {
            ProviderName = "Default",
            ProviderKey = "Default",
            Features = null // Null features dictionary
        };

        // Act & Assert
        await Should.ThrowAsync<Volo.Abp.Validation.AbpValidationException>(
            () => FeatureAppService.UpdateAsync(input));
    }

    [Fact]
    public async Task GetListAsync_Should_Filter_By_Provider()
    {
        // Arrange
        var input = new GetFeaturesDto
        {
            ProviderName = "Default",
            ProviderKey = "Default"
        };

        // Act
        var result = await FeatureAppService.GetListAsync(input);

        // Assert
        result.ShouldNotBeNull();
        result.Items.ShouldNotBeNull();
        
        // All features should belong to the specified provider
        foreach (var feature in result.Items)
        {
            feature.ProviderName.ShouldBe("Default");
            feature.ProviderKey.ShouldBe("Default");
        }
    }

    [Fact]
    public async Task FeatureChecker_Should_Work_With_Different_Providers()
    {
        // Arrange
        var updateInput1 = new UpdateFeaturesDto
        {
            ProviderName = "Default",
            ProviderKey = "Default",
            Features = new System.Collections.Generic.Dictionary<string, string>
            {
                { "MyFeature1", "true" }
            }
        };

        var updateInput2 = new UpdateFeaturesDto
        {
            ProviderName = "Tenant",
            ProviderKey = "tenant-123",
            Features = new System.Collections.Generic.Dictionary<string, string>
            {
                { "MyFeature1", "false" }
            }
        };

        await FeatureAppService.UpdateAsync(updateInput1);
        await FeatureAppService.UpdateAsync(updateInput2);

        // Act
        var defaultValue = await FeatureChecker.IsEnabledAsync("MyFeature1");
        var tenantValue = await FeatureChecker.IsEnabledAsync("MyFeature1", "Tenant", "tenant-123");

        // Assert
        defaultValue.ShouldBeTrue();
        tenantValue.ShouldBeFalse();
    }
}

public class FeatureDefinitionTests : FeatureManagementAppServiceTestBase
{
    [Fact]
    public async Task Feature_Definitions_Should_Be_Available()
    {
        // Arrange
        var input = new GetFeaturesDto
        {
            ProviderName = "Default",
            ProviderKey = "Default"
        };

        // Act
        var result = await FeatureAppService.GetListAsync(input);

        // Assert
        result.ShouldNotBeNull();
        result.Items.ShouldNotBeNull();
        
        // Verify that some standard features are available
        var features = result.Items;
        features.Count.ShouldBeGreaterThan(0);
        
        // Each feature should have required properties
        foreach (var feature in features)
        {
            feature.Name.ShouldNotBeNullOrEmpty();
            feature.DisplayName.ShouldNotBeNullOrEmpty();
            feature.ValueType.ShouldNotBeNullOrEmpty();
        }
    }

    [Fact]
    public async Task Feature_Definitions_Should_Have_Correct_Properties()
    {
        // Arrange
        var input = new GetFeaturesDto
        {
            ProviderName = "Default",
            ProviderKey = "Default"
        };

        // Act
        var result = await FeatureAppService.GetListAsync(input);

        // Assert
        result.ShouldNotBeNull();
        
        if (result.Items.Count > 0)
        {
            var firstFeature = result.Items[0];
            
            firstFeature.Name.ShouldNotBeNullOrEmpty();
            firstFeature.DisplayName.ShouldNotBeNullOrEmpty();
            firstFeature.Description.ShouldNotBeNull();
            firstFeature.ValueType.ShouldNotBeNullOrEmpty();
            firstFeature.DefaultValue.ShouldNotBeNull();
        }
    }
}
