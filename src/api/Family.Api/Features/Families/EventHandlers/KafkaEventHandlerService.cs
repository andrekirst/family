using Family.Api.Features.Families.DomainEvents;
using Family.Infrastructure.Messaging.Abstractions;
using Family.Infrastructure.Messaging.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Family.Api.Features.Families.EventHandlers;

public class KafkaEventHandlerService : IHostedService
{
    private readonly IEventBusConsumer _eventBusConsumer;
    private readonly ILogger<KafkaEventHandlerService> _logger;
    private readonly KafkaConfiguration _kafkaConfig;

    public KafkaEventHandlerService(
        IEventBusConsumer eventBusConsumer,
        IOptions<KafkaConfiguration> kafkaOptions,
        ILogger<KafkaEventHandlerService> logger)
    {
        _eventBusConsumer = eventBusConsumer;
        _logger = logger;
        _kafkaConfig = kafkaOptions.Value;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting Kafka event handler service");

        // Subscribe to Family events
        _eventBusConsumer.Subscribe<FamilyCreated>(
            _kafkaConfig.Topics.FamilyEvents,
            HandleFamilyCreatedEvent);

        _eventBusConsumer.Subscribe<FamilyMemberAdded>(
            _kafkaConfig.Topics.FamilyEvents,
            HandleFamilyMemberAddedEvent);

        _eventBusConsumer.Subscribe<FamilyAdminAssigned>(
            _kafkaConfig.Topics.FamilyEvents,
            HandleFamilyAdminAssignedEvent);

        return _eventBusConsumer.StartAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping Kafka event handler service");
        return _eventBusConsumer.StopAsync(cancellationToken);
    }

    private async Task HandleFamilyCreatedEvent(FamilyCreated familyCreated, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling FamilyCreated event for family {FamilyId} with name {FamilyName}",
            familyCreated.AggregateId, familyCreated.Name);

        // Here you could:
        // - Send welcome emails
        // - Create initial family resources
        // - Trigger integration with external systems
        // - Update read models
        // - Generate analytics events

        await Task.Delay(100, cancellationToken); // Simulate async work
        
        _logger.LogDebug("Successfully processed FamilyCreated event for family {FamilyId}",
            familyCreated.AggregateId);
    }

    private async Task HandleFamilyMemberAddedEvent(FamilyMemberAdded familyMemberAdded, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling FamilyMemberAdded event for family {FamilyId}, user {UserId}",
            familyMemberAdded.AggregateId, familyMemberAdded.UserId);

        // Here you could:
        // - Send notification to family members
        // - Update member statistics
        // - Assign default permissions
        // - Create member profile

        await Task.Delay(100, cancellationToken); // Simulate async work
        
        _logger.LogDebug("Successfully processed FamilyMemberAdded event for family {FamilyId}",
            familyMemberAdded.AggregateId);
    }

    private async Task HandleFamilyAdminAssignedEvent(FamilyAdminAssigned familyAdminAssigned, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling FamilyAdminAssigned event for family {FamilyId}, admin {UserId}",
            familyAdminAssigned.AggregateId, familyAdminAssigned.UserId);

        // Here you could:
        // - Send admin welcome email
        // - Grant admin permissions
        // - Log audit trail
        // - Update analytics

        await Task.Delay(100, cancellationToken); // Simulate async work
        
        _logger.LogDebug("Successfully processed FamilyAdminAssigned event for family {FamilyId}",
            familyAdminAssigned.AggregateId);
    }
}