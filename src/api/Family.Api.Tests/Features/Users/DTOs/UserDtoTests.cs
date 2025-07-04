using Family.Api.Features.Users.DTOs;
using FluentAssertions;

namespace Family.Api.Tests.Features.Users.DTOs;

public class UserDtoTests
{
    [Fact]
    public void UserDto_ShouldCreateWithAllProperties()
    {
        var userId = Guid.NewGuid();
        var email = "test@example.com";
        var firstName = "John";
        var lastName = "Doe";
        var preferredLanguage = "en";
        var isActive = true;
        var createdAt = DateTime.UtcNow;
        var updatedAt = DateTime.UtcNow.AddMinutes(1);
        var keycloakSubjectId = "keycloak-123";

        var userDto = new UserDto
        {
            Id = userId,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            PreferredLanguage = preferredLanguage,
            IsActive = isActive,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt,
            KeycloakSubjectId = keycloakSubjectId
        };

        userDto.Id.Should().Be(userId);
        userDto.Email.Should().Be(email);
        userDto.FirstName.Should().Be(firstName);
        userDto.LastName.Should().Be(lastName);
        userDto.PreferredLanguage.Should().Be(preferredLanguage);
        userDto.IsActive.Should().Be(isActive);
        userDto.CreatedAt.Should().Be(createdAt);
        userDto.UpdatedAt.Should().Be(updatedAt);
        userDto.KeycloakSubjectId.Should().Be(keycloakSubjectId);
    }

    [Fact]
    public void UserDto_ShouldAllowNullKeycloakSubjectId()
    {
        var userDto = new UserDto
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            FirstName = "John",
            LastName = "Doe",
            PreferredLanguage = "de",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            KeycloakSubjectId = null
        };

        userDto.KeycloakSubjectId.Should().BeNull();
    }

    [Theory]
    [InlineData("de")]
    [InlineData("en")]
    [InlineData("fr")]
    public void UserDto_ShouldAcceptDifferentLanguages(string language)
    {
        var userDto = new UserDto
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            FirstName = "John",
            LastName = "Doe",
            PreferredLanguage = language,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        userDto.PreferredLanguage.Should().Be(language);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void UserDto_ShouldAcceptDifferentActiveStates(bool isActive)
    {
        var userDto = new UserDto
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            FirstName = "John",
            LastName = "Doe",
            PreferredLanguage = "de",
            IsActive = isActive,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        userDto.IsActive.Should().Be(isActive);
    }
}