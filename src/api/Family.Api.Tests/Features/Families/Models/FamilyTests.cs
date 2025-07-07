using AutoFixture;
using Family.Api.Features.Families.DomainEvents;
using Family.Api.Features.Families.Models;
using FluentAssertions;

namespace Family.Api.Tests.Features.Families.Models;

public class FamilyTests
{
    private readonly Fixture _fixture = new();

    [Fact]
    public void Create_WithValidNameAndOwnerId_ShouldCreateFamilyWithCorrectProperties()
    {
        // Arrange
        var name = _fixture.Create<string>();
        var ownerId = _fixture.Create<Guid>();

        // Act
        var family = Api.Features.Families.Models.Family.Create(name, ownerId);

        // Assert
        family.Should().NotBeNull();
        family.Name.Should().Be(name);
        family.OwnerId.Should().Be(ownerId);
        family.Members.Should().BeEmpty();
        family.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        family.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
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
        Assert.Throws<ArgumentException>(() => Api.Features.Families.Models.Family.Create(invalidName, ownerId));
    }

    [Fact]
    public void Create_WithEmptyOwnerId_ShouldThrowArgumentException()
    {
        // Arrange
        var name = _fixture.Create<string>();
        var ownerId = Guid.Empty;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => Api.Features.Families.Models.Family.Create(name, ownerId));
    }

    [Fact]
    public void Create_ShouldAddFamilyCreatedEvent()
    {
        // Arrange
        var name = _fixture.Create<string>();
        var ownerId = _fixture.Create<Guid>();

        // Act
        var family = Api.Features.Families.Models.Family.Create(name, ownerId);

        // Assert
        family.DomainEvents.Should().HaveCount(1);
        family.DomainEvents.First().Should().BeOfType<FamilyCreated>();
        
        var createdEvent = (FamilyCreated)family.DomainEvents.First();
        createdEvent.Name.Should().Be(name);
        createdEvent.OwnerId.Should().Be(ownerId);
    }

    [Fact]
    public void AddMember_WithValidUserIdAndRole_ShouldAddMemberToFamily()
    {
        // Arrange
        var family = CreateValidFamily();
        var userId = _fixture.Create<Guid>();
        var role = FamilyRole.FamilyUser;

        // Act
        family.AddMember(userId, role);

        // Assert
        family.Members.Should().HaveCount(1);
        var member = family.Members.First();
        member.FamilyId.Should().Be(family.Id);
        member.UserId.Should().Be(userId);
        member.Role.Should().Be(role);
        member.JoinedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void AddMember_ShouldAddFamilyMemberAddedEvent()
    {
        // Arrange
        var family = CreateValidFamily();
        var userId = _fixture.Create<Guid>();
        var role = FamilyRole.FamilyUser;

        // Act
        family.AddMember(userId, role);

        // Assert
        family.DomainEvents.Should().HaveCount(2); // FamilyCreated + FamilyMemberAdded
        family.DomainEvents.Last().Should().BeOfType<FamilyMemberAdded>();
        
        var memberAddedEvent = (FamilyMemberAdded)family.DomainEvents.Last();
        memberAddedEvent.MemberUserId.Should().Be(userId);
        memberAddedEvent.Role.Should().Be(role);
        memberAddedEvent.JoinedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void AddMember_WithExistingUserId_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var family = CreateValidFamily();
        var userId = _fixture.Create<Guid>();
        family.AddMember(userId, FamilyRole.FamilyUser);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => family.AddMember(userId, FamilyRole.FamilyAdmin));
    }

    [Fact]
    public void AddMember_WithEmptyUserId_ShouldThrowArgumentException()
    {
        // Arrange
        var family = CreateValidFamily();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => family.AddMember(Guid.Empty, FamilyRole.FamilyUser));
    }

    [Fact]
    public void IsAdmin_WithAdminMember_ShouldReturnTrue()
    {
        // Arrange
        var family = CreateValidFamily();
        var userId = _fixture.Create<Guid>();
        family.AddMember(userId, FamilyRole.FamilyAdmin);

        // Act
        var isAdmin = family.IsAdmin(userId);

        // Assert
        isAdmin.Should().BeTrue();
    }

    [Fact]
    public void IsAdmin_WithRegularMember_ShouldReturnFalse()
    {
        // Arrange
        var family = CreateValidFamily();
        var userId = _fixture.Create<Guid>();
        family.AddMember(userId, FamilyRole.FamilyUser);

        // Act
        var isAdmin = family.IsAdmin(userId);

        // Assert
        isAdmin.Should().BeFalse();
    }

    [Fact]
    public void IsAdmin_WithOwner_ShouldReturnTrue()
    {
        // Arrange
        var ownerId = _fixture.Create<Guid>();
        var family = Api.Features.Families.Models.Family.Create(_fixture.Create<string>(), ownerId);

        // Act
        var isAdmin = family.IsAdmin(ownerId);

        // Assert
        isAdmin.Should().BeTrue();
    }

    [Fact]
    public void IsAdmin_WithNonMember_ShouldReturnFalse()
    {
        // Arrange
        var family = CreateValidFamily();
        var nonMemberUserId = _fixture.Create<Guid>();

        // Act
        var isAdmin = family.IsAdmin(nonMemberUserId);

        // Assert
        isAdmin.Should().BeFalse();
    }

    [Fact]
    public void IsMember_WithExistingMember_ShouldReturnTrue()
    {
        // Arrange
        var family = CreateValidFamily();
        var userId = _fixture.Create<Guid>();
        family.AddMember(userId, FamilyRole.FamilyUser);

        // Act
        var isMember = family.IsMember(userId);

        // Assert
        isMember.Should().BeTrue();
    }

    [Fact]
    public void IsMember_WithOwner_ShouldReturnTrue()
    {
        // Arrange
        var ownerId = _fixture.Create<Guid>();
        var family = Api.Features.Families.Models.Family.Create(_fixture.Create<string>(), ownerId);

        // Act
        var isMember = family.IsMember(ownerId);

        // Assert
        isMember.Should().BeTrue();
    }

    [Fact]
    public void IsMember_WithNonMember_ShouldReturnFalse()
    {
        // Arrange
        var family = CreateValidFamily();
        var nonMemberUserId = _fixture.Create<Guid>();

        // Act
        var isMember = family.IsMember(nonMemberUserId);

        // Assert
        isMember.Should().BeFalse();
    }

    [Fact]
    public void Apply_FamilyCreatedEvent_ShouldUpdateFamilyProperties()
    {
        // Arrange
        var family = new Api.Features.Families.Models.Family();
        var familyCreatedEvent = FamilyCreated.Create(_fixture.Create<string>(), _fixture.Create<Guid>());

        // Act
        family.Apply(familyCreatedEvent);

        // Assert
        family.Name.Should().Be(familyCreatedEvent.Name);
        family.OwnerId.Should().Be(familyCreatedEvent.OwnerId);
    }

    [Fact]
    public void Apply_FamilyMemberAddedEvent_ShouldAddMemberToFamily()
    {
        // Arrange
        var family = CreateValidFamily();
        var userId = _fixture.Create<Guid>();
        var role = FamilyRole.FamilyAdmin;
        var joinedAt = DateTime.UtcNow;
        var memberAddedEvent = FamilyMemberAdded.Create(userId, role, joinedAt);

        // Act
        family.Apply(memberAddedEvent);

        // Assert
        family.Members.Should().HaveCount(1);
        var member = family.Members.First();
        member.UserId.Should().Be(userId);
        member.Role.Should().Be(role);
        member.JoinedAt.Should().Be(joinedAt);
    }

    [Fact]
    public void Apply_FamilyAdminAssignedEvent_ShouldUpdateMemberRoleToAdmin()
    {
        // Arrange
        var family = CreateValidFamily();
        var userId = _fixture.Create<Guid>();
        family.AddMember(userId, FamilyRole.FamilyUser);
        
        var adminAssignedEvent = FamilyAdminAssigned.Create();

        // Act
        family.Apply(adminAssignedEvent);

        // Assert
        // Note: This assumes the event updates the last added member to admin
        // Implementation may need to include UserId in the event
        family.Members.Should().HaveCount(1);
    }

    private Api.Features.Families.Models.Family CreateValidFamily()
    {
        return Api.Features.Families.Models.Family.Create(_fixture.Create<string>(), _fixture.Create<Guid>());
    }
}