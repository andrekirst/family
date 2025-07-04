using Family.Api.Features.Users.Services;
using HotChocolate.Types;

namespace Family.Api.GraphQL.Types;

public class FirstTimeUserInfoType : ObjectType<FirstTimeUserInfo>
{
    protected override void Configure(IObjectTypeDescriptor<FirstTimeUserInfo> descriptor)
    {
        descriptor.Name("FirstTimeUserInfo");
        descriptor.Description("Information about whether a user is using the application for the first time");

        descriptor
            .Field(f => f.IsFirstTime)
            .Type<NonNullType<BooleanType>>()
            .Description("Indicates if this is the user's first time using the application");

        descriptor
            .Field(f => f.HasFamily)
            .Type<NonNullType<BooleanType>>()
            .Description("Indicates if the user already belongs to a family");

        descriptor
            .Field(f => f.FamilyId)
            .Type<StringType>()
            .Description("The ID of the user's family, if they belong to one");

        descriptor
            .Field(f => f.FamilyName)
            .Type<StringType>()
            .Description("The name of the user's family, if they belong to one");
    }
}