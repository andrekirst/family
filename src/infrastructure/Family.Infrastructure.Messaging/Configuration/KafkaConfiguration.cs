namespace Family.Infrastructure.Messaging.Configuration;

public class KafkaConfiguration
{
    public const string SectionName = "Kafka";
    
    [Required]
    public string BootstrapServers { get; set; } = "localhost:9092";
    
    public string GroupId { get; set; } = "family-api";
    
    public string ClientId { get; set; } = "family-api-client";
    
    public int RetryAttempts { get; set; } = 3;
    
    public int RetryDelayMs { get; set; } = 1000;
    
    public bool EnableAutoCommit { get; set; } = true;
    
    public int AutoCommitIntervalMs { get; set; } = 1000;
    
    public AutoOffsetReset AutoOffsetReset { get; set; } = AutoOffsetReset.Earliest;
    
    public SecurityProtocol SecurityProtocol { get; set; } = SecurityProtocol.Plaintext;
    
    public CompressionType CompressionType { get; set; } = CompressionType.None;
    
    public int MessageTimeoutMs { get; set; } = 30000;
    
    public int BatchSize { get; set; } = 16384;
    
    public int LingerMs { get; set; } = 5;
    
    public TopicConfiguration Topics { get; set; } = new();
}

public class TopicConfiguration
{
    public string FamilyEvents { get; set; } = "family.events";
    public string UserEvents { get; set; } = "user.events";
    public string NotificationEvents { get; set; } = "notification.events";
    public string AuditEvents { get; set; } = "audit.events";
    public string IntegrationEvents { get; set; } = "integration.events";
    
    public int DefaultPartitions { get; set; } = 3;
    public short DefaultReplicationFactor { get; set; } = 1;
}