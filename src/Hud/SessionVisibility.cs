using System.Collections.Generic;

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

    private static readonly Dictionary<int, Flags> Table = new();
    private static string? _playthrough;

    internal static void Reset()
    {
        Table.Clear();
        _playthrough = null;
    }

    internal static bool IsVisible(Player player, HudLayer layer)
    {
        return LayerOf(Of(player), layer);
    }

    internal static void Toggle(Player player, HudLayer layer)
    {
        var clock = player.abstractCreature?.world?.game?.clock ?? int.MinValue;
        var flags = Of(player);
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

    private static Flags Of(Player player)
    {
        EnsurePlaythrough(player);
        var key = player.playerState?.playerNumber ?? 0;
        if (!Table.TryGetValue(key, out var flags))
        {
            flags = new Flags();
            Table[key] = flags;
        }

        return flags;
    }

    private static void EnsurePlaythrough(Player player)
    {
        var playthrough = PlaythroughOf(player);
        if (playthrough == null)
        {
            return;
        }

        if (_playthrough == null)
        {
            _playthrough = playthrough;
            return;
        }

        if (_playthrough == playthrough)
        {
            return;
        }

        Table.Clear();
        _playthrough = playthrough;
    }

    private static string? PlaythroughOf(Player player)
    {
        var game = GameContext.FromPlayer(player);
        if (game == null)
        {
            return null;
        }

        if (game.IsArenaSession)
        {
            return "arena";
        }

        if (!game.IsStorySession)
        {
            return "other";
        }

        var slot = game.rainWorld?.options?.saveSlot ?? 0;
        var slug = game.GetStorySession.saveState?.saveStateNumber?.value ?? "story";
        return $"story:{slot}:{slug}";
    }

    private static bool LayerOf(Flags flags, HudLayer layer) => layer switch
    {
        HudLayer.Id => flags.Id,
        HudLayer.History => flags.History,
        _ => flags.Inventory,
    };

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
