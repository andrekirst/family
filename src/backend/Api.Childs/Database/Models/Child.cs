using System.ComponentModel.DataAnnotations;
using Api.Childs.Infrastructure;

namespace Api.Childs.Database.Models;

public class Child : Entity, IHasEntityKey
{
    public Guid Id { get; set; }

    [Required, MaxLength(Lengths.Child.FirstName.MaximumLength)]
    public string FirstName { get; set; } = default!;

    [Required, MaxLength(Lengths.Child.LastName.MaximumLength)]
    public string LastName { get; set; } = default!;
    
    public DateTime? BirthDate { get; set; }

    [MaxLength(Lengths.Child.CountryAlpha3Code.Length)]
    public string? CountryAlpha3Code { get; set; }

    [MaxLength(Lengths.Child.City.MaximumLength)]
    public string? CityOfBirth { get; set; }
    public string Key => Id.ToString("N");
    public string KeyName => "Child";
}