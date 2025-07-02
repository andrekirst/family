using Family.Api.GraphQL.Types;
using MediatR;

namespace Family.Api.Features.Authentication.Commands;

public record DirectLoginCommand(string Email, string Password) : IRequest<LoginPayload>;