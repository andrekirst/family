namespace Api.Extensions;

public static class TimeSpanExtensions
{
    public static TimeSpan Seconds(this int seconds) => TimeSpan.FromSeconds(seconds);
}