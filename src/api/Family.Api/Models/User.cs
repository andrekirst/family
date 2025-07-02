using Family.Api.Models.Base;
using System.ComponentModel.DataAnnotations;

namespace Family.Api.Models;

public class User : BaseEntity
{

    [Required]
    [MaxLength(255)]
    public string KeycloakSubjectId { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(320)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? FirstName { get; set; }

    [MaxLength(100)]
    public string? LastName { get; set; }

    [MaxLength(10)]
    public string? PreferredLanguage { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime? LastLoginAt { get; set; }

    // Navigation properties
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

    // Computed properties
    public string FullName => $"{FirstName} {LastName}".Trim();
}

public class UserRole : BaseEntity
{

    [Required]
    public Guid UserId { get; set; }

    [Required]
    [MaxLength(100)]
    public string RoleName { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Source { get; set; } // "keycloak", "application", etc.

    // Navigation properties
    public virtual User User { get; set; } = null!;
}