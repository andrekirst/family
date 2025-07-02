using Family.Api.GraphQL.Types;
using MediatR;

namespace Family.Api.Features.Authentication.Queries;

public record InitiateLoginQuery : IRequest<LoginInitiationPayload>;