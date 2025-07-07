using Family.Api.GraphQL.Mutations;
using HotChocolate.Types;

namespace Family.Api.GraphQL.Types;

public class AssignRoleResultType : ObjectType<AssignRoleResult>
{
    protected override void Configure(IObjectTypeDescriptor<AssignRoleResult> descriptor)
    {
        descriptor.Name("AssignRoleResult");
        descriptor.Description("Result of role assignment or revocation operation");

        descriptor
            .Field(f => f.Success)
            .Type<NonNullType<BooleanType>>()
            .Description("Indicates if the role operation was successful");

        descriptor
            .Field(f => f.Message)
            .Type<StringType>()
            .Description("Success message if operation completed successfully");

        descriptor
            .Field(f => f.ErrorMessage)
            .Type<StringType>()
            .Description("Error message if operation failed");
    }
}