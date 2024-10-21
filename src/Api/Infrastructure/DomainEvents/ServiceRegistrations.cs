using Confluent.Kafka;
using Confluent.Kafka.Admin;

namespace Api.Infrastructure.DomainEvents;

public static class ServiceRegistrations
{
    public static async Task AddDomainEventHandling(this WebApplicationBuilder builder)
    {
        builder.Services.Configure<KafkaOptions>(builder.Configuration.GetSection(KafkaOptions.OptionsName));
        var kafkaOptions = builder.Configuration.GetSection(KafkaOptions.OptionsName);
        var bootstrapServers = kafkaOptions[nameof(KafkaOptions.BootstrapServers)] ?? throw new ArgumentException($"No value for {nameof(KafkaOptions.BootstrapServers)}");
        using var adminClient = new AdminClientBuilder(new AdminClientConfig
        {
            BootstrapServers = bootstrapServers
        }).Build();

        var metadata = adminClient.GetMetadata(TimeSpan.FromSeconds(5));

        if (metadata.Topics.All(t => t.Topic != "DomainEvents"))
        {
            await adminClient.CreateTopicsAsync([
                new TopicSpecification
                {
                    Name = "DomainEvents",
                    ReplicationFactor = 1,
                    NumPartitions = 1
                }
            ]);  
        }
        
        var producerConfig = new ProducerConfig
        {
            BootstrapServers = bootstrapServers
        };
        
        builder.Services.AddSingleton<IProducer<Null, string>>(_ => new ProducerBuilder<Null, string>(producerConfig).Build());
        builder.Services.AddSingleton<IDomainEventBus, DomainEventBus>();
        builder.Services.AddHostedService<DomainEventsConsumer>();
    }
}