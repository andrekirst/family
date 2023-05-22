using AutoMapper;
using WebUi.Data.Model;

namespace WebUi.Areas.Administration.FamilyMembers.Detail;

public class Mappings : Profile
{
    public Mappings()
    {
        CreateMap<ViewModel, ViewModel>();
        CreateProjection<FamilyMember, ViewModel>();
    }
}