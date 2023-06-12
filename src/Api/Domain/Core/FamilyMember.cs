using Api.Infrastructure;

namespace Api.Domain.Core;

public class FamilyMember : BaseEntity
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateTime? Birthdate { get; set; }
}