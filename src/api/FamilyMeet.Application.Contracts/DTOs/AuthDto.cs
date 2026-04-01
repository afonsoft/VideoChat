using FamilyMeet.Domain.Shared.Enums;

namespace FamilyMeet.Application.Contracts.DTOs;

public class UserDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Avatar { get; set; }
    public string? Provider { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsOnline { get; set; }
}

public class CreateUserDto
{
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Avatar { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public string ProviderId { get; set; } = string.Empty;
}
