using AutoFixture;
using Family.Api.Features.Families.DomainEvents;
using FluentAssertions;

namespace Family.Api.Tests.Features.Families.DomainEvents;

public class FamilyCreatedTests
{
    private readonly Fixture _fixture = new();

    [Fact]
    public void Create_WithValidNameAndOwnerId_ShouldCreateEventWithCorrectProperties()
    {
        // Arrange
        var name = _fixture.Create<string>();
        var ownerId = _fixture.Create<Guid>();

        // Act
        var domainEvent = FamilyCreated.Create(name, ownerId);

        // Assert
        domainEvent.Should().NotBeNull();
        domainEvent.Name.Should().Be(name);
        domainEvent.OwnerId.Should().Be(ownerId);
        domainEvent.OccurredAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        domainEvent.EventId.Should().NotBeEmpty();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidName_ShouldThrowArgumentException(string invalidName)
    {
        // Arrange
        var ownerId = _fixture.Create<Guid>();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => FamilyCreated.Create(invalidName, ownerId));
    }

    [Fact]
    public void Create_WithEmptyOwnerId_ShouldThrowArgumentException()
    {
        // Arrange
        var name = _fixture.Create<string>();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => FamilyCreated.Create(name, Guid.Empty));
    }

    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateEventWithCorrectProperties()
    {
        // Arrange
        var eventId = _fixture.Create<Guid>();
        var name = _fixture.Create<string>();
        var ownerId = _fixture.Create<Guid>();
        var occurredAt = DateTime.UtcNow;

        // Act
        var domainEvent = new FamilyCreated(eventId, name, ownerId, occurredAt);

        // Assert
        domainEvent.EventId.Should().Be(eventId);
        domainEvent.Name.Should().Be(name);
        domainEvent.OwnerId.Should().Be(ownerId);
        domainEvent.OccurredAt.Should().Be(occurredAt);
    }

    [Fact]
    public void Constructor_WithEmptyEventId_ShouldThrowArgumentException()
    {
        // Arrange
        var name = _fixture.Create<string>();
        var ownerId = _fixture.Create<Guid>();
        var occurredAt = DateTime.UtcNow;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new FamilyCreated(Guid.Empty, name, ownerId, occurredAt));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidName_ShouldThrowArgumentException(string invalidName)
    {
        // Arrange
        var eventId = _fixture.Create<Guid>();
        var ownerId = _fixture.Create<Guid>();
        var occurredAt = DateTime.UtcNow;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new FamilyCreated(eventId, invalidName, ownerId, occurredAt));
    }

    [Fact]
    public void Constructor_WithEmptyOwnerId_ShouldThrowArgumentException()
    {
        // Arrange
        var eventId = _fixture.Create<Guid>();
        var name = _fixture.Create<string>();
        var occurredAt = DateTime.UtcNow;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new FamilyCreated(eventId, name, Guid.Empty, occurredAt));
    }

    [Fact]
    public void Constructor_WithFutureOccurredAt_ShouldThrowArgumentException()
    {
        // Arrange
        var eventId = _fixture.Create<Guid>();
        var name = _fixture.Create<string>();
        var ownerId = _fixture.Create<Guid>();
        var futureDate = DateTime.UtcNow.AddDays(1);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new FamilyCreated(eventId, name, ownerId, futureDate));
    }

    [Fact]
    public void Equality_WithSameEventId_ShouldBeEqual()
    {
        // Arrange
        var eventId = _fixture.Create<Guid>();
        var name1 = _fixture.Create<string>();
        var name2 = _fixture.Create<string>();
        var ownerId1 = _fixture.Create<Guid>();
        var ownerId2 = _fixture.Create<Guid>();
        var occurredAt = DateTime.UtcNow;

        var event1 = new FamilyCreated(eventId, name1, ownerId1, occurredAt);
        var event2 = new FamilyCreated(eventId, name2, ownerId2, occurredAt);

        // Act & Assert
        event1.Should().Be(event2);
        event1.GetHashCode().Should().Be(event2.GetHashCode());
    }

    [Fact]
    public void Equality_WithDifferentEventId_ShouldNotBeEqual()
    {
        // Arrange
        var name = _fixture.Create<string>();
        var ownerId = _fixture.Create<Guid>();
        var occurredAt = DateTime.UtcNow;

        var event1 = new FamilyCreated(_fixture.Create<Guid>(), name, ownerId, occurredAt);
        var event2 = new FamilyCreated(_fixture.Create<Guid>(), name, ownerId, occurredAt);

        // Act & Assert
        event1.Should().NotBe(event2);
    }

    [Fact]
    public void ToString_ShouldReturnMeaningfulString()
    {
        // Arrange
        var name = "Test Family";
        var ownerId = _fixture.Create<Guid>();
        var domainEvent = FamilyCreated.Create(name, ownerId);

        // Act
        var result = domainEvent.ToString();

        // Assert
        result.Should().Contain("FamilyCreated");
        result.Should().Contain(name);
        result.Should().Contain(ownerId.ToString());
    }
}