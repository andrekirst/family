using Family.Api.Features.Users.Queries;
using Family.Api.Features.Users;
using FluentAssertions;
using Microsoft.Extensions.Localization;
using NSubstitute;

namespace Family.Api.Tests.Features.Users.Queries;

public class GetAllUsersQueryTests
{
    [Fact]
    public void GetAllUsersQuery_ShouldCreateWithDefaultValues()
    {
        var query = new GetAllUsersQuery();

        query.PageNumber.Should().Be(1);
        query.PageSize.Should().Be(50);
        query.IncludeInactive.Should().BeFalse();
    }

    [Fact]
    public void GetAllUsersQuery_ShouldCreateWithCustomValues()
    {
        var pageNumber = 2;
        var pageSize = 25;
        var includeInactive = true;

        var query = new GetAllUsersQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            IncludeInactive = includeInactive
        };

        query.PageNumber.Should().Be(pageNumber);
        query.PageSize.Should().Be(pageSize);
        query.IncludeInactive.Should().Be(includeInactive);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void GetAllUsersQuery_Validation_ShouldFailForInvalidPageNumber(int invalidPageNumber)
    {
        var query = new GetAllUsersQuery
        {
            PageNumber = invalidPageNumber,
            PageSize = 10
        };

        var localizer = Substitute.For<IStringLocalizer<UserValidationMessages>>();
        localizer["PageNumberInvalid"].Returns(new LocalizedString("PageNumberInvalid", "Page number must be greater than 0"));
        localizer["PageSizeInvalid"].Returns(new LocalizedString("PageSizeInvalid", "Page size must be between 1 and 100"));
        
        var validator = new GetAllUsersQueryValidator(localizer);
        var result = validator.Validate(query);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(GetAllUsersQuery.PageNumber));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(101)]
    public void GetAllUsersQuery_Validation_ShouldFailForInvalidPageSize(int invalidPageSize)
    {
        var query = new GetAllUsersQuery
        {
            PageNumber = 1,
            PageSize = invalidPageSize
        };

        var localizer = Substitute.For<IStringLocalizer<UserValidationMessages>>();
        localizer["PageNumberInvalid"].Returns(new LocalizedString("PageNumberInvalid", "Page number must be greater than 0"));
        localizer["PageSizeInvalid"].Returns(new LocalizedString("PageSizeInvalid", "Page size must be between 1 and 100"));
        
        var validator = new GetAllUsersQueryValidator(localizer);
        var result = validator.Validate(query);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(GetAllUsersQuery.PageSize));
    }

    [Fact]
    public void GetAllUsersQuery_Validation_ShouldPassForValidQuery()
    {
        var query = new GetAllUsersQuery
        {
            PageNumber = 1,
            PageSize = 25,
            IncludeInactive = true
        };

        var localizer = Substitute.For<IStringLocalizer<UserValidationMessages>>();
        localizer["PageNumberInvalid"].Returns(new LocalizedString("PageNumberInvalid", "Page number must be greater than 0"));
        localizer["PageSizeInvalid"].Returns(new LocalizedString("PageSizeInvalid", "Page size must be between 1 and 100"));
        
        var validator = new GetAllUsersQueryValidator(localizer);
        var result = validator.Validate(query);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void GetAllUsersQuery_ShouldAcceptDifferentIncludeInactiveValues(bool includeInactive)
    {
        var query = new GetAllUsersQuery
        {
            PageNumber = 1,
            PageSize = 10,
            IncludeInactive = includeInactive
        };

        query.IncludeInactive.Should().Be(includeInactive);
    }
}