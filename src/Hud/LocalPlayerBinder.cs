namespace MoreSlugHUD;

internal static class LocalPlayerBinder
{
    internal static Player? Bind(HUD.HUD hud)
    {
        var ownerType = hud.owner?.GetOwnerType();
        if (ownerType != HUD.HUD.OwnerType.Player && ownerType != HUD.HUD.OwnerType.ArenaSession)
        {
            return null;
        }

        Player? candidate = hud.owner as Player;
        if (candidate == null)
        {
            var game = GameContext.FromHud(hud);
            var camera = GameContext.CameraOf(hud, game);
            if (camera?.followAbstractCreature?.realizedCreature is Player followed)
            {
                candidate = followed;
            }
        }

        if (candidate == null || !IsEligible(candidate))
        {
            return null;
        }

        if (!IsLocalOwned(candidate))
        {
            return null;
        }

        return candidate;
    }

    internal static bool CanTrack(Player player) => IsEligible(player) && IsLocalOwned(player);

    internal static bool IsEligible(Player player) =>
        !player.isNPC && !player.isSlugpup && player.playerState != null;

    private static bool IsLocalOwned(Player player)
    {
        if (!MeadowOwnership.IsMeadowActive)
        {
            return true;
        }

        return MeadowOwnership.TryIsLocal(player, out var isLocal) && isLocal;
    }
}
