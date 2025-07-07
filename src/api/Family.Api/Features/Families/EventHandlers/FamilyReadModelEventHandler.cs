using Family.Api.Data;
using Family.Api.Data.Entities;
using Family.Api.Features.Families.DomainEvents;
using Family.Api.Features.Families.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Family.Api.Features.Families.EventHandlers;

public class FamilyReadModelEventHandler : 
    INotificationHandler<FamilyCreated>,
    INotificationHandler<FamilyMemberAdded>,
    INotificationHandler<FamilyAdminAssigned>
{
    private readonly FamilyDbContext _dbContext;

    public FamilyReadModelEventHandler(FamilyDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(FamilyCreated notification, CancellationToken cancellationToken)
    {
        // Create new family read model
        var familyEntity = new FamilyEntity
        {
            Id = notification.AggregateId,
            Name = notification.Name,
            OwnerId = notification.OwnerId,
            CreatedAt = notification.Timestamp,
            UpdatedAt = notification.Timestamp
        };

        _dbContext.Families.Add(familyEntity);

        // Add the owner as admin member
        var adminMember = new FamilyMemberEntity
        {
            FamilyId = notification.AggregateId,
            UserId = notification.OwnerId,
            Role = FamilyRole.FamilyAdmin.ToString(),
            JoinedAt = notification.Timestamp
        };

        _dbContext.FamilyMembers.Add(adminMember);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task Handle(FamilyMemberAdded notification, CancellationToken cancellationToken)
    {
        // Add new member to read model
        var memberEntity = new FamilyMemberEntity
        {
            FamilyId = notification.AggregateId,
            UserId = notification.MemberUserId,
            Role = notification.Role,
            JoinedAt = notification.JoinedAt
        };

        _dbContext.FamilyMembers.Add(memberEntity);

        // Update family UpdatedAt timestamp
        var family = await _dbContext.Families
            .FirstOrDefaultAsync(f => f.Id == notification.AggregateId, cancellationToken);
        if (family != null)
        {
            family.UpdatedAt = notification.Timestamp;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task Handle(FamilyAdminAssigned notification, CancellationToken cancellationToken)
    {
        // Update member role to admin
        var memberEntity = await _dbContext.FamilyMembers
            .FirstOrDefaultAsync(fm => fm.FamilyId == notification.AggregateId && 
                                     fm.UserId.ToString() == notification.UserId, cancellationToken);

        if (memberEntity != null)
        {
            memberEntity.Role = FamilyRole.FamilyAdmin.ToString();
        }

        // Update family UpdatedAt timestamp
        var family = await _dbContext.Families
            .FirstOrDefaultAsync(f => f.Id == notification.AggregateId, cancellationToken);
        if (family != null)
        {
            family.UpdatedAt = notification.Timestamp;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}