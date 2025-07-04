using Family.Api.GraphQL.Queries;
using HotChocolate.Types;

namespace Family.Api.GraphQL.Types;

public class FamilyMemberRoleType : ObjectType<FamilyMemberRole>
{
    protected override void Configure(IObjectTypeDescriptor<FamilyMemberRole> descriptor)
    {
        descriptor.Name("FamilyMemberRole");
        descriptor.Description("Represents a family member with their role information");

        descriptor
            .Field(f => f.UserId)
            .Type<NonNullType<IdType>>()
            .Description("The unique identifier of the user");

        descriptor
            .Field(f => f.UserEmail)
            .Type<NonNullType<StringType>>()
            .Description("The email address of the user");

        descriptor
            .Field(f => f.UserName)
            .Type<NonNullType<StringType>>()
            .Description("The full name of the user");

        descriptor
            .Field(f => f.Role)
            .Type<NonNullType<StringType>>()
            .Description("The role of the user in the family (FamilyUser, FamilyAdmin)");

        descriptor
            .Field(f => f.JoinedAt)
            .Type<NonNullType<DateTimeType>>()
            .Description("The date and time when the user joined the family");

        descriptor
            .Field(f => f.IsOwner)
            .Type<NonNullType<BooleanType>>()
            .Description("Indicates if the user is the owner of the family");
    }
}