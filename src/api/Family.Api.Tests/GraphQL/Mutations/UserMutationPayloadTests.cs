using Family.Api.Features.Users.DTOs;
using Family.Api.GraphQL.Mutations;
using FluentAssertions;

namespace Family.Api.Tests.GraphQL.Mutations;

public class UserMutationPayloadTests
{
    [Fact]
    public void CreateUserPayload_ShouldCreateWithAllProperties()
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
            UpdatedAt = DateTime.UtcNow
        };
        var isSuccess = true;
        var errorMessage = "Error occurred";
        var validationErrors = new Dictionary<string, string[]>
        {
            { "Email", new[] { "Email is required" } }
        };

        var payload = new CreateUserPayload(userDto, isSuccess, errorMessage, validationErrors);

        payload.User.Should().Be(userDto);
        payload.IsSuccess.Should().Be(isSuccess);
        payload.ErrorMessage.Should().Be(errorMessage);
        payload.ValidationErrors.Should().BeEquivalentTo(validationErrors);
    }

    [Fact]
    public void CreateUserPayload_ShouldCreateWithNullValues()
    {
        var payload = new CreateUserPayload(null, false, null, null);

        payload.User.Should().BeNull();
        payload.IsSuccess.Should().BeFalse();
        payload.ErrorMessage.Should().BeNull();
        payload.ValidationErrors.Should().BeNull();
    }

    [Fact]
    public void UpdateUserPayload_ShouldCreateWithAllProperties()
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
            UpdatedAt = DateTime.UtcNow
        };
        var isSuccess = true;
        var errorMessage = "Error occurred";
        var validationErrors = new Dictionary<string, string[]>
        {
            { "FirstName", new[] { "FirstName is required" } }
        };

        var payload = new UpdateUserPayload(userDto, isSuccess, errorMessage, validationErrors);

        payload.User.Should().Be(userDto);
        payload.IsSuccess.Should().Be(isSuccess);
        payload.ErrorMessage.Should().Be(errorMessage);
        payload.ValidationErrors.Should().BeEquivalentTo(validationErrors);
    }

    [Fact]
    public void DeleteUserPayload_ShouldCreateWithAllProperties()
    {
        var isSuccess = true;
        var errorMessage = "User not found";
        var validationErrors = new Dictionary<string, string[]>
        {
            { "UserId", new[] { "UserId is required" } }
        };

        var payload = new DeleteUserPayload(isSuccess, errorMessage, validationErrors);

        payload.IsSuccess.Should().Be(isSuccess);
        payload.ErrorMessage.Should().Be(errorMessage);
        payload.ValidationErrors.Should().BeEquivalentTo(validationErrors);
    }

    [Fact]
    public void DeleteUserPayload_ShouldCreateWithNullValues()
    {
        var payload = new DeleteUserPayload(false, null, null);

        payload.IsSuccess.Should().BeFalse();
        payload.ErrorMessage.Should().BeNull();
        payload.ValidationErrors.Should().BeNull();
    }
}