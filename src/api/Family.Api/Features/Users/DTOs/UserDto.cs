namespace Family.Api.Features.Users.DTOs;

/// <summary>
/// Data Transfer Object for User information
/// </summary>
public class UserDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PreferredLanguage { get; set; } = "de";
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public string? KeycloakSubjectId { get; set; }
    public List<UserRoleDto> Roles { get; set; } = [];
}

/// <summary>
/// Data Transfer Object for User Role information
/// </summary>
public class UserRoleDto
{
    public Guid Id { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Data Transfer Object for creating a new user
/// </summary>
public class CreateUserDto
{
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PreferredLanguage { get; set; } = "de";
    public string? KeycloakSubjectId { get; set; }
}

/// <summary>
/// Data Transfer Object for updating user information
/// </summary>
public class UpdateUserDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PreferredLanguage { get; set; } = "de";
    public bool IsActive { get; set; }
}