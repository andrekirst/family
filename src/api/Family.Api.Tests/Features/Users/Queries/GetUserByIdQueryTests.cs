using Family.Api.Features.Users.Queries;
using FluentAssertions;

namespace Family.Api.Tests.Features.Users.Queries;

public class GetUserByIdQueryTests
{
    [Fact]
    public void GetUserByIdQuery_ShouldCreateWithUserId()
    {
        var userId = Guid.NewGuid();

        var query = new GetUserByIdQuery
        {
            UserId = userId
        };

        query.UserId.Should().Be(userId);
    }

    [Fact]
    public void GetUserByIdQuery_Validation_ShouldFailForEmptyUserId()
    {
        var query = new GetUserByIdQuery
        {
            UserId = Guid.Empty
        };

        var validator = new GetUserByIdQueryValidator();
        var result = validator.Validate(query);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(GetUserByIdQuery.UserId));
    }

    [Fact]
    public void GetUserByIdQuery_Validation_ShouldPassForValidUserId()
    {
        var query = new GetUserByIdQuery
        {
            UserId = Guid.NewGuid()
        };

        var validator = new GetUserByIdQueryValidator();
        var result = validator.Validate(query);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }
}