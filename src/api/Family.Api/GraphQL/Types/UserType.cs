using Family.Api.Models;

namespace Family.Api.GraphQL.Types;

public class UserType : ObjectType<User>
{
    protected override void Configure(IObjectTypeDescriptor<User> descriptor)
    {
        descriptor.Description("Represents a user in the family application");

        descriptor.Field(u => u.Id)
            .Description("The unique identifier of the user");

        descriptor.Field(u => u.Email)
            .Description("The email address of the user");

        descriptor.Field(u => u.FirstName)
            .Description("The first name of the user");

        descriptor.Field(u => u.LastName)
            .Description("The last name of the user");

        descriptor.Field(u => u.FullName)
            .Description("The full name of the user (computed)");

        descriptor.Field(u => u.PreferredLanguage)
            .Description("The preferred language of the user");

        descriptor.Field(u => u.IsActive)
            .Description("Indicates whether the user account is active");

        descriptor.Field(u => u.CreatedAt)
            .Description("The date and time when the user was created");

        descriptor.Field(u => u.UpdatedAt)
            .Description("The date and time when the user was last updated");

        descriptor.Field(u => u.LastLoginAt)
            .Description("The date and time of the user's last login");

        // Hide sensitive information
        descriptor.Ignore(u => u.KeycloakSubjectId);
        descriptor.Ignore(u => u.UserRoles);
    }
}