namespace Api.Infrastructure;

public sealed record Error(string Code, string? Message = null);