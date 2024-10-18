using System.Text.Json;
using Api.Database;
using Api.Database.Calendar;
using Api.Domain;
using Api.Domain.Calendar;
using Api.Infrastructure;
using Confluent.Kafka;
using FluentValidation;
using MediatR;

namespace Api.Features.Calendar.Me;

public record CreateCalendarCommand(CreateCalendarRequest Request) : ICommand<bool>;

public class CreateCalendarRequest
{
    public string Name { get; set; } = default!;
}

public class CreateCalendarCommandValidator : AbstractValidator<CreateCalendarCommand>
{
    public CreateCalendarCommandValidator()
    {
        RuleFor(c => c.Request.Name)
            .NotNull()
            .NotEmpty()
            .MaximumLength(DefaultLengths.Text);
    }
}

public class CreateCalendarCommandHandler(
    ICommandTaskBus commandTaskBus,
    IHttpContextAccessor httpContextAccessor) : ICommandHandler<CreateCalendarCommand, bool>
{
    public async Task<bool> Handle(CreateCalendarCommand command, CancellationToken cancellationToken)
    {
        var familyMemberId = httpContextAccessor.HttpContext!.GetFamilyMemberId();
        var task = new CreateCalendarCommandTask(familyMemberId, command.Request);
        var response = await commandTaskBus.ExecuteAsync(task, cancellationToken);
        return true;
    }
}

public interface ICommandTaskBus
{
    Task<bool> ExecuteAsync<TCommandTask>(TCommandTask commandTask, CancellationToken cancellationToken = default)
        where TCommandTask : ICommandTask;
}

public class CommandTaskBus(IProducer<Null, string> producer) : ICommandTaskBus
{
    public async Task<bool> ExecuteAsync<TCommandTask>(TCommandTask commandTask, CancellationToken cancellationToken = default)
        where TCommandTask : ICommandTask
    {
        var json = JsonSerializer.Serialize(commandTask);
        var result = await producer.ProduceAsync($"CommandTask{typeof(TCommandTask).Name}", new Message<Null, string> { Value = json }, cancellationToken);

        return result.Status == PersistenceStatus.Persisted;
    }
}

public class Consumer : BackgroundService
{
    private readonly ILogger<Consumer> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IConsumer<Ignore, string> _consumer;

    public Consumer(
        ILogger<Consumer> logger,
        IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
        var topics = typeof(ICommandTask)
            .Assembly
            .GetTypes()
            .Where(c => c is { IsClass: true, IsAbstract: false } && typeof(ICommandTask).IsAssignableFrom(c))
            .Select(c => $"CommandTask{c.Name}");
        
        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = "localhost:9092",
            GroupId = "CommandTasksGroup",
            AllowAutoCreateTopics = true,
            EnableAutoCommit = false
        };
        
        _consumer = new ConsumerBuilder<Ignore, string>(consumerConfig).Build();
        _consumer.Subscribe(topics);
    }

    private static Type[]? _cachedImplementationsOfICommandTask;
    
    private static Type[] ImplementationsOfICommandTask()
    {
        return _cachedImplementationsOfICommandTask ??= typeof(ICommandTask)
            .Assembly
            .GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false })
            .ToArray();
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        
        while (!stoppingToken.IsCancellationRequested)
        {
            var result = _consumer.Consume(stoppingToken);
            var commandTaskName = result.Topic[11..];
            var type = ImplementationsOfICommandTask().Single(t => t.Name == commandTaskName);
            var commandTask = JsonSerializer.Deserialize(result.Message.Value, type);
            if (commandTask != null)
            {
                await mediator.Send(commandTask, stoppingToken);
            }
            _consumer.Commit(result);
        }
    }

    public override void Dispose()
    {
        _consumer.Dispose();
    }
}

public class CreateCalendarCommandTask(Guid familyMemberId, CreateCalendarRequest request) : ICommandTask
{
    public CreateCalendarRequest Request { get; set; } = request;
    public Guid FamilyMemberId { get; set; } = familyMemberId;
}

public class CreateCalendarCommandTaskHandler(
    ApplicationDbContext dbContext,
    IUnitOfWork unitOfWork,
    IPublisher publisher) : ICommandHandler<CreateCalendarCommandTask>
{
    public async Task Handle(CreateCalendarCommandTask command, CancellationToken cancellationToken)
    {
        dbContext.Calendars.Add(new CalendarEntity
        {
            Name = command.Request.Name,
            FamilyMemberId = command.FamilyMemberId
        });

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await publisher.Publish(new CalendarCreatedDomainEvent
        {
            Name = command.Request.Name,
            CreatedByFamilyMemberId = command.FamilyMemberId,
            CreatedForFamilyMemberId = command.FamilyMemberId
        }, cancellationToken);
    }
}

public interface ICommandTask : ICommand
{
    Guid FamilyMemberId { get; set; }
}