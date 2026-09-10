using System;
using System.Runtime.CompilerServices;

namespace MoreSlugHUD;

internal static class IdAttachFail
{
    internal const string AttachStage = "Attach";

    internal static void Record<TKey>(
        ConditionalWeakTable<TKey, IdAttachBackoff> failed,
        TKey key,
        Exception exception,
        float now)
        where TKey : class
    {
        if (!failed.TryGetValue(key, out var fail))
        {
            fail = new IdAttachBackoff();
            failed.Add(key, fail);
        }

        fail.Record(AttachStage, exception, now);
    }
}
