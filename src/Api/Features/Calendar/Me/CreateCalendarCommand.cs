using System.Reflection;
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

public class Consumer(
    IConsumer<Ignore, string> consumer,
    ILogger<Consumer> logger,
    IServiceScopeFactory serviceScopeFactory) : IHostedService, IDisposable
{
    // https://stackoverflow.com/questions/64141794/how-to-use-mediator-inside-background-service-in-c-sharp-asp-net-core
    private Timer? _timer = null;
    
    public Task StartAsync(CancellationToken cancellationToken)
    {
        var topics = typeof(ICommandTask)
            .Assembly
            .GetTypes()
            .Where(c => c is { IsClass: true, IsAbstract: false } && typeof(ICommandTask).IsAssignableFrom(c))
            .Select(c => $"CommandTask{c.Name}");
        
        consumer.Subscribe(topics);

        _timer = new Timer(Consume, null, TimeSpan.Zero, TimeSpan.FromSeconds(2));
        
        return Task.CompletedTask;
    }

    private void Consume(object? state)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var canceled = false;
        while (!canceled)
        {
            try
            {
                var result = consumer.Consume();
                var commandTaskName = result.Topic[11..];
                var type = typeof(ICommandTask)
                    .Assembly
                    .GetTypes()
                    .Single(t => t is { IsClass: true, IsAbstract: false } && t.Name == commandTaskName);
                var commandTask = JsonSerializer.Deserialize(result.Message.Value, type);
                if (commandTask != null) mediator.Send(commandTask);
                consumer.Commit(result);
            }
            catch (Exception e)
            {
                canceled = true;
                logger.LogError(e, e.Message);
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
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