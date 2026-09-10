using System;
using System.Collections.Generic;

namespace MoreSlugHUD;

internal static class IdAttachIndex
{
    internal static bool TryGetLive<TKey, TLabel>(
        Dictionary<TKey, TLabel> instances,
        TKey key,
        Func<TLabel, bool> isLive,
        out TLabel label)
        where TKey : notnull
        where TLabel : class
    {
        if (instances.TryGetValue(key, out label!) && isLive(label))
        {
            return true;
        }

        label = null!;
        return false;
    }

    internal static void Publish<TKey, TLabel>(Dictionary<TKey, TLabel> instances, TKey key, TLabel label)
        where TKey : notnull
    {
        instances[key] = label;
    }

    internal static bool RemoveIfCurrent<TKey, TLabel>(Dictionary<TKey, TLabel> instances, TKey key, TLabel label)
        where TKey : notnull
        where TLabel : class
    {
        if (!instances.TryGetValue(key, out var current) || !ReferenceEquals(current, label))
        {
            return false;
        }

        instances.Remove(key);
        return true;
    }
}
