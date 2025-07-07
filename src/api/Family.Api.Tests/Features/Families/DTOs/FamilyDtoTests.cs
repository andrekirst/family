using AutoFixture;
using Family.Api.Features.Families.DTOs;
using Family.Api.Features.Families.Models;
using FluentAssertions;

namespace Family.Api.Tests.Features.Families.DTOs;

public class FamilyDtoTests
{
    private readonly Fixture _fixture = new();

    [Fact]
    public void FromDomain_WithValidFamily_ShouldCreateDtoWithCorrectProperties()
    {
        // Arrange
        var name = _fixture.Create<string>();
        var ownerId = _fixture.Create<Guid>();
        var family = Api.Features.Families.Models.Family.Create(name, ownerId);
        
        // Add some members
        var userId1 = _fixture.Create<Guid>();
        var userId2 = _fixture.Create<Guid>();
        family.AddMember(userId1, FamilyRole.FamilyUser);
        family.AddMember(userId2, FamilyRole.FamilyAdmin);

        // Act
        var dto = FamilyDto.FromDomain(family);

        // Assert
        dto.Should().NotBeNull();
        dto.Id.Should().Be(family.Id);
        dto.Name.Should().Be(family.Name);
        dto.OwnerId.Should().Be(family.OwnerId);
        dto.CreatedAt.Should().Be(family.CreatedAt);
        dto.UpdatedAt.Should().Be(family.UpdatedAt);
        dto.Members.Should().HaveCount(2);
        
        dto.Members.Should().Contain(m => m.UserId == userId1 && m.Role == FamilyRole.FamilyUser);
        dto.Members.Should().Contain(m => m.UserId == userId2 && m.Role == FamilyRole.FamilyAdmin);
    }

    [Fact]
    public void FromDomain_WithFamilyWithoutMembers_ShouldCreateDtoWithEmptyMembersList()
    {
        // Arrange
        var name = _fixture.Create<string>();
        var ownerId = _fixture.Create<Guid>();
        var family = Api.Features.Families.Models.Family.Create(name, ownerId);

        // Act
        var dto = FamilyDto.FromDomain(family);

        // Assert
        dto.Should().NotBeNull();
        dto.Members.Should().BeEmpty();
    }

    [Fact]
    public void FromDomain_WithNullFamily_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => FamilyDto.FromDomain(null!));
    }

    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateDtoWithCorrectProperties()
    {
        // Arrange
        var id = _fixture.Create<Guid>();
        var name = _fixture.Create<string>();
        var ownerId = _fixture.Create<Guid>();
        var createdAt = _fixture.Create<DateTime>();
        var updatedAt = _fixture.Create<DateTime>();
        var members = new List<FamilyMemberDto>
        {
            new(id, _fixture.Create<Guid>(), FamilyRole.FamilyUser, DateTime.UtcNow),
            new(id, _fixture.Create<Guid>(), FamilyRole.FamilyAdmin, DateTime.UtcNow)
        };

        // Act
        var dto = new FamilyDto(id, name, ownerId, createdAt, updatedAt, members);

        // Assert
        dto.Id.Should().Be(id);
        dto.Name.Should().Be(name);
        dto.OwnerId.Should().Be(ownerId);
        dto.CreatedAt.Should().Be(createdAt);
        dto.UpdatedAt.Should().Be(updatedAt);
        dto.Members.Should().BeEquivalentTo(members);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidName_ShouldThrowArgumentException(string invalidName)
    {
        // Arrange
        var id = _fixture.Create<Guid>();
        var ownerId = _fixture.Create<Guid>();
        var createdAt = _fixture.Create<DateTime>();
        var updatedAt = _fixture.Create<DateTime>();
        var members = new List<FamilyMemberDto>();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            new FamilyDto(id, invalidName, ownerId, createdAt, updatedAt, members));
    }

    [Fact]
    public void Constructor_WithEmptyId_ShouldThrowArgumentException()
    {
        // Arrange
        var name = _fixture.Create<string>();
        var ownerId = _fixture.Create<Guid>();
        var createdAt = _fixture.Create<DateTime>();
        var updatedAt = _fixture.Create<DateTime>();
        var members = new List<FamilyMemberDto>();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            new FamilyDto(Guid.Empty, name, ownerId, createdAt, updatedAt, members));
    }

    [Fact]
    public void Constructor_WithEmptyOwnerId_ShouldThrowArgumentException()
    {
        // Arrange
        var id = _fixture.Create<Guid>();
        var name = _fixture.Create<string>();
        var createdAt = _fixture.Create<DateTime>();
        var updatedAt = _fixture.Create<DateTime>();
        var members = new List<FamilyMemberDto>();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            new FamilyDto(id, name, Guid.Empty, createdAt, updatedAt, members));
    }

    [Fact]
    public void Constructor_WithNullMembers_ShouldThrowArgumentNullException()
    {
        // Arrange
        var id = _fixture.Create<Guid>();
        var name = _fixture.Create<string>();
        var ownerId = _fixture.Create<Guid>();
        var createdAt = _fixture.Create<DateTime>();
        var updatedAt = _fixture.Create<DateTime>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new FamilyDto(id, name, ownerId, createdAt, updatedAt, null!));
    }

    [Fact]
    public void Equality_WithSameProperties_ShouldBeEqual()
    {
        // Arrange
        var id = _fixture.Create<Guid>();
        var name = _fixture.Create<string>();
        var ownerId = _fixture.Create<Guid>();
        var createdAt = _fixture.Create<DateTime>();
        var updatedAt = _fixture.Create<DateTime>();
        var members = new List<FamilyMemberDto>
        {
            new(id, _fixture.Create<Guid>(), FamilyRole.FamilyUser, DateTime.UtcNow)
        };

        var dto1 = new FamilyDto(id, name, ownerId, createdAt, updatedAt, members);
        var dto2 = new FamilyDto(id, name, ownerId, createdAt, updatedAt, members);

        // Act & Assert
        dto1.Should().Be(dto2);
        dto1.GetHashCode().Should().Be(dto2.GetHashCode());
    }

    [Fact]
    public void Equality_WithDifferentId_ShouldNotBeEqual()
    {
        // Arrange
        var name = _fixture.Create<string>();
        var ownerId = _fixture.Create<Guid>();
        var createdAt = _fixture.Create<DateTime>();
        var updatedAt = _fixture.Create<DateTime>();
        var members = new List<FamilyMemberDto>();

        var dto1 = new FamilyDto(_fixture.Create<Guid>(), name, ownerId, createdAt, updatedAt, members);
        var dto2 = new FamilyDto(_fixture.Create<Guid>(), name, ownerId, createdAt, updatedAt, members);

        // Act & Assert
        dto1.Should().NotBe(dto2);
    }
}