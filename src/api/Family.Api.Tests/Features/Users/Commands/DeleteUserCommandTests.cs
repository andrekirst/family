using Family.Api.Features.Users.Commands;
using Family.Api.Features.Users;
using FluentAssertions;
using Microsoft.Extensions.Localization;
using NSubstitute;

namespace Family.Api.Tests.Features.Users.Commands;

public class DeleteUserCommandTests
{
    [Fact]
    public void DeleteUserCommand_ShouldCreateWithUserId()
    {
        var userId = Guid.NewGuid();

        var command = new DeleteUserCommand
        {
            UserId = userId
        };

        command.UserId.Should().Be(userId);
    }

    [Fact]
    public void DeleteUserCommand_Validation_ShouldFailForEmptyUserId()
    {
        var command = new DeleteUserCommand
        {
            UserId = Guid.Empty
        };

        var localizer = Substitute.For<IStringLocalizer<UserValidationMessages>>();
        localizer["UserIdRequired"].Returns("User ID is required");
        
        var validator = new DeleteUserCommandValidator(localizer);
        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(DeleteUserCommand.UserId));
    }

    [Fact]
    public void DeleteUserCommand_Validation_ShouldPassForValidUserId()
    {
        var command = new DeleteUserCommand
        {
            UserId = Guid.NewGuid()
        };

        var localizer = Substitute.For<IStringLocalizer<UserValidationMessages>>();
        localizer["UserIdRequired"].Returns("User ID is required");
        
        var validator = new DeleteUserCommandValidator(localizer);
        var result = validator.Validate(command);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }
}