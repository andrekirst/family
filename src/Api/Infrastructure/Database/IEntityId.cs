namespace Api.Infrastructure.Database;

public interface IEntityId
{
    Guid Id { get; set; }
}