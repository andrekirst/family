using AutoMapper;

namespace Api.Childs.Features.Child.Commands.Create;

public class Mappings : Profile
{
    public Mappings()
    {
        CreateMap<CreateChildRequest, CreateChildCommand>();
    }
}