using System.ComponentModel.DataAnnotations;
using FamilyDomain = Family.Api.Features.Families.Models.Family;
using FamilyMemberDomain = Family.Api.Features.Families.Models.FamilyMember;

namespace Family.Api.Data.Entities;

public class FamilyEntity
{
    [Key]
    [StringLength(255)]
    public string Id { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    public Guid OwnerId { get; set; }
    
    [Required]
    public DateTime CreatedAt { get; set; }
    
    [Required]
    public DateTime UpdatedAt { get; set; }
    
    // Navigation property
    public List<FamilyMemberEntity> Members { get; set; } = new();
    
    // Factory method to create from domain model
    public static FamilyEntity FromDomain(FamilyDomain family)
    {
        return new FamilyEntity
        {
            Id = family.Id,
            Name = family.Name,
            OwnerId = family.OwnerId,
            CreatedAt = family.CreatedAt,
            UpdatedAt = family.UpdatedAt,
            Members = family.Members.Select(FamilyMemberEntity.FromDomain).ToList()
        };
    }
    
    // Method to convert to domain model
    public FamilyDomain ToDomain()
    {
        var family = new FamilyDomain();
        
        // Use reflection to set private fields for Event Sourcing compatibility
        var idField = typeof(FamilyDomain).GetField("<Id>k__BackingField", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        idField?.SetValue(family, Id);
        
        var nameField = typeof(FamilyDomain).GetField("<Name>k__BackingField", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        nameField?.SetValue(family, Name);
        
        var ownerIdField = typeof(FamilyDomain).GetField("<OwnerId>k__BackingField", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        ownerIdField?.SetValue(family, OwnerId);
        
        var createdAtField = typeof(FamilyDomain).GetField("<CreatedAt>k__BackingField", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        createdAtField?.SetValue(family, CreatedAt);
        
        var updatedAtField = typeof(FamilyDomain).GetField("<UpdatedAt>k__BackingField", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        updatedAtField?.SetValue(family, UpdatedAt);
        
        return family;
    }
}