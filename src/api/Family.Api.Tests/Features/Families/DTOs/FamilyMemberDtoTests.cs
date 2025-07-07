using AutoFixture;
using Family.Api.Features.Families.DTOs;
using Family.Api.Features.Families.Models;
using FluentAssertions;

namespace Family.Api.Tests.Features.Families.DTOs;

public class FamilyMemberDtoTests
{
    private readonly Fixture _fixture = new();

    [Fact]
    public void FromDomain_WithValidFamilyMember_ShouldCreateDtoWithCorrectProperties()
    {
        // Arrange
        var familyId = _fixture.Create<Guid>();
        var userId = _fixture.Create<Guid>();
        var role = FamilyRole.FamilyAdmin;
        var joinedAt = DateTime.UtcNow;
        var member = new FamilyMember(familyId, userId, role, joinedAt);

        // Act
        var dto = FamilyMemberDto.FromDomain(member);

        // Assert
        dto.Should().NotBeNull();
        dto.FamilyId.Should().Be(familyId);
        dto.UserId.Should().Be(userId);
        dto.Role.Should().Be(role);
        dto.JoinedAt.Should().Be(joinedAt);
    }

    [Fact]
    public void FromDomain_WithNullFamilyMember_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => FamilyMemberDto.FromDomain(null!));
    }

    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateDtoWithCorrectProperties()
    {
        // Arrange
        var familyId = _fixture.Create<Guid>();
        var userId = _fixture.Create<Guid>();
        var role = FamilyRole.FamilyUser;
        var joinedAt = DateTime.UtcNow;

        // Act
        var dto = new FamilyMemberDto(familyId, userId, role, joinedAt);

        // Assert
        dto.FamilyId.Should().Be(familyId);
        dto.UserId.Should().Be(userId);
        dto.Role.Should().Be(role);
        dto.JoinedAt.Should().Be(joinedAt);
    }

    [Fact]
    public void Constructor_WithEmptyFamilyId_ShouldThrowArgumentException()
    {
        // Arrange
        var userId = _fixture.Create<Guid>();
        var role = FamilyRole.FamilyUser;
        var joinedAt = DateTime.UtcNow;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new FamilyMemberDto(Guid.Empty, userId, role, joinedAt));
    }

    [Fact]
    public void Constructor_WithEmptyUserId_ShouldThrowArgumentException()
    {
        // Arrange
        var familyId = _fixture.Create<Guid>();
        var role = FamilyRole.FamilyUser;
        var joinedAt = DateTime.UtcNow;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new FamilyMemberDto(familyId, Guid.Empty, role, joinedAt));
    }

    [Theory]
    [InlineData(FamilyRole.FamilyUser)]
    [InlineData(FamilyRole.FamilyAdmin)]
    public void Constructor_WithValidRoles_ShouldCreateDto(FamilyRole role)
    {
        // Arrange
        var familyId = _fixture.Create<Guid>();
        var userId = _fixture.Create<Guid>();
        var joinedAt = DateTime.UtcNow;

        // Act
        var dto = new FamilyMemberDto(familyId, userId, role, joinedAt);

        // Assert
        dto.Role.Should().Be(role);
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
        Assert.Throws<ArgumentException>(() => new FamilyMemberDto(familyId, userId, invalidRole, joinedAt));
    }

    [Fact]
    public void Equality_WithSameProperties_ShouldBeEqual()
    {
        // Arrange
        var familyId = _fixture.Create<Guid>();
        var userId = _fixture.Create<Guid>();
        var role = FamilyRole.FamilyUser;
        var joinedAt = DateTime.UtcNow;

        var dto1 = new FamilyMemberDto(familyId, userId, role, joinedAt);
        var dto2 = new FamilyMemberDto(familyId, userId, role, joinedAt);

        // Act & Assert
        dto1.Should().Be(dto2);
        dto1.GetHashCode().Should().Be(dto2.GetHashCode());
    }

    [Fact]
    public void Equality_WithDifferentFamilyId_ShouldNotBeEqual()
    {
        // Arrange
        var userId = _fixture.Create<Guid>();
        var role = FamilyRole.FamilyUser;
        var joinedAt = DateTime.UtcNow;

        var dto1 = new FamilyMemberDto(_fixture.Create<Guid>(), userId, role, joinedAt);
        var dto2 = new FamilyMemberDto(_fixture.Create<Guid>(), userId, role, joinedAt);

        // Act & Assert
        dto1.Should().NotBe(dto2);
    }

    [Fact]
    public void Equality_WithDifferentUserId_ShouldNotBeEqual()
    {
        // Arrange
        var familyId = _fixture.Create<Guid>();
        var role = FamilyRole.FamilyUser;
        var joinedAt = DateTime.UtcNow;

        var dto1 = new FamilyMemberDto(familyId, _fixture.Create<Guid>(), role, joinedAt);
        var dto2 = new FamilyMemberDto(familyId, _fixture.Create<Guid>(), role, joinedAt);

        // Act & Assert
        dto1.Should().NotBe(dto2);
    }

    [Fact]
    public void ToString_ShouldReturnMeaningfulString()
    {
        // Arrange
        var familyId = _fixture.Create<Guid>();
        var userId = _fixture.Create<Guid>();
        var role = FamilyRole.FamilyAdmin;
        var joinedAt = DateTime.UtcNow;

        var dto = new FamilyMemberDto(familyId, userId, role, joinedAt);

        // Act
        var result = dto.ToString();

        // Assert
        result.Should().Contain(familyId.ToString());
        result.Should().Contain(userId.ToString());
        result.Should().Contain(role.ToString());
    }
}