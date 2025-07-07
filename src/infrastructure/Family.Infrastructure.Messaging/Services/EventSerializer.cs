using System.Reflection;
using Family.Infrastructure.EventSourcing.Models;
using Family.Infrastructure.Messaging.Abstractions;

namespace Family.Infrastructure.Messaging.Services;

public class EventSerializer : IEventSerializer
{
    private static readonly Dictionary<string, Type> EventTypeCache = new();
    private static readonly Dictionary<Type, string> EventNameCache = new();
    private readonly JsonSerializerOptions _jsonOptions;

    public EventSerializer()
    {
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            PropertyNameCaseInsensitive = true
        };
        
        InitializeEventTypeCache();
    }

    private static void InitializeEventTypeCache()
    {
        var eventTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => type.IsSubclassOf(typeof(DomainEvent)) && !type.IsAbstract)
            .ToList();

        foreach (var eventType in eventTypes)
        {
            var eventName = eventType.Name;
            EventTypeCache[eventName] = eventType;
            EventNameCache[eventType] = eventName;
        }
    }

    public string Serialize<T>(T @event) where T : DomainEvent
    {
        if (@event == null)
            throw new ArgumentNullException(nameof(@event));

        return JsonSerializer.Serialize(@event, _jsonOptions);
    }

    public T? Deserialize<T>(string data) where T : DomainEvent
    {
        if (string.IsNullOrWhiteSpace(data))
            return null;

        try
        {
            return JsonSerializer.Deserialize<T>(data, _jsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    public DomainEvent? Deserialize(string data, Type eventType)
    {
        if (string.IsNullOrWhiteSpace(data) || eventType == null)
            return null;

        if (!eventType.IsSubclassOf(typeof(DomainEvent)))
            throw new ArgumentException($"Type {eventType.Name} is not a DomainEvent", nameof(eventType));

        try
        {
            return JsonSerializer.Deserialize(data, eventType, _jsonOptions) as DomainEvent;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    public string GetEventTypeName<T>() where T : DomainEvent
    {
        var eventType = typeof(T);
        return EventNameCache.TryGetValue(eventType, out var eventName) 
            ? eventName 
            : eventType.Name;
    }

    public Type? GetEventTypeFromName(string eventTypeName)
    {
        return EventTypeCache.TryGetValue(eventTypeName, out var eventType) 
            ? eventType 
            : null;
    }
}