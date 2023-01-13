using Api.Childs.Database.Repositories;
using Api.Childs.Features.Child.Commands.Create;
using Api.Childs.Infrastructure;
using AutoFixture.Xunit2;
using AutoMapper;
using Moq;

namespace Api.Childs.Tests.Features.Child;

public class CreateChildCommandHandlerTests
{
    [Theory]
    [AutoMoqData]
    public async Task ExpectCallMapping(
        [Frozen] Mock<IMapper> mapper,
        CreateChildCommandHandler sut)
    {
        await sut.Handle(It.IsAny<CreateChildCommand>(), It.IsAny<CancellationToken>());

        mapper.Verify(v => v.Map<Database.Models.Child>(It.IsAny<CreateChildCommand>()));
    }

    [Theory]
    [AutoMoqData]
    public async Task ExpectCallRepositoryAdd(
        [Frozen] Mock<IChildRepository> childRepository,
        CreateChildCommandHandler sut)
    {
        await sut.Handle(It.IsAny<CreateChildCommand>(), It.IsAny<CancellationToken>());

        childRepository.Verify(v => v.Add(It.IsAny<Database.Models.Child>()));
    }

    [Theory]
    [AutoMoqData]
    public async Task ExpectCallUnitOfWorkSaveChangesAsync(
        [Frozen] Mock<IUnitOfWork> unitOfWork,
        CreateChildCommandHandler sut)
    {
        await sut.Handle(It.IsAny<CreateChildCommand>(), It.IsAny<CancellationToken>());

        unitOfWork.Verify(v => v.SaveChangesAsync(It.IsAny<CancellationToken>()));
    }
}