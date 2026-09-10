using System;
using ImprovedInput;

namespace MoreSlugHUD;

internal static class Visibility
{
    internal static string? HideHud(HUD.HUD hud, Player? player, RainWorldGame? game, HudLayer layer)
    {
        if (!FeatureEnabled(layer))
        {
            return "disabled";
        }

        var ownerType = hud.owner?.GetOwnerType();
        if (ownerType != HUD.HUD.OwnerType.Player && ownerType != HUD.HUD.OwnerType.ArenaSession)
        {
            return "owner";
        }

        if (game == null)
        {
            return "no-game";
        }

        if (player == null)
        {
            return "no-player";
        }

        if (!SessionVisibility.IsVisible(player, layer))
        {
            return "toggled-off";
        }

        return HideWorld(hud, GameContext.CameraOf(hud, game), player, game);
    }

    internal static string? HideWorld(HUD.HUD? hud, RoomCamera? camera, Player? player, RainWorldGame? game)
    {
        if (game == null)
        {
            return "no-game";
        }

        if (game.GamePaused)
        {
            return "paused";
        }

        var host = hud ?? camera?.hud;
        if (host != null && host.HideGeneralHud)
        {
            return "hide-general";
        }

        camera ??= host != null ? GameContext.CameraOf(host, game) : null;
        if (camera != null && camera.InCutscene)
        {
            return "cutscene";
        }

        if (camera?.hud?.textPrompt is { gameOverMode: true })
        {
            return "game-over";
        }

        if (player != null && player.RevealMap)
        {
            return "map";
        }

        return null;
    }

    internal static bool CanRecord(Player player, RainWorldGame? game)
    {
        if (game == null || player.input == null || player.input.Length == 0)
        {
            return false;
        }

        if (game.GamePaused)
        {
            return false;
        }

        if (game.manager?.currentMainLoop is not RainWorldGame)
        {
            return false;
        }

        var camera = GameContext.CameraOfPlayer(player, game);
        if (camera != null && camera.InCutscene)
        {
            return false;
        }

        return camera?.hud?.textPrompt is not { gameOverMode: true };
    }

    internal static void PollToggle(Player? player, PlayerKeybind? key, HudLayer layer, bool featureOn, string errorKey)
    {
        if (player == null || !featureOn || key == null)
        {
            return;
        }

        try
        {
            if (player.JustPressed(key))
            {
                SessionVisibility.Toggle(player, layer);
            }
        }
        catch (Exception exception)
        {
            MoreSlugHUDLog.ErrorOnce(errorKey, $"{layer} toggle key failed", exception);
        }
    }

    private static bool FeatureEnabled(HudLayer layer) => layer switch
    {
        HudLayer.Id => MoreSlugHUDConfig.IdAttachable,
        HudLayer.History => InputHistoryConfig.Enabled,
        _ => MoreSlugHUDConfig.Enabled,
    };
}
