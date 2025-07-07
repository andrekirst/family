using AutoFixture;
using Family.Api.Features.Families.IFamilyRepository;
using Family.Api.Features.Families.Queries;
using FluentAssertions;
using NSubstitute;

namespace Family.Api.Tests.Features.Families.Queries;

public class GetUserFamilyQueryTests
{
    private readonly Fixture _fixture = new();
    private readonly IFamilyRepository _familyRepository = Substitute.For<IFamilyRepository>();
    private readonly GetUserFamilyQueryHandler _handler;

    public GetUserFamilyQueryTests()
    {
        _handler = new GetUserFamilyQueryHandler(_familyRepository);
    }

    [Fact]
    public async Task Handle_WithUserHavingFamilyAsOwner_ShouldReturnFamily()
    {
        // Arrange
        var userId = _fixture.Create<Guid>();
        var query = new GetUserFamilyQuery(userId);
        
        var family = Api.Features.Families.Models.Family.Create(_fixture.Create<string>(), userId);
        _familyRepository.GetByOwnerIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(family);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().Be(family);
        await _familyRepository.Received(1).GetByOwnerIdAsync(userId, Arg.Any<CancellationToken>());
        await _familyRepository.DidNotReceive().GetByMemberIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithUserHavingFamilyAsMember_ShouldReturnFamily()
    {
        // Arrange
        var userId = _fixture.Create<Guid>();
        var ownerId = _fixture.Create<Guid>();
        var query = new GetUserFamilyQuery(userId);
        
        var family = Api.Features.Families.Models.Family.Create(_fixture.Create<string>(), ownerId);
        family.AddMember(userId, Api.Features.Families.Models.FamilyRole.FamilyUser);

        _familyRepository.GetByOwnerIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns((Api.Features.Families.Models.Family?)null);
        
        _familyRepository.GetByMemberIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(family);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().Be(family);
        await _familyRepository.Received(1).GetByOwnerIdAsync(userId, Arg.Any<CancellationToken>());
        await _familyRepository.Received(1).GetByMemberIdAsync(userId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithUserNotHavingFamily_ShouldReturnNull()
    {
        // Arrange
        var userId = _fixture.Create<Guid>();
        var query = new GetUserFamilyQuery(userId);
        
        _familyRepository.GetByOwnerIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns((Api.Features.Families.Models.Family?)null);
        
        _familyRepository.GetByMemberIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns((Api.Features.Families.Models.Family?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
        await _familyRepository.Received(1).GetByOwnerIdAsync(userId, Arg.Any<CancellationToken>());
        await _familyRepository.Received(1).GetByMemberIdAsync(userId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithEmptyUserId_ShouldThrowArgumentException()
    {
        // Arrange
        var query = new GetUserFamilyQuery(Guid.Empty);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(async () => 
            await _handler.Handle(query, CancellationToken.None));
    }

    [Fact]
    public void GetUserFamilyQuery_WithValidUserId_ShouldCreateQuery()
    {
        // Arrange
        var userId = _fixture.Create<Guid>();

        // Act
        var query = new GetUserFamilyQuery(userId);

        // Assert
        query.UserId.Should().Be(userId);
    }

    [Fact]
    public async Task Handle_WithRepositoryExceptionOnOwnerCheck_ShouldPropagateException()
    {
        // Arrange
        var userId = _fixture.Create<Guid>();
        var query = new GetUserFamilyQuery(userId);
        
        _familyRepository.GetByOwnerIdAsync(userId, Arg.Any<CancellationToken>())
            .Throws(new Exception("Database connection failed"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(async () => 
            await _handler.Handle(query, CancellationToken.None));
        
        exception.Message.Should().Be("Database connection failed");
    }

    [Fact]
    public async Task Handle_WithRepositoryExceptionOnMemberCheck_ShouldPropagateException()
    {
        // Arrange
        var userId = _fixture.Create<Guid>();
        var query = new GetUserFamilyQuery(userId);
        
        _familyRepository.GetByOwnerIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns((Api.Features.Families.Models.Family?)null);
        
        _familyRepository.GetByMemberIdAsync(userId, Arg.Any<CancellationToken>())
            .Throws(new Exception("Database connection failed"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(async () => 
            await _handler.Handle(query, CancellationToken.None));
        
        exception.Message.Should().Be("Database connection failed");
    }

    [Fact]
    public async Task Handle_ShouldPrioritizeOwnershipOverMembership()
    {
        // Arrange
        var userId = _fixture.Create<Guid>();
        var query = new GetUserFamilyQuery(userId);
        
        var ownedFamily = Api.Features.Families.Models.Family.Create("Owned Family", userId);
        var memberFamily = Api.Features.Families.Models.Family.Create("Member Family", _fixture.Create<Guid>());
        
        _familyRepository.GetByOwnerIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(ownedFamily);
        
        _familyRepository.GetByMemberIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(memberFamily);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().Be(ownedFamily);
        result.Should().NotBe(memberFamily);
        await _familyRepository.Received(1).GetByOwnerIdAsync(userId, Arg.Any<CancellationToken>());
        await _familyRepository.DidNotReceive().GetByMemberIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }
}