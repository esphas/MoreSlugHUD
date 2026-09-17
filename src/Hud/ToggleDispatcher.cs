namespace MoreSlugHUD;

internal static class ToggleDispatcher
{
    internal static void AfterPlayerUpdate(Player player)
    {
        if (!MoreSlugHUDPlugin.Active || !LocalPlayerBinder.CanTrack(player))
        {
            return;
        }

        Visibility.PollToggle(
            player,
            MoreSlugHUDPlugin.ToggleKeybind,
            HudFeatureId.Inventory,
            MoreSlugHUDConfig.Enabled,
            "inventory-toggle");
        Visibility.PollToggle(
            player,
            MoreSlugHUDPlugin.ToggleIdKeybind,
            HudFeatureId.CreatureLabels,
            MoreSlugHUDConfig.IdEnabled,
            "id-toggle");
        Visibility.PollToggle(
            player,
            MoreSlugHUDPlugin.ToggleHistoryKeybind,
            HudFeatureId.InputHistory,
            InputHistoryConfig.Enabled,
            "history-toggle");
    }
}
