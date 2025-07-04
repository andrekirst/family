using Family.Api.Features.Families.DomainEvents;
using Family.Infrastructure.EventSourcing.Models;
using System.ComponentModel.DataAnnotations;

namespace Family.Api.Features.Families.Models;

public class Family : AggregateRoot
{
    private readonly List<FamilyMember> _members = new();
    
    [Required]
    [StringLength(100)]
    public string Name { get; private set; } = string.Empty;
    
    [Required]
    public Guid OwnerId { get; private set; }
    
    public IReadOnlyList<FamilyMember> Members => _members.AsReadOnly();
    
    public Family()
    {
        // Required for Event Sourcing reconstruction
    }

    public static Family Create(string name, Guid ownerId, string userId, string correlationId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Family name cannot be empty", nameof(name));
        
        if (name.Length > 100)
            throw new ArgumentException("Family name cannot exceed 100 characters", nameof(name));
        
        if (ownerId == Guid.Empty)
            throw new ArgumentException("Owner ID cannot be empty", nameof(ownerId));

        var familyId = Guid.NewGuid().ToString();
        var family = new Family();
        
        // Create family
        var familyCreatedEvent = FamilyCreated.Create(
            familyId, name, ownerId, userId, correlationId);
        family.RaiseEvent(familyCreatedEvent);
        
        // Add owner as first member
        var memberAddedEvent = FamilyMemberAdded.Create(
            familyId, ownerId, FamilyRole.FamilyAdmin.ToString(), 2, userId, correlationId, familyCreatedEvent.EventId);
        family.RaiseEvent(memberAddedEvent);
        
        // Assign admin role
        var adminAssignedEvent = FamilyAdminAssigned.Create(
            familyId, ownerId, 3, userId, correlationId, memberAddedEvent.EventId);
        family.RaiseEvent(adminAssignedEvent);
        
        return family;
    }

    public void AddMember(Guid userId, FamilyRole role, string requestUserId, string correlationId)
    {
        if (_members.Any(m => m.UserId == userId))
            throw new InvalidOperationException($"User {userId} is already a member of this family");

        var memberAddedEvent = FamilyMemberAdded.Create(
            Id, userId, role.ToString(), Version + 1, requestUserId, correlationId);
        RaiseEvent(memberAddedEvent);
    }

    public bool IsAdmin(Guid userId)
    {
        return _members.Any(m => m.UserId == userId && m.Role == FamilyRole.FamilyAdmin);
    }

    public bool IsMember(Guid userId)
    {
        return _members.Any(m => m.UserId == userId);
    }

    protected override void ResetState()
    {
        Name = string.Empty;
        OwnerId = Guid.Empty;
        _members.Clear();
    }

    // Event Handlers for Event Sourcing
    public void Apply(FamilyCreated familyCreated)
    {
        Id = familyCreated.AggregateId;
        Name = familyCreated.Name;
        OwnerId = familyCreated.OwnerId;
        CreatedAt = familyCreated.Timestamp;
    }

    public void Apply(FamilyMemberAdded memberAdded)
    {
        var role = Enum.Parse<FamilyRole>(memberAdded.Role);
        var member = new FamilyMember(memberAdded.MemberUserId, role, memberAdded.JoinedAt);
        _members.Add(member);
    }

    public void Apply(FamilyAdminAssigned adminAssigned)
    {
        // Admin assignment is already handled by FamilyMemberAdded with FamilyAdmin role
        // This event can be used for additional admin-specific logic in the future
    }
}