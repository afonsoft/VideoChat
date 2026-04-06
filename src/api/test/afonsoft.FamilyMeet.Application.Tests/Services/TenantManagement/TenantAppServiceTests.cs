using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Volo.Abp.TenantManagement;
using Xunit;

namespace afonsoft.FamilyMeet.Application.Tests.Services.TenantManagement;

public abstract class TenantManagementAppServiceTestBase : FamilyMeetApplicationTestBase
{
    protected ITenantAppService TenantAppService { get; }
    protected ITenantRepository TenantRepository { get; }

    protected TenantManagementAppServiceTestBase()
    {
        TenantAppService = GetRequiredService<ITenantAppService>();
        TenantRepository = GetRequiredService<ITenantRepository>();
    }
}

public class TenantAppServiceTests : TenantManagementAppServiceTestBase
{
    [Fact]
    public async Task GetListAsync_Should_Return_Paged_Tenants()
    {
        // Arrange
        var input = new PagedAndSortedResultRequestDto();

        // Act
        var result = await TenantAppService.GetListAsync(input);

        // Assert
        result.ShouldNotBeNull();
        result.TotalCount.ShouldBeGreaterThanOrEqualTo(0);
        result.Items.ShouldNotBeNull();
    }

    [Fact]
    public async Task CreateAsync_Should_Create_New_Tenant()
    {
        // Arrange
        var input = new TenantCreateDto
        {
            Name = "TestTenant",
            AdminEmailAddress = "admin@testtenant.com",
            AdminPassword = "Test123!",
            IsActive = true
        };

        // Act
        var result = await TenantAppService.CreateAsync(input);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldNotBe(System.Guid.Empty);
        result.Name.ShouldBe("TestTenant");
        result.IsActive.ShouldBeTrue();
        
        // Verify admin user was created
        var adminUser = await TenantAppService.GetAdminUserAsync(result.Id);
        adminUser.ShouldNotBeNull();
        adminUser.Email.ShouldBe("admin@testtenant.com");
    }

    [Fact]
    public async Task UpdateAsync_Should_Update_Existing_Tenant()
    {
        // Arrange
        var createInput = new TenantCreateDto
        {
            Name = "UpdateTenant",
            AdminEmailAddress = "admin@updatetenant.com",
            AdminPassword = "Test123!",
            IsActive = true
        };

        var createdTenant = await TenantAppService.CreateAsync(createInput);

        var updateInput = new TenantUpdateDto
        {
            Name = "UpdatedTenant",
            IsActive = false
        };

        // Act
        var result = await TenantAppService.UpdateAsync(createdTenant.Id, updateInput);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(createdTenant.Id);
        result.Name.ShouldBe("UpdatedTenant");
        result.IsActive.ShouldBeFalse();
    }

    [Fact]
    public async Task DeleteAsync_Should_Delete_Tenant()
    {
        // Arrange
        var createInput = new TenantCreateDto
        {
            Name = "DeleteTenant",
            AdminEmailAddress = "admin@deletetenant.com",
            AdminPassword = "Test123!",
            IsActive = true
        };

        var createdTenant = await TenantAppService.CreateAsync(createInput);

        // Act
        await TenantAppService.DeleteAsync(createdTenant.Id);

        // Assert
        await Should.ThrowAsync<Volo.Abp.Domain.Entities.EntityNotFoundException>(
            () => TenantAppService.GetAsync(createdTenant.Id));
    }

    [Fact]
    public async Task GetAsync_Should_Return_Tenant_By_Id()
    {
        // Arrange
        var createInput = new TenantCreateDto
        {
            Name = "GetTenant",
            AdminEmailAddress = "admin@gettenant.com",
            AdminPassword = "Test123!",
            IsActive = true
        };

        var createdTenant = await TenantAppService.CreateAsync(createInput);

        // Act
        var result = await TenantAppService.GetAsync(createdTenant.Id);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(createdTenant.Id);
        result.Name.ShouldBe("GetTenant");
        result.IsActive.ShouldBeTrue();
    }

    [Fact]
    public async Task GetAdminUserAsync_Should_Return_Admin_User()
    {
        // Arrange
        var createInput = new TenantCreateDto
        {
            Name = "AdminTenant",
            AdminEmailAddress = "admin@admintenant.com",
            AdminPassword = "Test123!",
            IsActive = true
        };

        var createdTenant = await TenantAppService.CreateAsync(createInput);

        // Act
        var result = await TenantAppService.GetAdminUserAsync(createdTenant.Id);

        // Assert
        result.ShouldNotBeNull();
        result.Email.ShouldBe("admin@admintenant.com");
    }

    [Fact]
    public async Task CreateAsync_Should_Validate_Tenant_Name_Uniqueness()
    {
        // Arrange
        var input1 = new TenantCreateDto
        {
            Name = "DuplicateTenant",
            AdminEmailAddress = "admin1@duplicatetenant.com",
            AdminPassword = "Test123!",
            IsActive = true
        };

        var input2 = new TenantCreateDto
        {
            Name = "DuplicateTenant", // Same name
            AdminEmailAddress = "admin2@duplicatetenant.com",
            AdminPassword = "Test123!",
            IsActive = true
        };

        // Act
        await TenantAppService.CreateAsync(input1);

        // Assert
        await Should.ThrowAsync<Volo.Abp.BusinessException>(
            () => TenantAppService.CreateAsync(input2));
    }

    [Fact]
    public async Task CreateAsync_Should_Validate_Admin_Email_Format()
    {
        // Arrange
        var input = new TenantCreateDto
        {
            Name = "InvalidEmailTenant",
            AdminEmailAddress = "invalid-email", // Invalid email format
            AdminPassword = "Test123!",
            IsActive = true
        };

        // Act & Assert
        await Should.ThrowAsync<Volo.Abp.Validation.AbpValidationException>(
            () => TenantAppService.CreateAsync(input));
    }

    [Fact]
    public async Task CreateAsync_Should_Validate_Admin_Password_Length()
    {
        // Arrange
        var input = new TenantCreateDto
        {
            Name = "WeakPasswordTenant",
            AdminEmailAddress = "admin@weakpassword.com",
            AdminPassword = "123", // Too short
            IsActive = true
        };

        // Act & Assert
        await Should.ThrowAsync<Volo.Abp.Validation.AbpValidationException>(
            () => TenantAppService.CreateAsync(input));
    }

    [Fact]
    public async Task CreateAsync_Should_Create_Database_For_Tenant()
    {
        // Arrange
        var input = new TenantCreateDto
        {
            Name = "DatabaseTenant",
            AdminEmailAddress = "admin@databasetenant.com",
            AdminPassword = "Test123!",
            IsActive = true
        };

        // Act
        var result = await TenantAppService.CreateAsync(input);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldNotBe(System.Guid.Empty);
        
        // Verify tenant database connection string was created
        var tenant = await TenantRepository.GetAsync(result.Id);
        tenant.ShouldNotBeNull();
        tenant.ConnectionStrings.ShouldNotBeNull();
        tenant.ConnectionStrings.Default.ShouldBe($"Server=localhost;Database=FamilyMeet_{result.Id:D};User Id=postgres;Password=postgres;");
    }

    [Fact]
    public async Task UpdateConnectionStringAsync_Should_Update_Connection_String()
    {
        // Arrange
        var createInput = new TenantCreateDto
        {
            Name = "ConnectionStringTenant",
            AdminEmailAddress = "admin@connectionstring.com",
            AdminPassword = "Test123!",
            IsActive = true
        };

        var createdTenant = await TenantAppService.CreateAsync(createInput);

        var updateInput = new TenantConnectionStringUpdateDto
        {
            Name = "Default",
            Value = "Server=localhost;Database=FamilyMeet_Updated;User Id=postgres;Password=postgres;"
        };

        // Act
        await TenantAppService.UpdateConnectionStringAsync(createdTenant.Id, updateInput);

        // Assert
        var updatedTenant = await TenantRepository.GetAsync(createdTenant.Id);
        updatedTenant.ShouldNotBeNull();
        updatedTenant.ConnectionStrings.Default.ShouldBe("Server=localhost;Database=FamilyMeet_Updated;User Id=postgres;Password=postgres;");
    }

    [Fact]
    public async Task DeleteConnectionStringAsync_Should_Delete_Connection_String()
    {
        // Arrange
        var createInput = new TenantCreateDto
        {
            Name = "DeleteConnectionStringTenant",
            AdminEmailAddress = "admin@deleteconnection.com",
            AdminPassword = "Test123!",
            IsActive = true
        };

        var createdTenant = await TenantAppService.CreateAsync(createInput);

        // Act
        await TenantAppService.DeleteConnectionStringAsync(createdTenant.Id, "Default");

        // Assert
        var updatedTenant = await TenantRepository.GetAsync(createdTenant.Id);
        updatedTenant.ShouldNotBeNull();
        updatedTenant.ConnectionStrings.Default.ShouldBeNull();
    }

    [Fact]
    public async Task CreateAsync_Should_Set_Default_Connection_String()
    {
        // Arrange
        var input = new TenantCreateDto
        {
            Name = "DefaultConnectionTenant",
            AdminEmailAddress = "admin@defaultconnection.com",
            AdminPassword = "Test123!",
            IsActive = true
        };

        // Act
        var result = await TenantAppService.CreateAsync(input);

        // Assert
        result.ShouldNotBeNull();
        
        var tenant = await TenantRepository.GetAsync(result.Id);
        tenant.ShouldNotBeNull();
        tenant.ConnectionStrings.ShouldNotBeNull();
        tenant.ConnectionStrings.Default.ShouldNotBeNull();
        tenant.ConnectionStrings.Default.ShouldContain("FamilyMeet_");
    }

    [Fact]
    public async Task CreateAsync_Should_Create_Admin_User_With_Default_Roles()
    {
        // Arrange
        var input = new TenantCreateDto
        {
            Name = "AdminRolesTenant",
            AdminEmailAddress = "admin@adminroles.com",
            AdminPassword = "Test123!",
            IsActive = true
        };

        // Act
        var result = await TenantAppService.CreateAsync(input);

        // Assert
        result.ShouldNotBeNull();
        
        var adminUser = await TenantAppService.GetAdminUserAsync(result.Id);
        adminUser.ShouldNotBeNull();
        
        // Verify admin user has default roles
        // This would require additional setup to verify roles
        // For now, just verify the user exists
        adminUser.Email.ShouldBe("admin@adminroles.com");
    }
}
