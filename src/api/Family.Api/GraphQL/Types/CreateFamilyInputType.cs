using HotChocolate.Types;

namespace Family.Api.GraphQL.Types;

public class CreateFamilyInputType : InputObjectType<CreateFamilyInput>
{
    protected override void Configure(IInputObjectTypeDescriptor<CreateFamilyInput> descriptor)
    {
        descriptor.Name("CreateFamilyInput");
        descriptor.Description("Input for creating a new family");

        descriptor
            .Field(f => f.Name)
            .Type<NonNullType<StringType>>()
            .Description("The name of the family");
    }
}

public class CreateFamilyInput
{
    public string Name { get; set; } = string.Empty;
}