using Family.Api.Features.Users.Commands;
using FluentAssertions;

namespace Family.Api.Tests.Features.Users.Commands;

public class UpdateUserCommandTests
{
    [Fact]
    public void UpdateUserCommand_ShouldCreateWithAllProperties()
    {
        var userId = Guid.NewGuid();
        var firstName = "John";
        var lastName = "Doe";
        var preferredLanguage = "en";
        var isActive = true;

        var command = new UpdateUserCommand
        {
            UserId = userId,
            FirstName = firstName,
            LastName = lastName,
            PreferredLanguage = preferredLanguage,
            IsActive = isActive
        };

        command.UserId.Should().Be(userId);
        command.FirstName.Should().Be(firstName);
        command.LastName.Should().Be(lastName);
        command.PreferredLanguage.Should().Be(preferredLanguage);
        command.IsActive.Should().Be(isActive);
    }

    [Fact]
    public void UpdateUserCommand_Validation_ShouldFailForEmptyUserId()
    {
        var command = new UpdateUserCommand
        {
            UserId = Guid.Empty,
            FirstName = "John",
            LastName = "Doe",
            PreferredLanguage = "de",
            IsActive = true
        };

        var validator = new UpdateUserCommandValidator();
        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateUserCommand.UserId));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void UpdateUserCommand_Validation_ShouldFailForInvalidFirstName(string invalidFirstName)
    {
        var command = new UpdateUserCommand
        {
            UserId = Guid.NewGuid(),
            FirstName = invalidFirstName,
            LastName = "Doe",
            PreferredLanguage = "de",
            IsActive = true
        };

        var validator = new UpdateUserCommandValidator();
        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateUserCommand.FirstName));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void UpdateUserCommand_Validation_ShouldFailForInvalidLastName(string invalidLastName)
    {
        var command = new UpdateUserCommand
        {
            UserId = Guid.NewGuid(),
            FirstName = "John",
            LastName = invalidLastName,
            PreferredLanguage = "de",
            IsActive = true
        };

        var validator = new UpdateUserCommandValidator();
        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateUserCommand.LastName));
    }

    [Fact]
    public void UpdateUserCommand_Validation_ShouldPassForValidCommand()
    {
        var command = new UpdateUserCommand
        {
            UserId = Guid.NewGuid(),
            FirstName = "John",
            LastName = "Doe",
            PreferredLanguage = "de",
            IsActive = true
        };

        var validator = new UpdateUserCommandValidator();
        var result = validator.Validate(command);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void UpdateUserCommand_ShouldAcceptDifferentActiveStates(bool isActive)
    {
        var command = new UpdateUserCommand
        {
            UserId = Guid.NewGuid(),
            FirstName = "John",
            LastName = "Doe",
            PreferredLanguage = "de",
            IsActive = isActive
        };

        command.IsActive.Should().Be(isActive);
    }
}