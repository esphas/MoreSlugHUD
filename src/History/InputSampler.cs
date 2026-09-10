using System;

namespace MoreSlugHUD;

internal static class InputSampler
{
    internal static void AfterPlayerUpdate(Player player)
    {
        try
        {
            Sample(player);
        }
        catch (Exception exception)
        {
            MoreSlugHUDLog.ErrorOnce("history-sample", "history sample failed", exception);
        }
    }

    private static void Sample(Player player)
    {
        if (!InputHistoryConfig.Enabled || !MoreSlugHUDPlugin.Active || !LocalPlayerBinder.CanTrack(player))
        {
            return;
        }

        var game = GameContext.FromPlayer(player);
        if (!Visibility.CanRecord(player, game))
        {
            return;
        }

        var timeline = PlayerTimelines.Get(player);
        var tags = MovementStateClassifier.Classify(player);
        var state = new InputState(player.input[0], tags);
        var created = timeline.Observe(state, InputHistoryConfig.EffectiveMaxRows);
        var stateCount = CountTags(state.Movement);
        if (created && stateCount > HudIcons.MaxSimultaneousStates)
        {
            MoreSlugHUDLog.Warning(
                $"state count {stateCount} exceeds reserved {HudIcons.MaxSimultaneousStates}: {InputHistoryFormatter.FormatTags(tags)}");
        }
    }

    private static int CountTags(MovementTags tags)
    {
        var count = 0;
        var value = (ushort)tags;
        while (value != 0)
        {
            count += value & 1;
            value >>= 1;
        }

        return count;
    }
}
