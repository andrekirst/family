using Api.Infrastructure;

namespace Api.Domain.Core.Authentication.Google;

public class GoogleAccount : BaseEntity
{
    public string GoogleId { get; set; } = default!;
    public string? AccessToken { get; set; }
    public string UserId { get; set; } = default!;
}