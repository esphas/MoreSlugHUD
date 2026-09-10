using System.Runtime.CompilerServices;

namespace MoreSlugHUD;

internal static class IdAttachGate
{
    internal static bool ShouldSkip<TKey>(
        ConditionalWeakTable<TKey, IdAttachBackoff> failed,
        TKey key,
        float now,
        bool live)
        where TKey : class
    {
        if (live)
        {
            return true;
        }

        return failed.TryGetValue(key, out var fail) && fail.IsWaiting(now);
    }
}
