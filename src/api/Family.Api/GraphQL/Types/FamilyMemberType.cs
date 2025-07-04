using Family.Api.Features.Families.DTOs;
using HotChocolate.Types;

namespace Family.Api.GraphQL.Types;

public class FamilyMemberType : ObjectType<FamilyMemberDto>
{
    protected override void Configure(IObjectTypeDescriptor<FamilyMemberDto> descriptor)
    {
        descriptor.Name("FamilyMember");
        descriptor.Description("Represents a member of a family");
        
        descriptor
            .Field(f => f.UserId)
            .Type<NonNullType<IdType>>()
            .Description("The user ID of the family member");

        descriptor
            .Field(f => f.Role)
            .Type<NonNullType<StringType>>()
            .Description("The role of the family member (FamilyUser, FamilyAdmin)");

        descriptor
            .Field(f => f.JoinedAt)
            .Type<NonNullType<DateTimeType>>()
            .Description("The date and time when the user joined the family");
    }
}