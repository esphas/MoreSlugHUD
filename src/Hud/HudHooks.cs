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

        TryAdd(hud, source, HudFeatures.ScreenHuds);
    }

    private static void TryAdd(HUD.HUD hud, string source, ScreenHudAttachment[] attachments)
    {
        for (var i = 0; i < attachments.Length; i++)
        {
            TryAdd(hud, source, attachments[i]);
        }
    }

    private static void TryAdd(HUD.HUD hud, string source, ScreenHudAttachment attachment)
    {
        for (var i = 0; i < hud.parts.Count; i++)
        {
            if (attachment.AlreadyAttached(hud.parts[i]))
            {
                return;
            }
        }

        try
        {
            hud.AddPart(attachment.Create(hud));
        }
        catch (Exception exception)
        {
            MoreSlugHUDLog.Error($"{attachment.Name} HUD attach failed via {source}", exception);
        }
    }
}
