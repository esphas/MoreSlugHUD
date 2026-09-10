namespace MoreSlugHUD;

internal static class GameContext
{
    internal static RainWorldGame? FromPlayer(Player player) =>
        player.abstractCreature?.world?.game;

    internal static RainWorldGame? FromHud(HUD.HUD hud)
    {
        if (hud.rainWorld?.processManager?.currentMainLoop is RainWorldGame game)
        {
            return game;
        }

        if (hud.owner is Player player)
        {
            return FromPlayer(player);
        }

        if (hud.owner is ArenaGameSession arena)
        {
            return arena.game;
        }

        return null;
    }

    internal static RoomCamera? CameraOf(HUD.HUD hud, RainWorldGame? game)
    {
        if (game?.cameras == null)
        {
            return null;
        }

        for (var i = 0; i < game.cameras.Length; i++)
        {
            if (game.cameras[i]?.hud == hud)
            {
                return game.cameras[i];
            }
        }

        return game.cameras.Length == 1 ? game.cameras[0] : null;
    }

    internal static RoomCamera? CameraOfPlayer(Player player, RainWorldGame? game)
    {
        if (game?.cameras == null)
        {
            return null;
        }

        RoomCamera? only = null;
        for (var i = 0; i < game.cameras.Length; i++)
        {
            var camera = game.cameras[i];
            if (camera == null)
            {
                continue;
            }

            only ??= camera;
            if (camera.hud != null && ReferenceEquals(LocalPlayerBinder.Bind(camera.hud), player))
            {
                return camera;
            }

            if (camera.followAbstractCreature == player.abstractCreature)
            {
                return camera;
            }
        }

        return game.cameras.Length == 1 ? only : null;
    }
}
