using AutoFixture;
using Family.Api.Features.Families.IFamilyRepository;
using Family.Api.Features.Families.Queries;
using FluentAssertions;
using NSubstitute;

namespace Family.Api.Tests.Features.Families.Queries;

public class GetFamilyByIdQueryTests
{
    private readonly Fixture _fixture = new();
    private readonly IFamilyRepository _familyRepository = Substitute.For<IFamilyRepository>();
    private readonly GetFamilyByIdQueryHandler _handler;

    public GetFamilyByIdQueryTests()
    {
        _handler = new GetFamilyByIdQueryHandler(_familyRepository);
    }

    [Fact]
    public async Task Handle_WithExistingFamilyId_ShouldReturnFamily()
    {
        // Arrange
        var familyId = _fixture.Create<Guid>();
        var query = new GetFamilyByIdQuery(familyId.ToString());
        
        var family = Api.Features.Families.Models.Family.Create(_fixture.Create<string>(), _fixture.Create<Guid>());
        _familyRepository.GetByIdAsync(familyId, Arg.Any<CancellationToken>())
            .Returns(family);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().Be(family);
        await _familyRepository.Received(1).GetByIdAsync(familyId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNonExistingFamilyId_ShouldReturnNull()
    {
        // Arrange
        var familyId = _fixture.Create<Guid>();
        var query = new GetFamilyByIdQuery(familyId.ToString());
        
        _familyRepository.GetByIdAsync(familyId, Arg.Any<CancellationToken>())
            .Returns((Api.Features.Families.Models.Family?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
        await _familyRepository.Received(1).GetByIdAsync(familyId, Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Handle_WithInvalidFamilyId_ShouldThrowArgumentException(string invalidFamilyId)
    {
        // Arrange
        var query = new GetFamilyByIdQuery(invalidFamilyId);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(async () => 
            await _handler.Handle(query, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WithInvalidGuidFormat_ShouldThrowFormatException()
    {
        // Arrange
        var query = new GetFamilyByIdQuery("invalid-guid-format");

        // Act & Assert
        await Assert.ThrowsAsync<FormatException>(async () => 
            await _handler.Handle(query, CancellationToken.None));
    }

    [Fact]
    public void GetFamilyByIdQuery_WithValidFamilyId_ShouldCreateQuery()
    {
        // Arrange
        var familyId = _fixture.Create<Guid>().ToString();

        // Act
        var query = new GetFamilyByIdQuery(familyId);

        // Assert
        query.FamilyId.Should().Be(familyId);
    }

    [Fact]
    public async Task Handle_WithRepositoryException_ShouldPropagateException()
    {
        // Arrange
        var familyId = _fixture.Create<Guid>();
        var query = new GetFamilyByIdQuery(familyId.ToString());
        
        _familyRepository.GetByIdAsync(familyId, Arg.Any<CancellationToken>())
            .Throws(new Exception("Database connection failed"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(async () => 
            await _handler.Handle(query, CancellationToken.None));
        
        exception.Message.Should().Be("Database connection failed");
    }
}