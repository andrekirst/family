using Family.Api.GraphQL.Types;
using MediatR;

namespace Family.Api.Features.Authentication.Commands;

public record RefreshTokenCommand(string RefreshToken) : IRequest<RefreshTokenPayload>;