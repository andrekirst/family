namespace Api.Infrastructure;

public class KafkaOptions
{
    public const string OptionsName = "Kafka";
    public string BootstrapServers { get; set; }
}