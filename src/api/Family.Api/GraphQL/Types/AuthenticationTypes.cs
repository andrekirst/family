namespace Family.Api.GraphQL.Types;

public record LoginInput(string Email, string Password);

public record LoginInitiationPayload(string LoginUrl, string State);

public record LoginCallbackInput(string AuthorizationCode, string State);

public record LoginPayload(
    string? AccessToken,
    string? RefreshToken,
    User? User,
    IReadOnlyList<string>? Errors);

public record LogoutPayload(bool Success, IReadOnlyList<string>? Errors);

public record RefreshTokenInput(string RefreshToken);

public record RefreshTokenPayload(
    string? AccessToken,
    string? RefreshToken,
    IReadOnlyList<string>? Errors);

public class LoginInputType : InputObjectType<LoginInput>
{
    protected override void Configure(IInputObjectTypeDescriptor<LoginInput> descriptor)
    {
        descriptor.Description("Input for direct login with email and password");
        
        descriptor.Field(f => f.Email)
            .Description("User email address");
            
        descriptor.Field(f => f.Password)
            .Description("User password");
    }
}

public class LoginCallbackInputType : InputObjectType<LoginCallbackInput>
{
    protected override void Configure(IInputObjectTypeDescriptor<LoginCallbackInput> descriptor)
    {
        descriptor.Description("Input for OAuth callback completion");
        
        descriptor.Field(f => f.AuthorizationCode)
            .Description("Authorization code from OAuth provider");
            
        descriptor.Field(f => f.State)
            .Description("State parameter for security validation");
    }
}

public class RefreshTokenInputType : InputObjectType<RefreshTokenInput>
{
    protected override void Configure(IInputObjectTypeDescriptor<RefreshTokenInput> descriptor)
    {
        descriptor.Description("Input for token refresh");
        
        descriptor.Field(f => f.RefreshToken)
            .Description("Refresh token to exchange for new access token");
    }
}

public class LoginInitiationPayloadType : ObjectType<LoginInitiationPayload>
{
    protected override void Configure(IObjectTypeDescriptor<LoginInitiationPayload> descriptor)
    {
        descriptor.Description("Response for login initiation");
        
        descriptor.Field(f => f.LoginUrl)
            .Description("URL to redirect user for authentication");
            
        descriptor.Field(f => f.State)
            .Description("State parameter for security validation");
    }
}

public class LoginPayloadType : ObjectType<LoginPayload>
{
    protected override void Configure(IObjectTypeDescriptor<LoginPayload> descriptor)
    {
        descriptor.Description("Response for login completion");
        
        descriptor.Field(f => f.AccessToken)
            .Description("JWT access token for API authentication");
            
        descriptor.Field(f => f.RefreshToken)
            .Description("Refresh token for token renewal");
            
        descriptor.Field(f => f.User)
            .Description("Authenticated user information");
            
        descriptor.Field(f => f.Errors)
            .Description("List of validation or authentication errors");
    }
}

public class LogoutPayloadType : ObjectType<LogoutPayload>
{
    protected override void Configure(IObjectTypeDescriptor<LogoutPayload> descriptor)
    {
        descriptor.Description("Response for logout operation");
        
        descriptor.Field(f => f.Success)
            .Description("Indicates whether logout was successful");
            
        descriptor.Field(f => f.Errors)
            .Description("List of errors that occurred during logout");
    }
}

public class RefreshTokenPayloadType : ObjectType<RefreshTokenPayload>
{
    protected override void Configure(IObjectTypeDescriptor<RefreshTokenPayload> descriptor)
    {
        descriptor.Description("Response for token refresh");
        
        descriptor.Field(f => f.AccessToken)
            .Description("New JWT access token");
            
        descriptor.Field(f => f.RefreshToken)
            .Description("New refresh token");
            
        descriptor.Field(f => f.Errors)
            .Description("List of errors that occurred during token refresh");
    }
}