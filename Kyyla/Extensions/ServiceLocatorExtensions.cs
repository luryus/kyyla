using System;
using Splat;

namespace Kyyla.Extensions;

internal static class ServiceLocatorExtensions
{
    public static T GetRequiredService<T>(this IReadonlyDependencyResolver resolver)
    {
        return resolver.GetService<T>() ??
               throw new InvalidOperationException($"Service of type {typeof(T)} is not registered");
    }
}