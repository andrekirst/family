using Microsoft.Extensions.Logging;

namespace Family.Infrastructure.Resilience.Tests.TestHelpers;

/// <summary>
/// Fake logger implementation for testing
/// </summary>
public class FakeLogger<T> : ILogger<T>
{
    public List<LogEntry> LogEntries { get; } = new();

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return new FakeScope();
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return true;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        var message = formatter(state, exception);
        LogEntries.Add(new LogEntry(logLevel, eventId, message, exception, state));
    }

    public bool HasLogLevel(LogLevel logLevel) => LogEntries.Any(e => e.LogLevel == logLevel);
    public bool HasMessage(string message) => LogEntries.Any(e => e.Message.Contains(message));
    public bool HasException<TException>() where TException : Exception => LogEntries.Any(e => e.Exception is TException);

    private class FakeScope : IDisposable
    {
        public void Dispose()
        {
            // Nothing to dispose
        }
    }
}

/// <summary>
/// Log entry for testing
/// </summary>
public record LogEntry(LogLevel LogLevel, EventId EventId, string Message, Exception? Exception, object? State);