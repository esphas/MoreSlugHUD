using System;

namespace MoreSlugHUD;

internal static class HudHooks
{
    internal static void Apply()
    {
        On.HUD.HUD.InitSinglePlayerHud += InitSinglePlayerHud;
        On.HUD.HUD.InitMultiplayerHud += InitMultiplayerHud;
    }

    internal static void Remove()
    {
        On.HUD.HUD.InitSinglePlayerHud -= InitSinglePlayerHud;
        On.HUD.HUD.InitMultiplayerHud -= InitMultiplayerHud;
    }

    private static void InitSinglePlayerHud(
        On.HUD.HUD.orig_InitSinglePlayerHud orig,
        HUD.HUD self,
        RoomCamera camera)
    {
        orig(self, camera);
        AttachOnce(self, "story");
    }

    private static void InitMultiplayerHud(
        On.HUD.HUD.orig_InitMultiplayerHud orig,
        HUD.HUD self,
        ArenaGameSession session)
    {
        orig(self, session);
        AttachOnce(self, "arena");
    }

    private static void AttachOnce(HUD.HUD hud, string source)
    {
        if (hud.fContainers == null || hud.fContainers.Length < 2)
        {
            return;
        }

        var hasInventory = false;
        var hasHistory = false;
        for (var i = 0; i < hud.parts.Count; i++)
        {
            if (hud.parts[i] is InventoryHud)
            {
                hasInventory = true;
            }
            else if (hud.parts[i] is InputHistoryHud)
            {
                hasHistory = true;
            }
        }

        if (!hasInventory)
        {
            try
            {
                hud.AddPart(new InventoryHud(hud));
            }
            catch (Exception exception)
            {
                MoreSlugHUDLog.Error($"inventory HUD attach failed via {source}", exception);
            }
        }

        if (!hasHistory)
        {
            try
            {
                hud.AddPart(new InputHistoryHud(hud));
            }
            catch (Exception exception)
            {
                MoreSlugHUDLog.Error($"history HUD attach failed via {source}", exception);
            }
        }
    }
}
