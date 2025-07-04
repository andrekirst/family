using Family.Api.Features.Families.DTOs;
using HotChocolate.Types;

namespace Family.Api.GraphQL.Types;

public class FamilyType : ObjectType<FamilyDto>
{
    protected override void Configure(IObjectTypeDescriptor<FamilyDto> descriptor)
    {
        descriptor.Name("Family");
        descriptor.Description("Represents a family unit with members and organizational structure");
        
        descriptor
            .Field(f => f.Id)
            .Type<NonNullType<IdType>>()
            .Description("The unique identifier for the family");

        descriptor
            .Field(f => f.Name)
            .Type<NonNullType<StringType>>()
            .Description("The name of the family");

        descriptor
            .Field(f => f.OwnerId)
            .Type<NonNullType<IdType>>()
            .Description("The user ID of the family owner");

        descriptor
            .Field(f => f.Members)
            .Type<ListType<FamilyMemberType>>()
            .Description("List of family members");

        descriptor
            .Field(f => f.CreatedAt)
            .Type<NonNullType<DateTimeType>>()
            .Description("The date and time when the family was created");
    }
}