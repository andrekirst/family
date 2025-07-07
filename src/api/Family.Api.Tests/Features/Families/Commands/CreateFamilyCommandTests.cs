using AutoFixture;
using Family.Api.Features.Families.Commands;
using Family.Api.Features.Families.DTOs;
using Family.Api.Features.Families.IFamilyRepository;
using Family.Api.Features.Families.Models;
using FluentAssertions;
using FluentValidation;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Family.Api.Tests.Features.Families.Commands;

public class CreateFamilyCommandTests
{
    private readonly Fixture _fixture = new();
    private readonly IFamilyRepository _familyRepository = Substitute.For<IFamilyRepository>();
    private readonly IStringLocalizer<CreateFamilyCommandHandler> _localizer = Substitute.For<IStringLocalizer<CreateFamilyCommandHandler>>();
    private readonly ILogger<CreateFamilyCommandHandler> _logger = Substitute.For<ILogger<CreateFamilyCommandHandler>>();
    private readonly CreateFamilyCommandHandler _handler;
    private readonly CreateFamilyCommandValidator _validator;

    public CreateFamilyCommandTests()
    {
        _handler = new CreateFamilyCommandHandler(_familyRepository, _localizer, _logger);
        _validator = new CreateFamilyCommandValidator(_localizer);
        
        // Setup default localizer responses
        _localizer[Arg.Any<string>()].Returns(x => new LocalizedString(x.ArgAt<string>(0), x.ArgAt<string>(0)));
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateFamilyAndReturnSuccessResult()
    {
        // Arrange
        var command = new CreateFamilyCommand(
            Name: _fixture.Create<string>(),
            UserId: _fixture.Create<Guid>(),
            CorrelationId: _fixture.Create<string>()
        );

        _familyRepository.GetByOwnerIdAsync(command.UserId, Arg.Any<CancellationToken>())
            .Returns((Api.Features.Families.Models.Family?)null);

        _familyRepository.SaveAsync(Arg.Any<Api.Features.Families.Models.Family>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Family.Should().NotBeNull();
        result.Family!.Name.Should().Be(command.Name);
        result.Family.OwnerId.Should().Be(command.UserId);
        result.ErrorMessage.Should().BeNull();

        await _familyRepository.Received(1).SaveAsync(Arg.Any<Api.Features.Families.Models.Family>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithUserAlreadyHavingFamily_ShouldReturnFailureResult()
    {
        // Arrange
        var command = new CreateFamilyCommand(
            Name: _fixture.Create<string>(),
            UserId: _fixture.Create<Guid>(),
            CorrelationId: _fixture.Create<string>()
        );

        var existingFamily = Api.Features.Families.Models.Family.Create(_fixture.Create<string>(), command.UserId);
        _familyRepository.GetByOwnerIdAsync(command.UserId, Arg.Any<CancellationToken>())
            .Returns(existingFamily);

        var errorMessage = "User already has a family";
        _localizer["User already has a family"].Returns(new LocalizedString("User already has a family", errorMessage));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Family.Should().BeNull();
        result.ErrorMessage.Should().Be(errorMessage);

        await _familyRepository.DidNotReceive().SaveAsync(Arg.Any<Api.Features.Families.Models.Family>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithRepositoryException_ShouldReturnFailureResult()
    {
        // Arrange
        var command = new CreateFamilyCommand(
            Name: _fixture.Create<string>(),
            UserId: _fixture.Create<Guid>(),
            CorrelationId: _fixture.Create<string>()
        );

        _familyRepository.GetByOwnerIdAsync(command.UserId, Arg.Any<CancellationToken>())
            .Returns((Api.Features.Families.Models.Family?)null);

        _familyRepository.SaveAsync(Arg.Any<Api.Features.Families.Models.Family>(), Arg.Any<CancellationToken>())
            .Throws(new Exception("Database error"));

        var errorMessage = "An error occurred while creating the family";
        _localizer["An error occurred while creating the family"].Returns(new LocalizedString("An error occurred while creating the family", errorMessage));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Family.Should().BeNull();
        result.ErrorMessage.Should().Be(errorMessage);
    }

    [Fact]
    public void CreateFamilyCommand_WithValidParameters_ShouldCreateCommand()
    {
        // Arrange
        var name = _fixture.Create<string>();
        var userId = _fixture.Create<Guid>();
        var correlationId = _fixture.Create<string>();

        // Act
        var command = new CreateFamilyCommand(name, userId, correlationId);

        // Assert
        command.Name.Should().Be(name);
        command.UserId.Should().Be(userId);
        command.CorrelationId.Should().Be(correlationId);
    }

    [Fact]
    public void CreateFamilyResult_WithSuccessfulFamily_ShouldCreateSuccessResult()
    {
        // Arrange
        var family = Api.Features.Families.Models.Family.Create(_fixture.Create<string>(), _fixture.Create<Guid>());
        var familyDto = FamilyDto.FromDomain(family);

        // Act
        var result = CreateFamilyResult.Success(familyDto);

        // Assert
        result.Success.Should().BeTrue();
        result.Family.Should().Be(familyDto);
        result.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public void CreateFamilyResult_WithFailure_ShouldCreateFailureResult()
    {
        // Arrange
        var errorMessage = _fixture.Create<string>();

        // Act
        var result = CreateFamilyResult.Failure(errorMessage);

        // Assert
        result.Success.Should().BeFalse();
        result.Family.Should().BeNull();
        result.ErrorMessage.Should().Be(errorMessage);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validator_WithInvalidName_ShouldHaveValidationError(string invalidName)
    {
        // Arrange
        var command = new CreateFamilyCommand(
            Name: invalidName,
            UserId: _fixture.Create<Guid>(),
            CorrelationId: _fixture.Create<string>()
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateFamilyCommand.Name));
    }

    [Fact]
    public void Validator_WithNameTooLong_ShouldHaveValidationError()
    {
        // Arrange
        var longName = new string('a', 101); // Max is 100
        var command = new CreateFamilyCommand(
            Name: longName,
            UserId: _fixture.Create<Guid>(),
            CorrelationId: _fixture.Create<string>()
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateFamilyCommand.Name));
    }

    [Fact]
    public void Validator_WithEmptyUserId_ShouldHaveValidationError()
    {
        // Arrange
        var command = new CreateFamilyCommand(
            Name: _fixture.Create<string>(),
            UserId: Guid.Empty,
            CorrelationId: _fixture.Create<string>()
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateFamilyCommand.UserId));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validator_WithInvalidCorrelationId_ShouldHaveValidationError(string invalidCorrelationId)
    {
        // Arrange
        var command = new CreateFamilyCommand(
            Name: _fixture.Create<string>(),
            UserId: _fixture.Create<Guid>(),
            CorrelationId: invalidCorrelationId
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateFamilyCommand.CorrelationId));
    }

    [Fact]
    public void Validator_WithValidCommand_ShouldBeValid()
    {
        // Arrange
        var command = new CreateFamilyCommand(
            Name: _fixture.Create<string>(),
            UserId: _fixture.Create<Guid>(),
            CorrelationId: _fixture.Create<string>()
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }
}