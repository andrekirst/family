namespace Api.Infrastructure;

[AttributeUsage(AttributeTargets.Class)]
public class DomainEventAttribute(string name, int version = 1) : Attribute
{
    public string Name { get; init; } = name;
    public int Version { get; init; } = version;
}