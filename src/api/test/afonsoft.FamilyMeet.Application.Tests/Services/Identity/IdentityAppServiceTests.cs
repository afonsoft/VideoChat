using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Volo.Abp.Identity;
using Volo.Abp.Modularity;
using Volo.Abp.Uow;
using Xunit;

namespace afonsoft.FamilyMeet.Application.Tests.Services.Identity;

public abstract class IdentityAppServiceTestBase : FamilyMeetApplicationTestBase
{
    protected IIdentityUserAppService IdentityUserAppService { get; }
    protected IIdentityRoleAppService IdentityRoleAppService { get; }
    protected IIdentityClaimTypeAppService IdentityClaimTypeAppService { get; }

    protected IdentityAppServiceTestBase()
    {
        IdentityUserAppService = GetRequiredService<IIdentityUserAppService>();
        IdentityRoleAppService = GetRequiredService<IIdentityRoleAppService>();
        IdentityClaimTypeAppService = GetRequiredService<IIdentityClaimTypeAppService>();
    }
}

public class IdentityUserAppServiceTests : IdentityAppServiceTestBase
{
    [Fact]
    public async Task GetListAsync_Should_Return_Paged_Users()
    {
        // Arrange
        var input = new PagedAndSortedResultRequestDto();

        // Act
        var result = await IdentityUserAppService.GetListAsync(input);

        // Assert
        result.ShouldNotBeNull();
        result.TotalCount.ShouldBeGreaterThanOrEqualTo(0);
        result.Items.ShouldNotBeNull();
    }

    [Fact]
    public async Task CreateAsync_Should_Create_New_User()
    {
        // Arrange
        var input = new IdentityUserCreateDto
        {
            UserName = "testuser",
            Email = "testuser@example.com",
            Name = "Test",
            Surname = "User",
            PhoneNumber = "123456789",
            Password = "Test123!",
            LockoutEnabled = false
        };

        // Act
        var result = await IdentityUserAppService.CreateAsync(input);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldNotBe(System.Guid.Empty);
        result.UserName.ShouldBe("testuser");
        result.Email.ShouldBe("testuser@example.com");
        result.Name.ShouldBe("Test");
        result.Surname.ShouldBe("User");
        result.PhoneNumber.ShouldBe("123456789");
        result.LockoutEnabled.ShouldBeFalse();
    }

    [Fact]
    public async Task UpdateAsync_Should_Update_Existing_User()
    {
        // Arrange
        var createInput = new IdentityUserCreateDto
        {
            UserName = "updatetestuser",
            Email = "updatetest@example.com",
            Name = "Update",
            Surname = "Test",
            PhoneNumber = "987654321",
            Password = "Test123!",
            LockoutEnabled = false
        };

        var createdUser = await IdentityUserAppService.CreateAsync(createInput);

        var updateInput = new IdentityUserUpdateDto
        {
            Name = "Updated",
            Surname = "User",
            PhoneNumber = "555555555",
            LockoutEnabled = true
        };

        // Act
        var result = await IdentityUserAppService.UpdateAsync(createdUser.Id, updateInput);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(createdUser.Id);
        result.Name.ShouldBe("Updated");
        result.Surname.ShouldBe("User");
        result.PhoneNumber.ShouldBe("555555555");
        result.LockoutEnabled.ShouldBeTrue();
    }

    [Fact]
    public async Task DeleteAsync_Should_Delete_User()
    {
        // Arrange
        var createInput = new IdentityUserCreateDto
        {
            UserName = "deletetestuser",
            Email = "deletetest@example.com",
            Name = "Delete",
            Surname = "Test",
            Password = "Test123!"
        };

        var createdUser = await IdentityUserAppService.CreateAsync(createInput);

        // Act
        await IdentityUserAppService.DeleteAsync(createdUser.Id);

        // Assert
        await Should.ThrowAsync<Volo.Abp.Domain.Entities.EntityNotFoundException>(
            () => IdentityUserAppService.GetAsync(createdUser.Id));
    }

    [Fact]
    public async Task GetRolesAsync_Should_Return_User_Roles()
    {
        // Arrange
        var createInput = new IdentityUserCreateDto
        {
            UserName = "rolestestuser",
            Email = "rolestest@example.com",
            Name = "Roles",
            Surname = "Test",
            Password = "Test123!"
        };

        var createdUser = await IdentityUserAppService.CreateAsync(createInput);

        // Act
        var result = await IdentityUserAppService.GetRolesAsync(createdUser.Id);

        // Assert
        result.ShouldNotBeNull();
        result.Items.ShouldNotBeNull();
    }

    [Fact]
    public async Task CreateAsync_Should_Validate_Email_Format()
    {
        // Arrange
        var input = new IdentityUserCreateDto
        {
            UserName = "invalidemailuser",
            Email = "invalid-email",
            Name = "Invalid",
            Surname = "Email",
            Password = "Test123!"
        };

        // Act & Assert
        await Should.ThrowAsync<Volo.Abp.Validation.AbpValidationException>(
            () => IdentityUserAppService.CreateAsync(input));
    }

    [Fact]
    public async Task CreateAsync_Should_Validate_Password_Length()
    {
        // Arrange
        var input = new IdentityUserCreateDto
        {
            UserName = "weakpassworduser",
            Email = "weakpassword@example.com",
            Name = "Weak",
            Surname = "Password",
            Password = "123" // Too short
        };

        // Act & Assert
        await Should.ThrowAsync<Volo.Abp.Validation.AbpValidationException>(
            () => IdentityUserAppService.CreateAsync(input));
    }

    [Fact]
    public async Task CreateAsync_Should_Validate_UserName_Uniqueness()
    {
        // Arrange
        var input1 = new IdentityUserCreateDto
        {
            UserName = "duplicateuser",
            Email = "duplicate1@example.com",
            Name = "Duplicate",
            Surname = "User1",
            Password = "Test123!"
        };

        var input2 = new IdentityUserCreateDto
        {
            UserName = "duplicateuser", // Same username
            Email = "duplicate2@example.com",
            Name = "Duplicate",
            Surname = "User2",
            Password = "Test123!"
        };

        // Act
        await IdentityUserAppService.CreateAsync(input1);

        // Assert
        await Should.ThrowAsync<Volo.Abp.BusinessException>(
            () => IdentityUserAppService.CreateAsync(input2));
    }
}

public class IdentityRoleAppServiceTests : IdentityAppServiceTestBase
{
    [Fact]
    public async Task GetListAsync_Should_Return_Paged_Roles()
    {
        // Arrange
        var input = new PagedAndSortedResultRequestDto();

        // Act
        var result = await IdentityRoleAppService.GetListAsync(input);

        // Assert
        result.ShouldNotBeNull();
        result.TotalCount.ShouldBeGreaterThanOrEqualTo(0);
        result.Items.ShouldNotBeNull();
    }

    [Fact]
    public async Task CreateAsync_Should_Create_New_Role()
    {
        // Arrange
        var input = new IdentityRoleCreateDto
        {
            Name = "TestRole",
            DisplayName = "Test Role Display",
            Description = "Test role description",
            IsStatic = false,
            IsPublic = true
        };

        // Act
        var result = await IdentityRoleAppService.CreateAsync(input);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldNotBe(System.Guid.Empty);
        result.Name.ShouldBe("TestRole");
        result.DisplayName.ShouldBe("Test Role Display");
        result.Description.ShouldBe("Test role description");
        result.IsStatic.ShouldBeFalse();
        result.IsPublic.ShouldBeTrue();
    }

    [Fact]
    public async Task UpdateAsync_Should_Update_Existing_Role()
    {
        // Arrange
        var createInput = new IdentityRoleCreateDto
        {
            Name = "UpdateRole",
            DisplayName = "Update Role Display",
            Description = "Update role description",
            IsStatic = false,
            IsPublic = true
        };

        var createdRole = await IdentityRoleAppService.CreateAsync(createInput);

        var updateInput = new IdentityRoleUpdateDto
        {
            DisplayName = "Updated Role Display",
            Description = "Updated role description",
            IsPublic = false
        };

        // Act
        var result = await IdentityRoleAppService.UpdateAsync(createdRole.Id, updateInput);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(createdRole.Id);
        result.Name.ShouldBe("UpdateRole"); // Name should not change
        result.DisplayName.ShouldBe("Updated Role Display");
        result.Description.ShouldBe("Updated role description");
        result.IsPublic.ShouldBeFalse();
    }

    [Fact]
    public async Task DeleteAsync_Should_Delete_Role()
    {
        // Arrange
        var createInput = new IdentityRoleCreateDto
        {
            Name = "DeleteRole",
            DisplayName = "Delete Role Display",
            Description = "Delete role description",
            IsStatic = false,
            IsPublic = true
        };

        var createdRole = await IdentityRoleAppService.CreateAsync(createInput);

        // Act
        await IdentityRoleAppService.DeleteAsync(createdRole.Id);

        // Assert
        await Should.ThrowAsync<Volo.Abp.Domain.Entities.EntityNotFoundException>(
            () => IdentityRoleAppService.GetAsync(createdRole.Id));
    }

    [Fact]
    public async Task CreateAsync_Should_Validate_Role_Name_Uniqueness()
    {
        // Arrange
        var input1 = new IdentityRoleCreateDto
        {
            Name = "DuplicateRole",
            DisplayName = "Duplicate Role 1",
            Description = "First duplicate role",
            IsStatic = false,
            IsPublic = true
        };

        var input2 = new IdentityRoleCreateDto
        {
            Name = "DuplicateRole", // Same name
            DisplayName = "Duplicate Role 2",
            Description = "Second duplicate role",
            IsStatic = false,
            IsPublic = true
        };

        // Act
        await IdentityRoleAppService.CreateAsync(input1);

        // Assert
        await Should.ThrowAsync<Volo.Abp.BusinessException>(
            () => IdentityRoleAppService.CreateAsync(input2));
    }

    [Fact]
    public async Task CreateAsync_Should_Not_Allow_Static_Role_Creation()
    {
        // Arrange
        var input = new IdentityRoleCreateDto
        {
            Name = "StaticRole",
            DisplayName = "Static Role Display",
            Description = "Static role description",
            IsStatic = true, // Should not be allowed
            IsPublic = true
        };

        // Act & Assert
        await Should.ThrowAsync<Volo.Abp.BusinessException>(
            () => IdentityRoleAppService.CreateAsync(input));
    }
}

public class IdentityClaimTypeAppServiceTests : IdentityAppServiceTestBase
{
    [Fact]
    public async Task GetListAsync_Should_Return_Paged_ClaimTypes()
    {
        // Arrange
        var input = new PagedAndSortedResultRequestDto();

        // Act
        var result = await IdentityClaimTypeAppService.GetListAsync(input);

        // Assert
        result.ShouldNotBeNull();
        result.TotalCount.ShouldBeGreaterThanOrEqualTo(0);
        result.Items.ShouldNotBeNull();
    }

    [Fact]
    public async Task CreateAsync_Should_Create_New_ClaimType()
    {
        // Arrange
        var input = new IdentityClaimTypeCreateDto
        {
            Name = "custom.claim",
            DisplayName = "Custom Claim",
            Description = "Custom claim description",
            Required = false,
            Regex = "^[a-zA-Z0-9]*$",
            ValueType = Volo.Abp.Identity.IdentityClaimValueType.String
        };

        // Act
        var result = await IdentityClaimTypeAppService.CreateAsync(input);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldNotBe(System.Guid.Empty);
        result.Name.ShouldBe("custom.claim");
        result.DisplayName.ShouldBe("Custom Claim");
        result.Description.ShouldBe("Custom claim description");
        result.Required.ShouldBeFalse();
        result.Regex.ShouldBe("^[a-zA-Z0-9]*$");
        result.ValueType.ShouldBe(Volo.Abp.Identity.IdentityClaimValueType.String);
    }

    [Fact]
    public async Task UpdateAsync_Should_Update_Existing_ClaimType()
    {
        // Arrange
        var createInput = new IdentityClaimTypeCreateDto
        {
            Name = "update.claim",
            DisplayName = "Update Claim",
            Description = "Update claim description",
            Required = false,
            ValueType = Volo.Abp.Identity.IdentityClaimValueType.String
        };

        var createdClaimType = await IdentityClaimTypeAppService.CreateAsync(createInput);

        var updateInput = new IdentityClaimTypeUpdateDto
        {
            DisplayName = "Updated Claim Display",
            Description = "Updated claim description",
            Required = true
        };

        // Act
        var result = await IdentityClaimTypeAppService.UpdateAsync(createdClaimType.Id, updateInput);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(createdClaimType.Id);
        result.Name.ShouldBe("update.claim"); // Name should not change
        result.DisplayName.ShouldBe("Updated Claim Display");
        result.Description.ShouldBe("Updated claim description");
        result.Required.ShouldBeTrue();
    }

    [Fact]
    public async Task DeleteAsync_Should_Delete_ClaimType()
    {
        // Arrange
        var createInput = new IdentityClaimTypeCreateDto
        {
            Name = "delete.claim",
            DisplayName = "Delete Claim",
            Description = "Delete claim description",
            Required = false,
            ValueType = Volo.Abp.Identity.IdentityClaimValueType.String
        };

        var createdClaimType = await IdentityClaimTypeAppService.CreateAsync(createInput);

        // Act
        await IdentityClaimTypeAppService.DeleteAsync(createdClaimType.Id);

        // Assert
        await Should.ThrowAsync<Volo.Abp.Domain.Entities.EntityNotFoundException>(
            () => IdentityClaimTypeAppService.GetAsync(createdClaimType.Id));
    }
}
