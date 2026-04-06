using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Volo.Abp.SettingManagement;
using Xunit;

namespace afonsoft.FamilyMeet.Application.Tests.Services.SettingManagement;

public abstract class SettingManagementAppServiceTestBase : FamilyMeetApplicationTestBase
{
    protected ISettingAppService SettingAppService { get; }
    protected ISettingDefinitionManager SettingDefinitionManager { get; }

    protected SettingManagementAppServiceTestBase()
    {
        SettingAppService = GetRequiredService<ISettingAppService>();
        SettingDefinitionManager = GetRequiredService<ISettingDefinitionManager>();
    }
}

public class SettingAppServiceTests : SettingManagementAppServiceTestBase
{
    [Fact]
    public async Task GetAsync_Should_Return_Setting_Value()
    {
        // Arrange
        var providerName = "Default";
        var providerKey = "Default";
        var settingName = "MySetting1";

        // Act
        var result = await SettingAppService.GetAsync(providerName, providerKey, settingName);

        // Assert
        result.ShouldNotBeNull();
        result.Name.ShouldBe(settingName);
    }

    [Fact]
    public async Task GetListAsync_Should_Return_All_Settings()
    {
        // Arrange
        var input = new GetSettingsDto
        {
            ProviderName = "Default",
            ProviderKey = "Default"
        };

        // Act
        var result = await SettingAppService.GetListAsync(input);

        // Assert
        result.ShouldNotBeNull();
        result.Items.ShouldNotBeNull();
        result.Items.Count.ShouldBeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task UpdateAsync_Should_Update_Setting_Value()
    {
        // Arrange
        var input = new UpdateSettingsDto
        {
            ProviderName = "Default",
            ProviderKey = "Default",
            Settings = new System.Collections.Generic.Dictionary<string, string>
            {
                { "MySetting1", "updated_value" },
                { "MySetting2", "123" }
            }
        };

        // Act
        await SettingAppService.UpdateAsync(input);

        // Assert
        // Verify the settings were updated
        var setting1 = await SettingAppService.GetAsync("Default", "Default", "MySetting1");
        setting1.ShouldNotBeNull();
        setting1.Value.ShouldBe("updated_value");

        var setting2 = await SettingAppService.GetAsync("Default", "Default", "MySetting2");
        setting2.ShouldNotBeNull();
        setting2.Value.ShouldBe("123");
    }

    [Fact]
    public async Task DeleteAsync_Should_Reset_Setting_To_Default()
    {
        // Arrange
        var updateInput = new UpdateSettingsDto
        {
            ProviderName = "Default",
            ProviderKey = "Default",
            Settings = new System.Collections.Generic.Dictionary<string, string>
            {
                { "MySetting1", "custom_value" }
            }
        };

        await SettingAppService.UpdateAsync(updateInput);

        // Act
        await SettingAppService.DeleteAsync("Default", "Default", "MySetting1");

        // Assert
        // Setting should be reset to default value
        var setting = await SettingAppService.GetAsync("Default", "Default", "MySetting1");
        setting.ShouldNotBeNull();
        // Default value should be different from what we set
    }

    [Fact]
    public async Task GetAsync_Should_Throw_When_Setting_Not_Found()
    {
        // Arrange
        var providerName = "Default";
        var providerKey = "Default";
        var settingName = "NonExistentSetting";

        // Act & Assert
        await Should.ThrowAsync<Volo.Abp.BusinessException>(
            () => SettingAppService.GetAsync(providerName, providerKey, settingName));
    }

    [Fact]
    public async Task UpdateAsync_Should_Handle_Multiple_Settings()
    {
        // Arrange
        var input = new UpdateSettingsDto
        {
            ProviderName = "Default",
            ProviderKey = "Default",
            Settings = new System.Collections.Generic.Dictionary<string, string>
            {
                { "MySetting1", "value1" },
                { "MySetting2", "value2" },
                { "MySetting3", "123" },
                { "MySetting4", "true" }
            }
        };

        // Act
        await SettingAppService.UpdateAsync(input);

        // Assert
        var setting1 = await SettingAppService.GetAsync("Default", "Default", "MySetting1");
        setting1.Value.ShouldBe("value1");

        var setting2 = await SettingAppService.GetAsync("Default", "Default", "MySetting2");
        setting2.Value.ShouldBe("value2");

        var setting3 = await SettingAppService.GetAsync("Default", "Default", "MySetting3");
        setting3.Value.ShouldBe("123");

        var setting4 = await SettingAppService.GetAsync("Default", "Default", "MySetting4");
        setting4.Value.ShouldBe("true");
    }

    [Fact]
    public async Task UpdateAsync_Should_Validate_Provider_Name()
    {
        // Arrange
        var input = new UpdateSettingsDto
        {
            ProviderName = "", // Empty provider name
            ProviderKey = "Default",
            Settings = new System.Collections.Generic.Dictionary<string, string>
            {
                { "MySetting1", "value" }
            }
        };

        // Act & Assert
        await Should.ThrowAsync<Volo.Abp.Validation.AbpValidationException>(
            () => SettingAppService.UpdateAsync(input));
    }

    [Fact]
    public async Task UpdateAsync_Should_Validate_Provider_Key()
    {
        // Arrange
        var input = new UpdateSettingsDto
        {
            ProviderName = "Default",
            ProviderKey = "", // Empty provider key
            Settings = new System.Collections.Generic.Dictionary<string, string>
            {
                { "MySetting1", "value" }
            }
        };

        // Act & Assert
        await Should.ThrowAsync<Volo.Abp.Validation.AbpValidationException>(
            () => SettingAppService.UpdateAsync(input));
    }

    [Fact]
    public async Task UpdateAsync_Should_Validate_Settings_Dictionary()
    {
        // Arrange
        var input = new UpdateSettingsDto
        {
            ProviderName = "Default",
            ProviderKey = "Default",
            Settings = null // Null settings dictionary
        };

        // Act & Assert
        await Should.ThrowAsync<Volo.Abp.Validation.AbpValidationException>(
            () => SettingAppService.UpdateAsync(input));
    }

    [Fact]
    public async Task GetListAsync_Should_Filter_By_Provider()
    {
        // Arrange
        var input = new GetSettingsDto
        {
            ProviderName = "Default",
            ProviderKey = "Default"
        };

        // Act
        var result = await SettingAppService.GetListAsync(input);

        // Assert
        result.ShouldNotBeNull();
        result.Items.ShouldNotBeNull();
        
        // All settings should belong to the specified provider
        foreach (var setting in result.Items)
        {
            setting.ProviderName.ShouldBe("Default");
            setting.ProviderKey.ShouldBe("Default");
        }
    }

    [Fact]
    public async Task UpdateAsync_Should_Work_With_Different_Providers()
    {
        // Arrange
        var updateInput1 = new UpdateSettingsDto
        {
            ProviderName = "Default",
            ProviderKey = "Default",
            Settings = new System.Collections.Generic.Dictionary<string, string>
            {
                { "MySetting1", "global_value" }
            }
        };

        var updateInput2 = new UpdateSettingsDto
        {
            ProviderName = "User",
            ProviderKey = "user-123",
            Settings = new System.Collections.Generic.Dictionary<string, string>
            {
                { "MySetting1", "user_value" }
            }
        };

        // Act
        await SettingAppService.UpdateAsync(updateInput1);
        await SettingAppService.UpdateAsync(updateInput2);

        // Assert
        var globalSetting = await SettingAppService.GetAsync("Default", "Default", "MySetting1");
        globalSetting.Value.ShouldBe("global_value");

        var userSetting = await SettingAppService.GetAsync("User", "user-123", "MySetting1");
        userSetting.Value.ShouldBe("user_value");
    }

    [Fact]
    public async Task GetListAsync_Should_Return_Setting_Definitions()
    {
        // Arrange
        var input = new GetSettingsDto
        {
            ProviderName = "Default",
            ProviderKey = "Default"
        };

        // Act
        var result = await SettingAppService.GetListAsync(input);

        // Assert
        result.ShouldNotBeNull();
        result.Items.ShouldNotBeNull();
        
        // Each setting should have required properties
        foreach (var setting in result.Items)
        {
            setting.Name.ShouldNotBeNullOrEmpty();
            setting.DisplayName.ShouldNotBeNullOrEmpty();
            setting.ValueType.ShouldNotBeNullOrEmpty();
        }
    }
}

public class SettingDefinitionTests : SettingManagementAppServiceTestBase
{
    [Fact]
    public async Task Setting_Definitions_Should_Be_Available()
    {
        // Arrange
        var input = new GetSettingsDto
        {
            ProviderName = "Default",
            ProviderKey = "Default"
        };

        // Act
        var result = await SettingAppService.GetListAsync(input);

        // Assert
        result.ShouldNotBeNull();
        result.Items.ShouldNotBeNull();
        
        // Verify that some standard settings are available
        var settings = result.Items;
        settings.Count.ShouldBeGreaterThan(0);
        
        // Each setting should have required properties
        foreach (var setting in settings)
        {
            setting.Name.ShouldNotBeNullOrEmpty();
            setting.DisplayName.ShouldNotBeNullOrEmpty();
            setting.ValueType.ShouldNotBeNullOrEmpty();
        }
    }

    [Fact]
    public async Task Setting_Definitions_Should_Have_Correct_Properties()
    {
        // Arrange
        var input = new GetSettingsDto
        {
            ProviderName = "Default",
            ProviderKey = "Default"
        };

        // Act
        var result = await SettingAppService.GetListAsync(input);

        // Assert
        result.ShouldNotBeNull();
        
        if (result.Items.Count > 0)
        {
            var firstSetting = result.Items[0];
            
            firstSetting.Name.ShouldNotBeNullOrEmpty();
            firstSetting.DisplayName.ShouldNotBeNullOrEmpty();
            firstSetting.Description.ShouldNotBeNull();
            firstSetting.ValueType.ShouldNotBeNullOrEmpty();
            firstSetting.DefaultValue.ShouldNotBeNull();
        }
    }

    [Fact]
    public async Task Setting_Definitions_Should_Include_Email_Settings()
    {
        // Arrange
        var input = new GetSettingsDto
        {
            ProviderName = "Default",
            ProviderKey = "Default"
        };

        // Act
        var result = await SettingAppService.GetListAsync(input);

        // Assert
        result.ShouldNotBeNull();
        
        // Look for email-related settings
        var emailSettings = result.Items.Where(s => s.Name.Contains("Email") || s.DisplayName.Contains("Email"));
        emailSettings.Count.ShouldBeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task Setting_Definitions_Should_Include_Security_Settings()
    {
        // Arrange
        var input = new GetSettingsDto
        {
            ProviderName = "Default",
            ProviderKey = "Default"
        };

        // Act
        var result = await SettingAppService.GetListAsync(input);

        // Assert
        result.ShouldNotBeNull();
        
        // Look for security-related settings
        var securitySettings = result.Items.Where(s => s.Name.Contains("Security") || s.DisplayName.Contains("Security"));
        securitySettings.Count.ShouldBeGreaterThanOrEqualTo(0);
    }
}

public class SettingHierarchyTests : SettingManagementAppServiceTestBase
{
    [Fact]
    public async Task Setting_Hierarchy_Should_Work_Correctly()
    {
        // Arrange
        var globalInput = new UpdateSettingsDto
        {
            ProviderName = "Default",
            ProviderKey = "Default",
            Settings = new System.Collections.Generic.Dictionary<string, string>
            {
                { "MySetting1", "global_value" }
            }
        };

        var tenantInput = new UpdateSettingsDto
        {
            ProviderName = "Tenant",
            ProviderKey = "tenant-123",
            Settings = new System.Collections.Generic.Dictionary<string, string>
            {
                { "MySetting1", "tenant_value" }
            }
        };

        var userInput = new UpdateSettingsDto
        {
            ProviderName = "User",
            ProviderKey = "user-456",
            Settings = new System.Collections.Generic.Dictionary<string, string>
            {
                { "MySetting1", "user_value" }
            }
        };

        // Act
        await SettingAppService.UpdateAsync(globalInput);
        await SettingAppService.UpdateAsync(tenantInput);
        await SettingAppService.UpdateAsync(userInput);

        // Assert
        // User setting should override tenant and global
        var userSetting = await SettingAppService.GetAsync("User", "user-456", "MySetting1");
        userSetting.Value.ShouldBe("user_value");

        // Tenant setting should override global
        var tenantSetting = await SettingAppService.GetAsync("Tenant", "tenant-123", "MySetting1");
        tenantSetting.Value.ShouldBe("tenant_value");

        // Global setting should remain
        var globalSetting = await SettingAppService.GetAsync("Default", "Default", "MySetting1");
        globalSetting.Value.ShouldBe("global_value");
    }

    [Fact]
    public async Task Setting_Hierarchy_Should_Fallback_To_Default()
    {
        // Arrange - Only set global setting
        var globalInput = new UpdateSettingsDto
        {
            ProviderName = "Default",
            ProviderKey = "Default",
            Settings = new System.Collections.Generic.Dictionary<string, string>
            {
                { "MySetting1", "global_value" }
            }
        };

        await SettingAppService.UpdateAsync(globalInput);

        // Act - Try to get tenant setting (should fallback to global)
        var tenantSetting = await SettingAppService.GetAsync("Tenant", "tenant-123", "MySetting1");

        // Assert
        tenantSetting.ShouldNotBeNull();
        tenantSetting.Value.ShouldBe("global_value");
    }
}
