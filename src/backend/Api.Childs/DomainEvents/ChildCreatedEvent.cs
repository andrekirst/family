using Api.Childs.Features.Child.Commands.Create;
using AutoMapper;

namespace Api.Childs.DomainEvents;

public class ChildCreatedEvent
{
    public class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<CreateChildCommand, ChildCreatedEvent>();
        }
    }
}