using AutoFixture;
using Family.Api.Authorization;
using Family.Api.Data;
using Family.Api.Data.Models;
using Family.Api.Features.Families.DTOs;
using Family.Api.Features.Families.Queries;
using Family.Api.GraphQL.Queries;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using System.Security.Claims;

namespace Family.Api.Tests.GraphQL.Queries;

public class FamilyQueriesTests
{
    private readonly Fixture _fixture = new();
    private readonly IMediator _mediator = Substitute.For<IMediator>();
    private readonly FamilyDbContext _context;
    private readonly FamilyQueries _queries;

    public FamilyQueriesTests()
    {
        var options = new DbContextOptionsBuilder<FamilyDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new FamilyDbContext(options);

        _queries = new FamilyQueries();
    }

    [Fact]
    public async Task GetMyFamily_WithValidUserHavingFamily_ShouldReturnFamily()
    {
        // Arrange
        var user = CreateTestUser();
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var claimsPrincipal = CreateClaimsPrincipal(user.KeycloakSubjectId);
        var familyDto = _fixture.Create<FamilyDto>();

        _mediator.Send(Arg.Any<GetUserFamilyQuery>(), Arg.Any<CancellationToken>())
            .Returns(familyDto);

        // Act
        var result = await _queries.GetMyFamily(claimsPrincipal, _mediator, _context, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().Be(familyDto);

        await _mediator.Received(1).Send(
            Arg.Is<GetUserFamilyQuery>(q => q.UserId == user.Id),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetMyFamily_WithMissingSubjectClaim_ShouldReturnNull()
    {
        // Arrange
        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity());

        // Act
        var result = await _queries.GetMyFamily(claimsPrincipal, _mediator, _context, CancellationToken.None);

        // Assert
        result.Should().BeNull();

        await _mediator.DidNotReceive().Send(Arg.Any<GetUserFamilyQuery>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetMyFamily_WithNonExistentUser_ShouldReturnNull()
    {
        // Arrange
        var claimsPrincipal = CreateClaimsPrincipal(_fixture.Create<string>());

        // Act
        var result = await _queries.GetMyFamily(claimsPrincipal, _mediator, _context, CancellationToken.None);

        // Assert
        result.Should().BeNull();

        await _mediator.DidNotReceive().Send(Arg.Any<GetUserFamilyQuery>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetMyFamily_WithUserWithoutFamily_ShouldReturnNull()
    {
        // Arrange
        var user = CreateTestUser();
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var claimsPrincipal = CreateClaimsPrincipal(user.KeycloakSubjectId);

        _mediator.Send(Arg.Any<GetUserFamilyQuery>(), Arg.Any<CancellationToken>())
            .Returns((FamilyDto?)null);

        // Act
        var result = await _queries.GetMyFamily(claimsPrincipal, _mediator, _context, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetMyFamily_WithException_ShouldReturnNull()
    {
        // Arrange
        var user = CreateTestUser();
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var claimsPrincipal = CreateClaimsPrincipal(user.KeycloakSubjectId);

        _mediator.Send(Arg.Any<GetUserFamilyQuery>(), Arg.Any<CancellationToken>())
            .Throws(new Exception("Database error"));

        // Act
        var result = await _queries.GetMyFamily(claimsPrincipal, _mediator, _context, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetFamilyById_WithValidFamilyId_ShouldReturnFamily()
    {
        // Arrange
        var familyId = _fixture.Create<Guid>().ToString();
        var familyDto = _fixture.Create<FamilyDto>();

        _mediator.Send(Arg.Any<GetFamilyByIdQuery>(), Arg.Any<CancellationToken>())
            .Returns(familyDto);

        // Act
        var result = await _queries.GetFamilyById(familyId, _mediator, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().Be(familyDto);

        await _mediator.Received(1).Send(
            Arg.Is<GetFamilyByIdQuery>(q => q.FamilyId == familyId),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetFamilyById_WithNonExistentFamilyId_ShouldReturnNull()
    {
        // Arrange
        var familyId = _fixture.Create<Guid>().ToString();

        _mediator.Send(Arg.Any<GetFamilyByIdQuery>(), Arg.Any<CancellationToken>())
            .Returns((FamilyDto?)null);

        // Act
        var result = await _queries.GetFamilyById(familyId, _mediator, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetFamilyById_WithException_ShouldReturnNull()
    {
        // Arrange
        var familyId = _fixture.Create<Guid>().ToString();

        _mediator.Send(Arg.Any<GetFamilyByIdQuery>(), Arg.Any<CancellationToken>())
            .Throws(new Exception("Database error"));

        // Act
        var result = await _queries.GetFamilyById(familyId, _mediator, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task HasFamily_WithUserHavingFamily_ShouldReturnTrue()
    {
        // Arrange
        var user = CreateTestUser();
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var claimsPrincipal = CreateClaimsPrincipal(user.KeycloakSubjectId);
        var familyDto = _fixture.Create<FamilyDto>();

        _mediator.Send(Arg.Any<GetUserFamilyQuery>(), Arg.Any<CancellationToken>())
            .Returns(familyDto);

        // Act
        var result = await _queries.HasFamily(claimsPrincipal, _mediator, _context, CancellationToken.None);

        // Assert
        result.Should().BeTrue();

        await _mediator.Received(1).Send(
            Arg.Is<GetUserFamilyQuery>(q => q.UserId == user.Id),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HasFamily_WithUserWithoutFamily_ShouldReturnFalse()
    {
        // Arrange
        var user = CreateTestUser();
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var claimsPrincipal = CreateClaimsPrincipal(user.KeycloakSubjectId);

        _mediator.Send(Arg.Any<GetUserFamilyQuery>(), Arg.Any<CancellationToken>())
            .Returns((FamilyDto?)null);

        // Act
        var result = await _queries.HasFamily(claimsPrincipal, _mediator, _context, CancellationToken.None);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task HasFamily_WithMissingSubjectClaim_ShouldReturnFalse()
    {
        // Arrange
        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity());

        // Act
        var result = await _queries.HasFamily(claimsPrincipal, _mediator, _context, CancellationToken.None);

        // Assert
        result.Should().BeFalse();

        await _mediator.DidNotReceive().Send(Arg.Any<GetUserFamilyQuery>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HasFamily_WithNonExistentUser_ShouldReturnFalse()
    {
        // Arrange
        var claimsPrincipal = CreateClaimsPrincipal(_fixture.Create<string>());

        // Act
        var result = await _queries.HasFamily(claimsPrincipal, _mediator, _context, CancellationToken.None);

        // Assert
        result.Should().BeFalse();

        await _mediator.DidNotReceive().Send(Arg.Any<GetUserFamilyQuery>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HasFamily_WithException_ShouldReturnFalse()
    {
        // Arrange
        var user = CreateTestUser();
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var claimsPrincipal = CreateClaimsPrincipal(user.KeycloakSubjectId);

        _mediator.Send(Arg.Any<GetUserFamilyQuery>(), Arg.Any<CancellationToken>())
            .Throws(new Exception("Database error"));

        // Act
        var result = await _queries.HasFamily(claimsPrincipal, _mediator, _context, CancellationToken.None);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void GetMyFamily_ShouldRequireFamilyUserPolicy()
    {
        // Arrange
        var method = typeof(FamilyQueries).GetMethod(nameof(FamilyQueries.GetMyFamily));

        // Act
        var authorizeAttribute = method?.GetCustomAttributes(typeof(HotChocolate.Authorization.AuthorizeAttribute), false)
            .Cast<HotChocolate.Authorization.AuthorizeAttribute>()
            .FirstOrDefault();

        // Assert
        authorizeAttribute.Should().NotBeNull();
        authorizeAttribute!.Policy.Should().Be(Policies.FamilyUser);
    }

    [Fact]
    public void GetFamilyById_ShouldRequireFamilyAdminPolicy()
    {
        // Arrange
        var method = typeof(FamilyQueries).GetMethod(nameof(FamilyQueries.GetFamilyById));

        // Act
        var authorizeAttribute = method?.GetCustomAttributes(typeof(HotChocolate.Authorization.AuthorizeAttribute), false)
            .Cast<HotChocolate.Authorization.AuthorizeAttribute>()
            .FirstOrDefault();

        // Assert
        authorizeAttribute.Should().NotBeNull();
        authorizeAttribute!.Policy.Should().Be(Policies.FamilyAdmin);
    }

    [Fact]
    public void HasFamily_ShouldRequireFamilyUserPolicy()
    {
        // Arrange
        var method = typeof(FamilyQueries).GetMethod(nameof(FamilyQueries.HasFamily));

        // Act
        var authorizeAttribute = method?.GetCustomAttributes(typeof(HotChocolate.Authorization.AuthorizeAttribute), false)
            .Cast<HotChocolate.Authorization.AuthorizeAttribute>()
            .FirstOrDefault();

        // Assert
        authorizeAttribute.Should().NotBeNull();
        authorizeAttribute!.Policy.Should().Be(Policies.FamilyUser);
    }

    private User CreateTestUser()
    {
        return new User
        {
            Id = _fixture.Create<Guid>(),
            KeycloakSubjectId = _fixture.Create<string>(),
            Email = _fixture.Create<string>(),
            FirstName = _fixture.Create<string>(),
            LastName = _fixture.Create<string>(),
            CreatedAt = DateTime.UtcNow
        };
    }

    private ClaimsPrincipal CreateClaimsPrincipal(string subjectId)
    {
        var claims = new List<Claim>
        {
            new("sub", subjectId)
        };

        return new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _context?.Dispose();
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}