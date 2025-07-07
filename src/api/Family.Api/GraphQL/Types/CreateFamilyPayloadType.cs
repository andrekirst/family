using Family.Api.Features.Families.DTOs;
using HotChocolate.Types;

namespace Family.Api.GraphQL.Types;

public class CreateFamilyPayloadType : ObjectType<CreateFamilyPayload>
{
    protected override void Configure(IObjectTypeDescriptor<CreateFamilyPayload> descriptor)
    {
        descriptor.Name("CreateFamilyPayload");
        descriptor.Description("Payload for family creation result");

        descriptor
            .Field(f => f.Success)
            .Type<NonNullType<BooleanType>>()
            .Description("Indicates if the family creation was successful");

        descriptor
            .Field(f => f.Family)
            .Type<FamilyType>()
            .Description("The created family, if successful");

        descriptor
            .Field(f => f.ErrorMessage)
            .Type<StringType>()
            .Description("Error message if creation failed");

        descriptor
            .Field(f => f.ValidationErrors)
            .Type<ListType<StringType>>()
            .Description("List of validation errors if creation failed");
    }
}

public class CreateFamilyPayload
{
    public bool Success { get; set; }
    public FamilyDto? Family { get; set; }
    public string? ErrorMessage { get; set; }
    public List<string>? ValidationErrors { get; set; }
}