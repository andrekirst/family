﻿namespace Api.Infrastructure;

public interface IMap<out TTarget, in TSource>
{
    static abstract TTarget MapFromSource(TSource source);
}