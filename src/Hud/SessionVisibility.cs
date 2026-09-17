using System.Collections.Generic;

namespace MoreSlugHUD;

internal static class SessionVisibility
{
    private sealed class ToggleState
    {
        internal bool Visible = true;
        internal int LastToggleClock = int.MinValue;
    }

    private sealed class PlayerToggles
    {
        internal readonly Dictionary<HudFeatureId, ToggleState> ByFeature = new();
    }

    private static readonly Dictionary<int, PlayerToggles> Table = new();
    private static string? _playthrough;

    internal static void Reset()
    {
        Table.Clear();
        _playthrough = null;
    }

    internal static bool IsVisible(Player player, HudFeatureId feature)
    {
        return StateOf(Of(player), feature).Visible;
    }

    internal static void Toggle(Player player, HudFeatureId feature)
    {
        var clock = player.abstractCreature?.world?.game?.clock ?? int.MinValue;
        var state = StateOf(Of(player), feature);
        if (clock == state.LastToggleClock)
        {
            return;
        }

        state.LastToggleClock = clock;
        state.Visible = !state.Visible;
    }

    private static PlayerToggles Of(Player player)
    {
        EnsurePlaythrough(player);
        var key = player.playerState?.playerNumber ?? 0;
        if (!Table.TryGetValue(key, out var flags))
        {
            flags = new PlayerToggles();
            Table[key] = flags;
        }

        return flags;
    }

    private static ToggleState StateOf(PlayerToggles flags, HudFeatureId feature)
    {
        if (!flags.ByFeature.TryGetValue(feature, out var state))
        {
            state = new ToggleState();
            flags.ByFeature[feature] = state;
        }

        return state;
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
}
