using System.ComponentModel.DataAnnotations;
using WebUi.ValueObjects;

namespace WebUi.Data.Model;

public class FamilyMember : BaseEntity
{
    [Key]
    public int Id { get; set; }

    [MaxLength(Lengths.TextField)]
    public string? FirstName { get; set; }

    [MaxLength(Lengths.TextField)]
    public string? LastName { get; set; }

    [MaxLength(Lengths.TextField)]
    public DateTime? Birthdate { get; set; }
}