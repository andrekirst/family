using Api.Childs.Infrastructure;

namespace Api.Childs.Features.Child.Commands.Create;

public class CreateChildCommand : Command<Guid?>
{
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public DateTime? BirthDate { get; set; }
    public string? CountryAlpha3Code { get; set; }
    public string? CityOfBirth { get; set; }
}
