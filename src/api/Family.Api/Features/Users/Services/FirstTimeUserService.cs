using Family.Api.Features.Families;
using Family.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace Family.Api.Features.Users.Services;

public interface IFirstTimeUserService
{
    Task<bool> IsFirstTimeUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<FirstTimeUserInfo> GetFirstTimeUserInfoAsync(Guid userId, CancellationToken cancellationToken = default);
}

public record FirstTimeUserInfo(
    bool IsFirstTime,
    bool HasFamily,
    string? FamilyId = null,
    string? FamilyName = null);

public class FirstTimeUserService : IFirstTimeUserService
{
    private readonly FamilyDbContext _dbContext;
    private readonly IFamilyRepository _familyRepository;

    public FirstTimeUserService(FamilyDbContext dbContext, IFamilyRepository familyRepository)
    {
        _dbContext = dbContext;
        _familyRepository = familyRepository;
    }

    public async Task<bool> IsFirstTimeUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        // Check if user exists in the database
        var userExists = await _dbContext.Users
            .AnyAsync(u => u.Id == userId && u.IsActive, cancellationToken);

        if (!userExists)
        {
            return true; // User doesn't exist in database, definitely first time
        }

        // Check if user is member of any family
        var hasFamilyMembership = await _dbContext.FamilyMembers
            .AnyAsync(fm => fm.UserId == userId, cancellationToken);

        return !hasFamilyMembership; // First time if no family membership
    }

    public async Task<FirstTimeUserInfo> GetFirstTimeUserInfoAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        // Check if user exists in the database
        var userExists = await _dbContext.Users
            .AnyAsync(u => u.Id == userId && u.IsActive, cancellationToken);

        if (!userExists)
        {
            return new FirstTimeUserInfo(
                IsFirstTime: true,
                HasFamily: false
            );
        }

        // Check if user is member of any family
        var familyMember = await _dbContext.FamilyMembers
            .Include(fm => fm.Family)
            .FirstOrDefaultAsync(fm => fm.UserId == userId, cancellationToken);

        if (familyMember?.Family == null)
        {
            return new FirstTimeUserInfo(
                IsFirstTime: true,
                HasFamily: false
            );
        }

        return new FirstTimeUserInfo(
            IsFirstTime: false,
            HasFamily: true,
            FamilyId: familyMember.FamilyId,
            FamilyName: familyMember.Family.Name
        );
    }
}