using Family.Api.GraphQL.Mutations;
using FluentAssertions;

namespace Family.Api.Tests.GraphQL.Mutations;

public class UserMutationInputTests
{
    [Fact]
    public void CreateUserInput_ShouldCreateWithAllProperties()
    {
        var email = "test@example.com";
        var firstName = "John";
        var lastName = "Doe";
        var preferredLanguage = "en";
        var keycloakSubjectId = "keycloak-123";

        var input = new CreateUserInput(email, firstName, lastName, preferredLanguage, keycloakSubjectId);

        input.Email.Should().Be(email);
        input.FirstName.Should().Be(firstName);
        input.LastName.Should().Be(lastName);
        input.PreferredLanguage.Should().Be(preferredLanguage);
        input.KeycloakSubjectId.Should().Be(keycloakSubjectId);
    }

    [Fact]
    public void CreateUserInput_ShouldUseDefaultLanguage()
    {
        var input = new CreateUserInput("test@example.com", "John", "Doe");

        input.PreferredLanguage.Should().Be("de");
        input.KeycloakSubjectId.Should().BeNull();
    }

    [Fact]
    public void UpdateUserInput_ShouldCreateWithAllProperties()
    {
        var userId = Guid.NewGuid();
        var firstName = "John";
        var lastName = "Doe";
        var preferredLanguage = "en";
        var isActive = true;

        var input = new UpdateUserInput(userId, firstName, lastName, preferredLanguage, isActive);

        input.UserId.Should().Be(userId);
        input.FirstName.Should().Be(firstName);
        input.LastName.Should().Be(lastName);
        input.PreferredLanguage.Should().Be(preferredLanguage);
        input.IsActive.Should().Be(isActive);
    }

    [Fact]
    public void DeleteUserInput_ShouldCreateWithUserId()
    {
        var userId = Guid.NewGuid();

        var input = new DeleteUserInput(userId);

        input.UserId.Should().Be(userId);
    }
}