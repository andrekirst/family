using AutoFixture;
using Family.Api.Features.Families.DomainEvents;
using Family.Api.Features.Families.Models;
using FluentAssertions;

namespace Family.Api.Tests.Features.Families.DomainEvents;

public class FamilyMemberAddedTests
{
    private readonly Fixture _fixture = new();

    [Fact]
    public void Create_WithValidParameters_ShouldCreateEventWithCorrectProperties()
    {
        // Arrange
        var memberUserId = _fixture.Create<Guid>();
        var role = FamilyRole.FamilyUser;
        var joinedAt = DateTime.UtcNow;

        // Act
        var domainEvent = FamilyMemberAdded.Create(memberUserId, role, joinedAt);

        // Assert
        domainEvent.Should().NotBeNull();
        domainEvent.MemberUserId.Should().Be(memberUserId);
        domainEvent.Role.Should().Be(role);
        domainEvent.JoinedAt.Should().Be(joinedAt);
        domainEvent.OccurredAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        domainEvent.EventId.Should().NotBeEmpty();
    }

    [Fact]
    public void Create_WithEmptyMemberUserId_ShouldThrowArgumentException()
    {
        // Arrange
        var role = FamilyRole.FamilyUser;
        var joinedAt = DateTime.UtcNow;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => FamilyMemberAdded.Create(Guid.Empty, role, joinedAt));
    }

    [Theory]
    [InlineData(FamilyRole.FamilyUser)]
    [InlineData(FamilyRole.FamilyAdmin)]
    public void Create_WithValidRoles_ShouldCreateEvent(FamilyRole role)
    {
        // Arrange
        var memberUserId = _fixture.Create<Guid>();
        var joinedAt = DateTime.UtcNow;

        // Act
        var domainEvent = FamilyMemberAdded.Create(memberUserId, role, joinedAt);

        // Assert
        domainEvent.Role.Should().Be(role);
    }

    [Fact]
    public void Create_WithInvalidRole_ShouldThrowArgumentException()
    {
        // Arrange
        var memberUserId = _fixture.Create<Guid>();
        var invalidRole = (FamilyRole)999;
        var joinedAt = DateTime.UtcNow;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => FamilyMemberAdded.Create(memberUserId, invalidRole, joinedAt));
    }

    [Fact]
    public void Create_WithFutureJoinedAt_ShouldThrowArgumentException()
    {
        // Arrange
        var memberUserId = _fixture.Create<Guid>();
        var role = FamilyRole.FamilyUser;
        var futureDate = DateTime.UtcNow.AddDays(1);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => FamilyMemberAdded.Create(memberUserId, role, futureDate));
    }

    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateEventWithCorrectProperties()
    {
        // Arrange
        var eventId = _fixture.Create<Guid>();
        var memberUserId = _fixture.Create<Guid>();
        var role = FamilyRole.FamilyAdmin;
        var joinedAt = DateTime.UtcNow;
        var occurredAt = DateTime.UtcNow;

        // Act
        var domainEvent = new FamilyMemberAdded(eventId, memberUserId, role, joinedAt, occurredAt);

        // Assert
        domainEvent.EventId.Should().Be(eventId);
        domainEvent.MemberUserId.Should().Be(memberUserId);
        domainEvent.Role.Should().Be(role);
        domainEvent.JoinedAt.Should().Be(joinedAt);
        domainEvent.OccurredAt.Should().Be(occurredAt);
    }

    [Fact]
    public void Constructor_WithEmptyEventId_ShouldThrowArgumentException()
    {
        // Arrange
        var memberUserId = _fixture.Create<Guid>();
        var role = FamilyRole.FamilyUser;
        var joinedAt = DateTime.UtcNow;
        var occurredAt = DateTime.UtcNow;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new FamilyMemberAdded(Guid.Empty, memberUserId, role, joinedAt, occurredAt));
    }

    [Fact]
    public void Constructor_WithEmptyMemberUserId_ShouldThrowArgumentException()
    {
        // Arrange
        var eventId = _fixture.Create<Guid>();
        var role = FamilyRole.FamilyUser;
        var joinedAt = DateTime.UtcNow;
        var occurredAt = DateTime.UtcNow;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new FamilyMemberAdded(eventId, Guid.Empty, role, joinedAt, occurredAt));
    }

    [Fact]
    public void Constructor_WithFutureOccurredAt_ShouldThrowArgumentException()
    {
        // Arrange
        var eventId = _fixture.Create<Guid>();
        var memberUserId = _fixture.Create<Guid>();
        var role = FamilyRole.FamilyUser;
        var joinedAt = DateTime.UtcNow;
        var futureDate = DateTime.UtcNow.AddDays(1);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new FamilyMemberAdded(eventId, memberUserId, role, joinedAt, futureDate));
    }

    [Fact]
    public void Equality_WithSameEventId_ShouldBeEqual()
    {
        // Arrange
        var eventId = _fixture.Create<Guid>();
        var memberUserId1 = _fixture.Create<Guid>();
        var memberUserId2 = _fixture.Create<Guid>();
        var joinedAt = DateTime.UtcNow;
        var occurredAt = DateTime.UtcNow;

        var event1 = new FamilyMemberAdded(eventId, memberUserId1, FamilyRole.FamilyUser, joinedAt, occurredAt);
        var event2 = new FamilyMemberAdded(eventId, memberUserId2, FamilyRole.FamilyAdmin, joinedAt, occurredAt);

        // Act & Assert
        event1.Should().Be(event2);
        event1.GetHashCode().Should().Be(event2.GetHashCode());
    }

    [Fact]
    public void Equality_WithDifferentEventId_ShouldNotBeEqual()
    {
        // Arrange
        var memberUserId = _fixture.Create<Guid>();
        var role = FamilyRole.FamilyUser;
        var joinedAt = DateTime.UtcNow;
        var occurredAt = DateTime.UtcNow;

        var event1 = new FamilyMemberAdded(_fixture.Create<Guid>(), memberUserId, role, joinedAt, occurredAt);
        var event2 = new FamilyMemberAdded(_fixture.Create<Guid>(), memberUserId, role, joinedAt, occurredAt);

        // Act & Assert
        event1.Should().NotBe(event2);
    }

    [Fact]
    public void ToString_ShouldReturnMeaningfulString()
    {
        // Arrange
        var memberUserId = _fixture.Create<Guid>();
        var role = FamilyRole.FamilyAdmin;
        var joinedAt = DateTime.UtcNow;
        var domainEvent = FamilyMemberAdded.Create(memberUserId, role, joinedAt);

        // Act
        var result = domainEvent.ToString();

        // Assert
        result.Should().Contain("FamilyMemberAdded");
        result.Should().Contain(memberUserId.ToString());
        result.Should().Contain(role.ToString());
    }
}