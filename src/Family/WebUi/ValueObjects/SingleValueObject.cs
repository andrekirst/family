namespace WebUi.ValueObjects;

public abstract class SingleValueObject<T>
{
    public T? Value { get; private set; }

    protected SingleValueObject()
    {
    }

    protected SingleValueObject(T value)
    {
        Value = value;
    }

    public override string ToString() => Value?.ToString() ?? base.ToString() ?? "<Unknown>";
}