using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace MoreSlugHUD;

internal static class IdAttachAttempt
{
    internal static void Run<TKey, TLabel>(
        Dictionary<TKey, TLabel> instances,
        ConditionalWeakTable<TKey, IdAttachBackoff> failed,
        TKey key,
        Func<TLabel, bool> isLive,
        Func<TLabel> create,
        Action<TLabel> join,
        Action<TKey, TLabel?, Exception, float> rollback,
        float now)
        where TKey : class
        where TLabel : class
    {
        if (IdAttachIndex.TryGetLive(instances, key, isLive, out _))
        {
            return;
        }

        if (failed.TryGetValue(key, out var waiting) && waiting.IsWaiting(now))
        {
            return;
        }

        TLabel? label = null;
        try
        {
            label = create();
            IdAttachIndex.Publish(instances, key, label);
            join(label);
        }
        catch (Exception exception)
        {
            rollback(key, label, exception, now);
            return;
        }

        ConfirmJoined(instances, failed, key, label, isLive);
    }

    internal static void ConfirmJoined<TKey, TLabel>(
        Dictionary<TKey, TLabel> instances,
        ConditionalWeakTable<TKey, IdAttachBackoff> failed,
        TKey key,
        TLabel label,
        Func<TLabel, bool> isLive)
        where TKey : class
        where TLabel : class
    {
        if (IdAttachIndex.TryGetLive(instances, key, isLive, out var live) && ReferenceEquals(live, label))
        {
            failed.Remove(key);
        }
    }
}
