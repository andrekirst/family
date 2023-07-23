namespace Api.Domain.Core;

public class InvalidColorException : Exception
{
    public string Value { get; }

    public InvalidColorException(string value)
    {
        Value = value;
        throw new NotImplementedException();
    }
}