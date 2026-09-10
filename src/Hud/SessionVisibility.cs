using System.Runtime.CompilerServices;

namespace MoreSlugHUD;

internal enum HudLayer
{
    Inventory,
    Id,
    History,
}

internal static class SessionVisibility
{
    private sealed class Flags
    {
        internal bool Inventory = true;
        internal bool Id = true;
        internal bool History = true;
        internal int InventoryClock = int.MinValue;
        internal int IdClock = int.MinValue;
        internal int HistoryClock = int.MinValue;
    }

    private static readonly ConditionalWeakTable<Player, Flags> Table = new();

    internal static bool IsVisible(Player player, HudLayer layer)
    {
        var flags = Table.GetValue(player, _ => new Flags());
        return layer switch
        {
            HudLayer.Id => flags.Id,
            HudLayer.History => flags.History,
            _ => flags.Inventory,
        };
    }

    internal static void Toggle(Player player, HudLayer layer)
    {
        var clock = player.abstractCreature?.world?.game?.clock ?? int.MinValue;
        var flags = Table.GetValue(player, _ => new Flags());
        ref var last = ref ClockOf(flags, layer);
        if (clock == last)
        {
            return;
        }

        last = clock;
        switch (layer)
        {
            case HudLayer.Id:
                flags.Id = !flags.Id;
                break;
            case HudLayer.History:
                flags.History = !flags.History;
                break;
            default:
                flags.Inventory = !flags.Inventory;
                break;
        }
    }

    private static ref int ClockOf(Flags flags, HudLayer layer)
    {
        switch (layer)
        {
            case HudLayer.Id:
                return ref flags.IdClock;
            case HudLayer.History:
                return ref flags.HistoryClock;
            default:
                return ref flags.InventoryClock;
        }
    }
}
