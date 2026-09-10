using System.Runtime.CompilerServices;

namespace MoreSlugHUD;

internal static class PlayerTimelines
{
    private static readonly ConditionalWeakTable<Player, InputTimeline> Values = new();

    internal static InputTimeline Get(Player player) => Values.GetOrCreateValue(player);

    internal static bool TryGet(Player player, out InputTimeline timeline) =>
        Values.TryGetValue(player, out timeline);
}
