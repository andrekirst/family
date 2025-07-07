using Family.Api.Authorization;
using FluentAssertions;

namespace Family.Api.Tests.Authorization;

public class FamilyAuthorizationConstantsTests
{
    [Fact]
    public void Policies_ShouldHaveCorrectFamilyUserValue()
    {
        // Act & Assert
        Policies.FamilyUser.Should().Be("FamilyUser");
    }

    [Fact]
    public void Policies_ShouldHaveCorrectFamilyAdminValue()
    {
        // Act & Assert
        Policies.FamilyAdmin.Should().Be("FamilyAdmin");
    }

    [Fact]
    public void Policies_FamilyUserAndFamilyAdminShouldBeDifferent()
    {
        // Act & Assert
        Policies.FamilyUser.Should().NotBe(Policies.FamilyAdmin);
    }

    [Fact]
    public void Roles_ShouldHaveCorrectFamilyUserValue()
    {
        // Act & Assert
        Roles.FamilyUser.Should().Be("family-user");
    }

    [Fact]
    public void Roles_ShouldHaveCorrectFamilyAdminValue()
    {
        // Act & Assert
        Roles.FamilyAdmin.Should().Be("family-admin");
    }

    [Fact]
    public void Roles_FamilyUserAndFamilyAdminShouldBeDifferent()
    {
        // Act & Assert
        Roles.FamilyUser.Should().NotBe(Roles.FamilyAdmin);
    }

    [Fact]
    public void Claims_ShouldHaveCorrectFamilyIdValue()
    {
        // Act & Assert
        Claims.FamilyId.Should().Be("family_id");
    }

    [Fact]
    public void Claims_ShouldHaveCorrectFamilyRoleValue()
    {
        // Act & Assert
        Claims.FamilyRole.Should().Be("family_role");
    }

    [Fact]
    public void Claims_FamilyIdAndFamilyRoleShouldBeDifferent()
    {
        // Act & Assert
        Claims.FamilyId.Should().NotBe(Claims.FamilyRole);
    }

    [Theory]
    [InlineData("FamilyUser")]
    [InlineData("FamilyAdmin")]
    public void Policies_ShouldNotBeNullOrEmpty(string policyName)
    {
        // Arrange
        var policyValue = typeof(Policies).GetField(policyName)?.GetValue(null) as string;

        // Act & Assert
        policyValue.Should().NotBeNullOrEmpty();
    }

    [Theory]
    [InlineData("FamilyUser")]
    [InlineData("FamilyAdmin")]
    public void Roles_ShouldNotBeNullOrEmpty(string roleName)
    {
        // Arrange
        var roleValue = typeof(Roles).GetField(roleName)?.GetValue(null) as string;

        // Act & Assert
        roleValue.Should().NotBeNullOrEmpty();
    }

    [Theory]
    [InlineData("FamilyId")]
    [InlineData("FamilyRole")]
    public void Claims_ShouldNotBeNullOrEmpty(string claimName)
    {
        // Arrange
        var claimValue = typeof(Claims).GetField(claimName)?.GetValue(null) as string;

        // Act & Assert
        claimValue.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Policies_ShouldBeConstant()
    {
        // Arrange
        var familyUserField = typeof(Policies).GetField(nameof(Policies.FamilyUser));
        var familyAdminField = typeof(Policies).GetField(nameof(Policies.FamilyAdmin));

        // Act & Assert
        familyUserField.Should().NotBeNull();
        familyUserField!.IsLiteral.Should().BeTrue();
        familyUserField.IsStatic.Should().BeTrue();

        familyAdminField.Should().NotBeNull();
        familyAdminField!.IsLiteral.Should().BeTrue();
        familyAdminField.IsStatic.Should().BeTrue();
    }

    [Fact]
    public void Roles_ShouldBeConstant()
    {
        // Arrange
        var familyUserField = typeof(Roles).GetField(nameof(Roles.FamilyUser));
        var familyAdminField = typeof(Roles).GetField(nameof(Roles.FamilyAdmin));

        // Act & Assert
        familyUserField.Should().NotBeNull();
        familyUserField!.IsLiteral.Should().BeTrue();
        familyUserField.IsStatic.Should().BeTrue();

        familyAdminField.Should().NotBeNull();
        familyAdminField!.IsLiteral.Should().BeTrue();
        familyAdminField.IsStatic.Should().BeTrue();
    }

    [Fact]
    public void Claims_ShouldBeConstant()
    {
        // Arrange
        var familyIdField = typeof(Claims).GetField(nameof(Claims.FamilyId));
        var familyRoleField = typeof(Claims).GetField(nameof(Claims.FamilyRole));

        // Act & Assert
        familyIdField.Should().NotBeNull();
        familyIdField!.IsLiteral.Should().BeTrue();
        familyIdField.IsStatic.Should().BeTrue();

        familyRoleField.Should().NotBeNull();
        familyRoleField!.IsLiteral.Should().BeTrue();
        familyRoleField.IsStatic.Should().BeTrue();
    }

    [Fact]
    public void Policies_ShouldFollowNamingConvention()
    {
        // Act & Assert
        Policies.FamilyUser.Should().Be("FamilyUser");
        Policies.FamilyAdmin.Should().Be("FamilyAdmin");
        
        // Policies should use PascalCase
        Policies.FamilyUser.Should().MatchRegex(@"^[A-Z][a-zA-Z]*$");
        Policies.FamilyAdmin.Should().MatchRegex(@"^[A-Z][a-zA-Z]*$");
    }

    [Fact]
    public void Roles_ShouldFollowNamingConvention()
    {
        // Act & Assert
        Roles.FamilyUser.Should().Be("family-user");
        Roles.FamilyAdmin.Should().Be("family-admin");
        
        // Roles should use kebab-case
        Roles.FamilyUser.Should().MatchRegex(@"^[a-z]+(-[a-z]+)*$");
        Roles.FamilyAdmin.Should().MatchRegex(@"^[a-z]+(-[a-z]+)*$");
    }

    [Fact]
    public void Claims_ShouldFollowNamingConvention()
    {
        // Act & Assert
        Claims.FamilyId.Should().Be("family_id");
        Claims.FamilyRole.Should().Be("family_role");
        
        // Claims should use snake_case
        Claims.FamilyId.Should().MatchRegex(@"^[a-z]+(_[a-z]+)*$");
        Claims.FamilyRole.Should().MatchRegex(@"^[a-z]+(_[a-z]+)*$");
    }
}