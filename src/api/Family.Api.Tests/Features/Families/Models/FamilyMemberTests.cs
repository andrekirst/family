using AutoFixture;
using Family.Api.Features.Families.Models;
using FluentAssertions;

namespace Family.Api.Tests.Features.Families.Models;

public class FamilyMemberTests
{
    private readonly Fixture _fixture = new();

    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateFamilyMemberWithCorrectProperties()
    {
        // Arrange
        var familyId = _fixture.Create<Guid>();
        var userId = _fixture.Create<Guid>();
        var role = FamilyRole.FamilyUser;
        var joinedAt = DateTime.UtcNow;

        // Act
        var member = new FamilyMember(familyId, userId, role, joinedAt);

        // Assert
        member.FamilyId.Should().Be(familyId);
        member.UserId.Should().Be(userId);
        member.Role.Should().Be(role);
        member.JoinedAt.Should().Be(joinedAt);
    }

    [Fact]
    public void Constructor_WithEmptyFamilyId_ShouldThrowArgumentException()
    {
        // Arrange
        var userId = _fixture.Create<Guid>();
        var role = FamilyRole.FamilyUser;
        var joinedAt = DateTime.UtcNow;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new FamilyMember(Guid.Empty, userId, role, joinedAt));
    }

    [Fact]
    public void Constructor_WithEmptyUserId_ShouldThrowArgumentException()
    {
        // Arrange
        var familyId = _fixture.Create<Guid>();
        var role = FamilyRole.FamilyUser;
        var joinedAt = DateTime.UtcNow;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new FamilyMember(familyId, Guid.Empty, role, joinedAt));
    }

    [Theory]
    [InlineData(FamilyRole.FamilyUser)]
    [InlineData(FamilyRole.FamilyAdmin)]
    public void Constructor_WithValidRoles_ShouldCreateFamilyMember(FamilyRole role)
    {
        // Arrange
        var familyId = _fixture.Create<Guid>();
        var userId = _fixture.Create<Guid>();
        var joinedAt = DateTime.UtcNow;

        // Act
        var member = new FamilyMember(familyId, userId, role, joinedAt);

        // Assert
        member.Role.Should().Be(role);
    }

    [Fact]
    public void Constructor_WithInvalidRole_ShouldThrowArgumentException()
    {
        // Arrange
        var familyId = _fixture.Create<Guid>();
        var userId = _fixture.Create<Guid>();
        var invalidRole = (FamilyRole)999;
        var joinedAt = DateTime.UtcNow;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new FamilyMember(familyId, userId, invalidRole, joinedAt));
    }

    [Fact]
    public void Constructor_WithFutureJoinedAt_ShouldThrowArgumentException()
    {
        // Arrange
        var familyId = _fixture.Create<Guid>();
        var userId = _fixture.Create<Guid>();
        var role = FamilyRole.FamilyUser;
        var futureDate = DateTime.UtcNow.AddDays(1);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new FamilyMember(familyId, userId, role, futureDate));
    }

    [Fact]
    public void Equals_WithSameFamilyIdAndUserId_ShouldReturnTrue()
    {
        // Arrange
        var familyId = _fixture.Create<Guid>();
        var userId = _fixture.Create<Guid>();
        var role = FamilyRole.FamilyUser;
        var joinedAt = DateTime.UtcNow;

        var member1 = new FamilyMember(familyId, userId, role, joinedAt);
        var member2 = new FamilyMember(familyId, userId, FamilyRole.FamilyAdmin, joinedAt.AddMinutes(1));

        // Act & Assert
        member1.Should().Be(member2);
        member1.GetHashCode().Should().Be(member2.GetHashCode());
    }

    [Fact]
    public void Equals_WithDifferentFamilyId_ShouldReturnFalse()
    {
        // Arrange
        var familyId1 = _fixture.Create<Guid>();
        var familyId2 = _fixture.Create<Guid>();
        var userId = _fixture.Create<Guid>();
        var role = FamilyRole.FamilyUser;
        var joinedAt = DateTime.UtcNow;

        var member1 = new FamilyMember(familyId1, userId, role, joinedAt);
        var member2 = new FamilyMember(familyId2, userId, role, joinedAt);

        // Act & Assert
        member1.Should().NotBe(member2);
    }

    [Fact]
    public void Equals_WithDifferentUserId_ShouldReturnFalse()
    {
        // Arrange
        var familyId = _fixture.Create<Guid>();
        var userId1 = _fixture.Create<Guid>();
        var userId2 = _fixture.Create<Guid>();
        var role = FamilyRole.FamilyUser;
        var joinedAt = DateTime.UtcNow;

        var member1 = new FamilyMember(familyId, userId1, role, joinedAt);
        var member2 = new FamilyMember(familyId, userId2, role, joinedAt);

        // Act & Assert
        member1.Should().NotBe(member2);
    }

    [Fact]
    public void ToString_ShouldReturnMeaningfulString()
    {
        // Arrange
        var familyId = _fixture.Create<Guid>();
        var userId = _fixture.Create<Guid>();
        var role = FamilyRole.FamilyAdmin;
        var joinedAt = DateTime.UtcNow;

        var member = new FamilyMember(familyId, userId, role, joinedAt);

        // Act
        var result = member.ToString();

        // Assert
        result.Should().Contain(familyId.ToString());
        result.Should().Contain(userId.ToString());
        result.Should().Contain(role.ToString());
    }
}