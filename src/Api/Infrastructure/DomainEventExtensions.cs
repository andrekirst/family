namespace Api.Infrastructure;

public static class DomainEventExtensions
{
    public static DomainEventAttribute GetDomainEventAttribute(this IDomainEvent domainEvent)
    {
        return (DomainEventAttribute)Attribute.GetCustomAttribute(domainEvent.GetType(), typeof(DomainEventAttribute))!;
    }
}