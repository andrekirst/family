using Api.Domain.Core;
using Api.Infrastructure;

namespace Api.Domain.Calendar;

public class CalendarEntity : BaseEntity
{
    public string? Name { get; set; }
    public Guid FamilyMemberId { get; set; }

    public FamilyMemberEntity FamilyMemberEntity { get; set; } = default!;
}