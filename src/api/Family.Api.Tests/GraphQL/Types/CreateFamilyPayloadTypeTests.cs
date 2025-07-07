using AutoFixture;
using Family.Api.Features.Families.DTOs;
using Family.Api.GraphQL.Types;
using FluentAssertions;
using HotChocolate.Types;
using Microsoft.Extensions.DependencyInjection;

namespace Family.Api.Tests.GraphQL.Types;

public class CreateFamilyPayloadTypeTests
{
    private readonly Fixture _fixture = new();

    [Fact]
    public void CreateFamilyPayload_WithSuccessfulCreation_ShouldCreateCorrectPayload()
    {
        // Arrange
        var family = _fixture.Create<FamilyDto>();

        // Act
        var payload = new CreateFamilyPayload
        {
            Success = true,
            Family = family,
            ErrorMessage = null,
            ValidationErrors = null
        };

        // Assert
        payload.Success.Should().BeTrue();
        payload.Family.Should().Be(family);
        payload.ErrorMessage.Should().BeNull();
        payload.ValidationErrors.Should().BeNull();
    }

    [Fact]
    public void CreateFamilyPayload_WithError_ShouldCreateCorrectPayload()
    {
        // Arrange
        var errorMessage = _fixture.Create<string>();

        // Act
        var payload = new CreateFamilyPayload
        {
            Success = false,
            Family = null,
            ErrorMessage = errorMessage,
            ValidationErrors = null
        };

        // Assert
        payload.Success.Should().BeFalse();
        payload.Family.Should().BeNull();
        payload.ErrorMessage.Should().Be(errorMessage);
        payload.ValidationErrors.Should().BeNull();
    }

    [Fact]
    public void CreateFamilyPayload_WithValidationErrors_ShouldCreateCorrectPayload()
    {
        // Arrange
        var validationErrors = _fixture.CreateMany<string>(3).ToList();

        // Act
        var payload = new CreateFamilyPayload
        {
            Success = false,
            Family = null,
            ErrorMessage = null,
            ValidationErrors = validationErrors
        };

        // Assert
        payload.Success.Should().BeFalse();
        payload.Family.Should().BeNull();
        payload.ErrorMessage.Should().BeNull();
        payload.ValidationErrors.Should().BeEquivalentTo(validationErrors);
    }

    [Fact]
    public void CreateFamilyPayload_DefaultConstructor_ShouldInitializeWithDefaults()
    {
        // Act
        var payload = new CreateFamilyPayload();

        // Assert
        payload.Success.Should().BeFalse();
        payload.Family.Should().BeNull();
        payload.ErrorMessage.Should().BeNull();
        payload.ValidationErrors.Should().BeNull();
    }

    [Fact]
    public async Task CreateFamilyPayloadType_Schema_ShouldHaveCorrectConfiguration()
    {
        // Arrange
        var schema = await new ServiceCollection()
            .AddGraphQLServer()
            .AddType<CreateFamilyPayloadType>()
            .AddType<FamilyType>()
            .BuildSchemaAsync();

        // Act
        var payloadType = schema.GetType<CreateFamilyPayloadType>("CreateFamilyPayload");

        // Assert
        payloadType.Should().NotBeNull();
        payloadType.Name.Should().Be("CreateFamilyPayload");
        payloadType.Description.Should().Be("Payload for family creation result");
    }

    [Fact]
    public async Task CreateFamilyPayloadType_SuccessField_ShouldBeNonNull()
    {
        // Arrange
        var schema = await new ServiceCollection()
            .AddGraphQLServer()
            .AddType<CreateFamilyPayloadType>()
            .AddType<FamilyType>()
            .BuildSchemaAsync();

        // Act
        var payloadType = schema.GetType<CreateFamilyPayloadType>("CreateFamilyPayload");
        var successField = payloadType.Fields["success"];

        // Assert
        successField.Should().NotBeNull();
        successField.Type.IsNonNullType().Should().BeTrue();
        successField.Description.Should().Be("Indicates if the family creation was successful");
    }

    [Fact]
    public async Task CreateFamilyPayloadType_FamilyField_ShouldBeNullable()
    {
        // Arrange
        var schema = await new ServiceCollection()
            .AddGraphQLServer()
            .AddType<CreateFamilyPayloadType>()
            .AddType<FamilyType>()
            .BuildSchemaAsync();

        // Act
        var payloadType = schema.GetType<CreateFamilyPayloadType>("CreateFamilyPayload");
        var familyField = payloadType.Fields["family"];

        // Assert
        familyField.Should().NotBeNull();
        familyField.Type.IsNonNullType().Should().BeFalse();
        familyField.Description.Should().Be("The created family, if successful");
    }

    [Fact]
    public async Task CreateFamilyPayloadType_ErrorMessageField_ShouldBeNullable()
    {
        // Arrange
        var schema = await new ServiceCollection()
            .AddGraphQLServer()
            .AddType<CreateFamilyPayloadType>()
            .AddType<FamilyType>()
            .BuildSchemaAsync();

        // Act
        var payloadType = schema.GetType<CreateFamilyPayloadType>("CreateFamilyPayload");
        var errorMessageField = payloadType.Fields["errorMessage"];

        // Assert
        errorMessageField.Should().NotBeNull();
        errorMessageField.Type.IsNonNullType().Should().BeFalse();
        errorMessageField.Description.Should().Be("Error message if creation failed");
    }

    [Fact]
    public async Task CreateFamilyPayloadType_ValidationErrorsField_ShouldBeNullableList()
    {
        // Arrange
        var schema = await new ServiceCollection()
            .AddGraphQLServer()
            .AddType<CreateFamilyPayloadType>()
            .AddType<FamilyType>()
            .BuildSchemaAsync();

        // Act
        var payloadType = schema.GetType<CreateFamilyPayloadType>("CreateFamilyPayload");
        var validationErrorsField = payloadType.Fields["validationErrors"];

        // Assert
        validationErrorsField.Should().NotBeNull();
        validationErrorsField.Type.IsNonNullType().Should().BeFalse();
        validationErrorsField.Type.IsListType().Should().BeTrue();
        validationErrorsField.Description.Should().Be("List of validation errors if creation failed");
    }

    [Fact]
    public void CreateFamilyPayload_WithMultipleErrorTypes_ShouldAllowCombination()
    {
        // Arrange
        var errorMessage = _fixture.Create<string>();
        var validationErrors = _fixture.CreateMany<string>(2).ToList();

        // Act
        var payload = new CreateFamilyPayload
        {
            Success = false,
            Family = null,
            ErrorMessage = errorMessage,
            ValidationErrors = validationErrors
        };

        // Assert
        payload.Success.Should().BeFalse();
        payload.Family.Should().BeNull();
        payload.ErrorMessage.Should().Be(errorMessage);
        payload.ValidationErrors.Should().BeEquivalentTo(validationErrors);
    }
}