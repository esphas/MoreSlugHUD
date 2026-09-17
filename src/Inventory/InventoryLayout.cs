using UnityEngine;

namespace MoreSlugHUD;

internal static class InventoryLayout
{
    internal static Vector2 OriginOf(HUD.HUD hud)
    {
        var screen = hud.rainWorld.options.ScreenSize;
        var safe = hud.rainWorld.options.SafeScreenOffset;
        return InventoryLayoutPresets.Origin(
            screen,
            safe,
            MoreSlugHUDConfig.CustomX,
            MoreSlugHUDConfig.CustomY);
    }
}
