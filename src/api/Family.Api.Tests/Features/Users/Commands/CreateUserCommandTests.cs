using Family.Api.Features.Users.Commands;
using Family.Api.Features.Users;
using FluentAssertions;
using Microsoft.Extensions.Localization;
using NSubstitute;

namespace Family.Api.Tests.Features.Users.Commands;

public class CreateUserCommandTests
{
    [Fact]
    public void CreateUserCommand_ShouldCreateWithAllProperties()
    {
        var email = "test@example.com";
        var firstName = "John";
        var lastName = "Doe";
        var preferredLanguage = "en";
        var keycloakSubjectId = "keycloak-123";

        var command = new CreateUserCommand
        {
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            PreferredLanguage = preferredLanguage,
            KeycloakSubjectId = keycloakSubjectId
        };

        command.Email.Should().Be(email);
        command.FirstName.Should().Be(firstName);
        command.LastName.Should().Be(lastName);
        command.PreferredLanguage.Should().Be(preferredLanguage);
        command.KeycloakSubjectId.Should().Be(keycloakSubjectId);
    }

    [Fact]
    public void CreateUserCommand_ShouldAllowNullKeycloakSubjectId()
    {
        var command = new CreateUserCommand
        {
            Email = "test@example.com",
            FirstName = "John",
            LastName = "Doe",
            PreferredLanguage = "de",
            KeycloakSubjectId = null
        };

        command.KeycloakSubjectId.Should().BeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void CreateUserCommand_Validation_ShouldFailForInvalidEmail(string invalidEmail)
    {
        var command = new CreateUserCommand
        {
            Email = invalidEmail,
            FirstName = "John",
            LastName = "Doe",
            PreferredLanguage = "de"
        };

        var localizer = Substitute.For<IStringLocalizer<UserValidationMessages>>();
        localizer["EmailRequired"].Returns(new LocalizedString("EmailRequired", "Email is required"));
        localizer["EmailInvalid"].Returns(new LocalizedString("EmailInvalid", "Invalid email format"));
        localizer["EmailMaxLength"].Returns(new LocalizedString("EmailMaxLength", "Email max length exceeded"));
        localizer["FirstNameRequired"].Returns(new LocalizedString("FirstNameRequired", "First name is required"));
        localizer["FirstNameMaxLength"].Returns(new LocalizedString("FirstNameMaxLength", "First name max length exceeded"));
        localizer["LastNameRequired"].Returns(new LocalizedString("LastNameRequired", "Last name is required"));
        localizer["LastNameMaxLength"].Returns(new LocalizedString("LastNameMaxLength", "Last name max length exceeded"));
        localizer["PreferredLanguageRequired"].Returns(new LocalizedString("PreferredLanguageRequired", "Preferred language is required"));
        localizer["PreferredLanguageInvalid"].Returns(new LocalizedString("PreferredLanguageInvalid", "Preferred language is invalid"));
        
        var validator = new CreateUserCommandValidator(localizer);
        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateUserCommand.Email));
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("test@")]
    [InlineData("@example.com")]
    public void CreateUserCommand_Validation_ShouldFailForMalformedEmail(string malformedEmail)
    {
        var command = new CreateUserCommand
        {
            Email = malformedEmail,
            FirstName = "John",
            LastName = "Doe",
            PreferredLanguage = "de"
        };

        var localizer = Substitute.For<IStringLocalizer<UserValidationMessages>>();
        localizer["EmailRequired"].Returns(new LocalizedString("EmailRequired", "Email is required"));
        localizer["EmailInvalid"].Returns(new LocalizedString("EmailInvalid", "Invalid email format"));
        localizer["EmailMaxLength"].Returns(new LocalizedString("EmailMaxLength", "Email max length exceeded"));
        localizer["FirstNameRequired"].Returns(new LocalizedString("FirstNameRequired", "First name is required"));
        localizer["FirstNameMaxLength"].Returns(new LocalizedString("FirstNameMaxLength", "First name max length exceeded"));
        localizer["LastNameRequired"].Returns(new LocalizedString("LastNameRequired", "Last name is required"));
        localizer["LastNameMaxLength"].Returns(new LocalizedString("LastNameMaxLength", "Last name max length exceeded"));
        localizer["PreferredLanguageRequired"].Returns(new LocalizedString("PreferredLanguageRequired", "Preferred language is required"));
        localizer["PreferredLanguageInvalid"].Returns(new LocalizedString("PreferredLanguageInvalid", "Preferred language is invalid"));
        
        var validator = new CreateUserCommandValidator(localizer);
        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateUserCommand.Email));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void CreateUserCommand_Validation_ShouldFailForInvalidFirstName(string invalidFirstName)
    {
        var command = new CreateUserCommand
        {
            Email = "test@example.com",
            FirstName = invalidFirstName,
            LastName = "Doe",
            PreferredLanguage = "de"
        };

        var localizer = Substitute.For<IStringLocalizer<UserValidationMessages>>();
        localizer["EmailRequired"].Returns(new LocalizedString("EmailRequired", "Email is required"));
        localizer["EmailInvalid"].Returns(new LocalizedString("EmailInvalid", "Invalid email format"));
        localizer["EmailMaxLength"].Returns(new LocalizedString("EmailMaxLength", "Email max length exceeded"));
        localizer["FirstNameRequired"].Returns(new LocalizedString("FirstNameRequired", "First name is required"));
        localizer["FirstNameMaxLength"].Returns(new LocalizedString("FirstNameMaxLength", "First name max length exceeded"));
        localizer["LastNameRequired"].Returns(new LocalizedString("LastNameRequired", "Last name is required"));
        localizer["LastNameMaxLength"].Returns(new LocalizedString("LastNameMaxLength", "Last name max length exceeded"));
        localizer["PreferredLanguageRequired"].Returns(new LocalizedString("PreferredLanguageRequired", "Preferred language is required"));
        localizer["PreferredLanguageInvalid"].Returns(new LocalizedString("PreferredLanguageInvalid", "Preferred language is invalid"));
        
        var validator = new CreateUserCommandValidator(localizer);
        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateUserCommand.FirstName));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void CreateUserCommand_Validation_ShouldFailForInvalidLastName(string invalidLastName)
    {
        var command = new CreateUserCommand
        {
            Email = "test@example.com",
            FirstName = "John",
            LastName = invalidLastName,
            PreferredLanguage = "de"
        };

        var localizer = Substitute.For<IStringLocalizer<UserValidationMessages>>();
        localizer["EmailRequired"].Returns(new LocalizedString("EmailRequired", "Email is required"));
        localizer["EmailInvalid"].Returns(new LocalizedString("EmailInvalid", "Invalid email format"));
        localizer["EmailMaxLength"].Returns(new LocalizedString("EmailMaxLength", "Email max length exceeded"));
        localizer["FirstNameRequired"].Returns(new LocalizedString("FirstNameRequired", "First name is required"));
        localizer["FirstNameMaxLength"].Returns(new LocalizedString("FirstNameMaxLength", "First name max length exceeded"));
        localizer["LastNameRequired"].Returns(new LocalizedString("LastNameRequired", "Last name is required"));
        localizer["LastNameMaxLength"].Returns(new LocalizedString("LastNameMaxLength", "Last name max length exceeded"));
        localizer["PreferredLanguageRequired"].Returns(new LocalizedString("PreferredLanguageRequired", "Preferred language is required"));
        localizer["PreferredLanguageInvalid"].Returns(new LocalizedString("PreferredLanguageInvalid", "Preferred language is invalid"));
        
        var validator = new CreateUserCommandValidator(localizer);
        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateUserCommand.LastName));
    }

    [Fact]
    public void CreateUserCommand_Validation_ShouldPassForValidCommand()
    {
        var command = new CreateUserCommand
        {
            Email = "test@example.com",
            FirstName = "John",
            LastName = "Doe",
            PreferredLanguage = "de"
        };

        var localizer = Substitute.For<IStringLocalizer<UserValidationMessages>>();
        localizer["EmailRequired"].Returns(new LocalizedString("EmailRequired", "Email is required"));
        localizer["EmailInvalid"].Returns(new LocalizedString("EmailInvalid", "Invalid email format"));
        localizer["EmailMaxLength"].Returns(new LocalizedString("EmailMaxLength", "Email max length exceeded"));
        localizer["FirstNameRequired"].Returns(new LocalizedString("FirstNameRequired", "First name is required"));
        localizer["FirstNameMaxLength"].Returns(new LocalizedString("FirstNameMaxLength", "First name max length exceeded"));
        localizer["LastNameRequired"].Returns(new LocalizedString("LastNameRequired", "Last name is required"));
        localizer["LastNameMaxLength"].Returns(new LocalizedString("LastNameMaxLength", "Last name max length exceeded"));
        localizer["PreferredLanguageRequired"].Returns(new LocalizedString("PreferredLanguageRequired", "Preferred language is required"));
        localizer["PreferredLanguageInvalid"].Returns(new LocalizedString("PreferredLanguageInvalid", "Preferred language is invalid"));
        
        var validator = new CreateUserCommandValidator(localizer);
        var result = validator.Validate(command);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }
}