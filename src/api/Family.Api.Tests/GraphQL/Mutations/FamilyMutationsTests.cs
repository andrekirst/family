using AutoFixture;
using Family.Api.Authorization;
using Family.Api.Data;
using Family.Api.Data.Models;
using Family.Api.Features.Families.Commands;
using Family.Api.Features.Families.DTOs;
using Family.Api.GraphQL.Mutations;
using Family.Api.GraphQL.Types;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using NSubstitute;
using System.Security.Claims;

namespace Family.Api.Tests.GraphQL.Mutations;

public class FamilyMutationsTests
{
    private readonly Fixture _fixture = new();
    private readonly IMediator _mediator = Substitute.For<IMediator>();
    private readonly FamilyDbContext _context;
    private readonly IStringLocalizer<FamilyMutations> _localizer = Substitute.For<IStringLocalizer<FamilyMutations>>();
    private readonly FamilyMutations _mutations;

    public FamilyMutationsTests()
    {
        var options = new DbContextOptionsBuilder<FamilyDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new FamilyDbContext(options);

        _mutations = new FamilyMutations();

        // Setup default localizer returns
        _localizer["UserNotFound"].Returns(new LocalizedString("UserNotFound", "User not found"));
    }

    [Fact]
    public async Task CreateFamily_WithValidUserAndInput_ShouldReturnSuccessPayload()
    {
        // Arrange
        var user = CreateTestUser();
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var input = new CreateFamilyInput { Name = _fixture.Create<string>() };
        var claimsPrincipal = CreateClaimsPrincipal(user.KeycloakSubjectId);
        var familyDto = _fixture.Create<FamilyDto>();

        var commandResult = new CreateFamilyCommandResult
        {
            IsSuccess = true,
            Family = familyDto
        };

        _mediator.Send(Arg.Any<CreateFamilyCommand>(), Arg.Any<CancellationToken>())
            .Returns(commandResult);

        // Act
        var result = await _mutations.CreateFamily(
            input, 
            claimsPrincipal, 
            _mediator, 
            _context, 
            _localizer, 
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Family.Should().Be(familyDto);
        result.ErrorMessage.Should().BeNull();
        result.ValidationErrors.Should().BeNull();

        await _mediator.Received(1).Send(
            Arg.Is<CreateFamilyCommand>(cmd => 
                cmd.Name == input.Name && 
                cmd.OwnerId == user.Id),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateFamily_WithMissingSubjectClaim_ShouldReturnErrorPayload()
    {
        // Arrange
        var input = new CreateFamilyInput { Name = _fixture.Create<string>() };
        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity());

        // Act
        var result = await _mutations.CreateFamily(
            input, 
            claimsPrincipal, 
            _mediator, 
            _context, 
            _localizer, 
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Family.Should().BeNull();
        result.ErrorMessage.Should().Be("User not found");
        result.ValidationErrors.Should().BeNull();

        await _mediator.DidNotReceive().Send(Arg.Any<CreateFamilyCommand>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateFamily_WithNonExistentUser_ShouldReturnErrorPayload()
    {
        // Arrange
        var input = new CreateFamilyInput { Name = _fixture.Create<string>() };
        var claimsPrincipal = CreateClaimsPrincipal(_fixture.Create<string>());

        // Act
        var result = await _mutations.CreateFamily(
            input, 
            claimsPrincipal, 
            _mediator, 
            _context, 
            _localizer, 
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Family.Should().BeNull();
        result.ErrorMessage.Should().Be("User not found");
        result.ValidationErrors.Should().BeNull();

        await _mediator.DidNotReceive().Send(Arg.Any<CreateFamilyCommand>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateFamily_WithCommandFailure_ShouldReturnErrorPayload()
    {
        // Arrange
        var user = CreateTestUser();
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var input = new CreateFamilyInput { Name = _fixture.Create<string>() };
        var claimsPrincipal = CreateClaimsPrincipal(user.KeycloakSubjectId);
        var errorMessage = _fixture.Create<string>();

        var commandResult = new CreateFamilyCommandResult
        {
            IsSuccess = false,
            ErrorMessage = errorMessage
        };

        _mediator.Send(Arg.Any<CreateFamilyCommand>(), Arg.Any<CancellationToken>())
            .Returns(commandResult);

        // Act
        var result = await _mutations.CreateFamily(
            input, 
            claimsPrincipal, 
            _mediator, 
            _context, 
            _localizer, 
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Family.Should().BeNull();
        result.ErrorMessage.Should().Be(errorMessage);
        result.ValidationErrors.Should().BeNull();
    }

    [Fact]
    public async Task CreateFamily_WithValidationErrors_ShouldReturnValidationErrorsPayload()
    {
        // Arrange
        var user = CreateTestUser();
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var input = new CreateFamilyInput { Name = _fixture.Create<string>() };
        var claimsPrincipal = CreateClaimsPrincipal(user.KeycloakSubjectId);

        var validationErrors = new Dictionary<string, List<string>>
        {
            { "Name", new List<string> { "Name is required", "Name too short" } },
            { "OwnerId", new List<string> { "Owner ID is invalid" } }
        };

        var commandResult = new CreateFamilyCommandResult
        {
            IsSuccess = false,
            ValidationErrors = validationErrors
        };

        _mediator.Send(Arg.Any<CreateFamilyCommand>(), Arg.Any<CancellationToken>())
            .Returns(commandResult);

        // Act
        var result = await _mutations.CreateFamily(
            input, 
            claimsPrincipal, 
            _mediator, 
            _context, 
            _localizer, 
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Family.Should().BeNull();
        result.ValidationErrors.Should().NotBeNull();
        result.ValidationErrors.Should().HaveCount(3);
        result.ValidationErrors.Should().Contain("Name is required");
        result.ValidationErrors.Should().Contain("Name too short");
        result.ValidationErrors.Should().Contain("Owner ID is invalid");
    }

    [Fact]
    public async Task CreateFamily_WithException_ShouldReturnErrorPayload()
    {
        // Arrange
        var user = CreateTestUser();
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var input = new CreateFamilyInput { Name = _fixture.Create<string>() };
        var claimsPrincipal = CreateClaimsPrincipal(user.KeycloakSubjectId);
        var exceptionMessage = _fixture.Create<string>();

        _mediator.Send(Arg.Any<CreateFamilyCommand>(), Arg.Any<CancellationToken>())
            .Throws(new Exception(exceptionMessage));

        // Act
        var result = await _mutations.CreateFamily(
            input, 
            claimsPrincipal, 
            _mediator, 
            _context, 
            _localizer, 
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Family.Should().BeNull();
        result.ErrorMessage.Should().Be(exceptionMessage);
        result.ValidationErrors.Should().BeNull();
    }

    [Fact]
    public void CreateFamily_ShouldRequireFamilyUserPolicy()
    {
        // Arrange
        var method = typeof(FamilyMutations).GetMethod(nameof(FamilyMutations.CreateFamily));

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