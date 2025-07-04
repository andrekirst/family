using Family.Api.Features.Users.Queries;
using FluentAssertions;

namespace Family.Api.Tests.Features.Users.Queries;

public class GetUserByEmailQueryTests
{
    [Fact]
    public void GetUserByEmailQuery_ShouldCreateWithEmail()
    {
        var email = "test@example.com";

        var query = new GetUserByEmailQuery
        {
            Email = email
        };

        query.Email.Should().Be(email);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void GetUserByEmailQuery_Validation_ShouldFailForInvalidEmail(string invalidEmail)
    {
        var query = new GetUserByEmailQuery
        {
            Email = invalidEmail
        };

        var validator = new GetUserByEmailQueryValidator();
        var result = validator.Validate(query);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(GetUserByEmailQuery.Email));
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("test@")]
    [InlineData("@example.com")]
    public void GetUserByEmailQuery_Validation_ShouldFailForMalformedEmail(string malformedEmail)
    {
        var query = new GetUserByEmailQuery
        {
            Email = malformedEmail
        };

        var validator = new GetUserByEmailQueryValidator();
        var result = validator.Validate(query);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(GetUserByEmailQuery.Email));
    }

    [Fact]
    public void GetUserByEmailQuery_Validation_ShouldPassForValidEmail()
    {
        var query = new GetUserByEmailQuery
        {
            Email = "test@example.com"
        };

        var validator = new GetUserByEmailQueryValidator();
        var result = validator.Validate(query);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }
}