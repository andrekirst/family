using Api.Childs.Database.Repositories;
using Api.Childs.DomainEvents;
using Api.Childs.Infrastructure;
using AutoMapper;
using MediatR;

namespace Api.Childs.Features.Child.Commands.Create;

public class CreateChildCommandHandler : IRequestHandler<CreateChildCommand, Guid?>
{
    private readonly IChildRepository _childRepository;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMessagingService _messagingService;

    public CreateChildCommandHandler(
        IChildRepository childRepository,
        IMapper mapper,
        IUnitOfWork unitOfWork,
        IMessagingService messagingService)
    {
        _childRepository = childRepository;
        _mapper = mapper;
        _unitOfWork = unitOfWork;
        _messagingService = messagingService;
    }

    public async Task<Guid?> Handle(CreateChildCommand request, CancellationToken cancellationToken)
    {
        var entity = await CreateEntity(request, cancellationToken);
        await SendEvent(request, cancellationToken);

        return entity.Id;
    }

    private async Task SendEvent(CreateChildCommand request, CancellationToken cancellationToken)
    {
        var @event = _mapper.Map<ChildCreatedEvent>(request);
        await _messagingService.SendEventAsync(@event, cancellationToken);
    }

    private async Task<Database.Models.Child> CreateEntity(CreateChildCommand request, CancellationToken cancellationToken)
    {
        var entity = _mapper.Map<Database.Models.Child>(request);
        _childRepository.Add(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return entity;
    }
}