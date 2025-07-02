using Family.Api.GraphQL.Types;
using MediatR;

namespace Family.Api.Features.Authentication.Commands;

public record CompleteLoginCommand(string AuthorizationCode, string State) : IRequest<LoginPayload>;