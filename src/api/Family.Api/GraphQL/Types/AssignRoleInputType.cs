using Family.Api.GraphQL.Mutations;
using HotChocolate.Types;

namespace Family.Api.GraphQL.Types;

public class AssignRoleInputType : InputObjectType<AssignRoleInput>
{
    protected override void Configure(IInputObjectTypeDescriptor<AssignRoleInput> descriptor)
    {
        descriptor.Name("AssignRoleInput");
        descriptor.Description("Input for assigning or revoking family roles");

        descriptor
            .Field(f => f.FamilyId)
            .Type<NonNullType<StringType>>()
            .Description("The ID of the family");

        descriptor
            .Field(f => f.UserId)
            .Type<NonNullType<IdType>>()
            .Description("The ID of the user to assign/revoke role");
    }
}