using Api.Infrastructure;

namespace Api.Database.Authentication;

public class GoogleAccountEntity : BaseEntity
{
    public string GoogleId { get; set; } = default!;
    public string? AccessToken { get; set; }
    public string UserId { get; set; } = default!;
}