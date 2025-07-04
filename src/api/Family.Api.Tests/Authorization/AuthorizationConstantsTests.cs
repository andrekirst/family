using Family.Api.Authorization;
using FluentAssertions;

namespace Family.Api.Tests.Authorization;

public class AuthorizationConstantsTests
{
    [Fact]
    public void Policies_ShouldHaveCorrectValues()
    {
        Policies.FamilyUser.Should().Be("FamilyUser");
        Policies.FamilyAdmin.Should().Be("FamilyAdmin");
    }

    [Fact]
    public void Claims_ShouldHaveCorrectValues()
    {
        Claims.FamilyRoles.Should().Be("family_roles");
    }

    [Fact]
    public void Roles_ShouldHaveCorrectValues()
    {
        Roles.FamilyUser.Should().Be("family-user");
        Roles.FamilyAdmin.Should().Be("family-admin");
    }

    [Theory]
    [InlineData(nameof(Policies.FamilyUser))]
    [InlineData(nameof(Policies.FamilyAdmin))]
    public void Policies_ShouldNotBeNullOrEmpty(string policyName)
    {
        var policyValue = typeof(Policies).GetField(policyName)?.GetValue(null) as string;
        policyValue.Should().NotBeNullOrEmpty();
    }

    [Theory]
    [InlineData(nameof(Claims.FamilyRoles))]
    public void Claims_ShouldNotBeNullOrEmpty(string claimName)
    {
        var claimValue = typeof(Claims).GetField(claimName)?.GetValue(null) as string;
        claimValue.Should().NotBeNullOrEmpty();
    }

    [Theory]
    [InlineData(nameof(Roles.FamilyUser))]
    [InlineData(nameof(Roles.FamilyAdmin))]
    public void Roles_ShouldNotBeNullOrEmpty(string roleName)
    {
        var roleValue = typeof(Roles).GetField(roleName)?.GetValue(null) as string;
        roleValue.Should().NotBeNullOrEmpty();
    }
}