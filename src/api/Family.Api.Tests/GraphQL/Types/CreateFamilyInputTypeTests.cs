using AutoFixture;
using Family.Api.GraphQL.Types;
using FluentAssertions;
using HotChocolate.Execution;
using HotChocolate.Types;
using Microsoft.Extensions.DependencyInjection;

namespace Family.Api.Tests.GraphQL.Types;

public class CreateFamilyInputTypeTests
{
    private readonly Fixture _fixture = new();

    [Fact]
    public void CreateFamilyInput_WithValidName_ShouldCreateInput()
    {
        // Arrange
        var name = _fixture.Create<string>();

        // Act
        var input = new CreateFamilyInput { Name = name };

        // Assert
        input.Name.Should().Be(name);
    }

    [Fact]
    public void CreateFamilyInput_DefaultConstructor_ShouldInitializeWithEmptyName()
    {
        // Act
        var input = new CreateFamilyInput();

        // Assert
        input.Name.Should().Be(string.Empty);
    }

    [Fact]
    public async Task CreateFamilyInputType_Schema_ShouldHaveCorrectConfiguration()
    {
        // Arrange
        var schema = await new ServiceCollection()
            .AddGraphQLServer()
            .AddType<CreateFamilyInputType>()
            .BuildSchemaAsync();

        // Act
        var inputType = schema.GetType<CreateFamilyInputType>("CreateFamilyInput");

        // Assert
        inputType.Should().NotBeNull();
        inputType.Name.Should().Be("CreateFamilyInput");
        inputType.Description.Should().Be("Input for creating a new family");
    }

    [Fact]
    public async Task CreateFamilyInputType_NameField_ShouldBeNonNull()
    {
        // Arrange
        var schema = await new ServiceCollection()
            .AddGraphQLServer()
            .AddType<CreateFamilyInputType>()
            .BuildSchemaAsync();

        // Act
        var inputType = schema.GetType<CreateFamilyInputType>("CreateFamilyInput");
        var nameField = inputType.Fields["name"];

        // Assert
        nameField.Should().NotBeNull();
        nameField.Type.IsNonNullType().Should().BeTrue();
        nameField.Description.Should().Be("The name of the family");
    }

    [Theory]
    [InlineData("")]
    [InlineData("Test Family")]
    [InlineData("Familie Müller")]
    [InlineData("Family with special chars: !@#$%")]
    public void CreateFamilyInput_WithVariousNames_ShouldAcceptAllValues(string name)
    {
        // Act
        var input = new CreateFamilyInput { Name = name };

        // Assert
        input.Name.Should().Be(name);
    }

    [Fact]
    public void CreateFamilyInput_Name_ShouldBeMutable()
    {
        // Arrange
        var input = new CreateFamilyInput { Name = "Original Name" };
        var newName = "Updated Name";

        // Act
        input.Name = newName;

        // Assert
        input.Name.Should().Be(newName);
    }
}