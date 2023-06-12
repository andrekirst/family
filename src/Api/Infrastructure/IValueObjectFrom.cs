namespace Api.Infrastructure;

public interface IValueObjectFrom<out TTarget, in TFrom>
{
    static abstract TTarget From(TFrom value);
    static abstract TTarget FromNull(TFrom? value);
    static abstract TTarget FromRaw(TFrom value);
    static abstract TTarget FromNullRaw(TFrom? value);
}