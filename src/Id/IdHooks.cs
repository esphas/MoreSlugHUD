using System;

namespace MoreSlugHUD;

internal static class IdHooks
{
    internal static void Apply()
    {
        On.Creature.Update += CreatureUpdate;
        On.RainWorldGame.Win += Win;
        On.RainWorldGame.ExitGame += ExitGame;
        On.RainWorldGame.ExitToMenu += ExitToMenu;
        On.ArenaSitting.NextLevel += NextLevel;
        On.ArenaSitting.SessionEnded += SessionEnded;
    }

    internal static void Remove()
    {
        On.Creature.Update -= CreatureUpdate;
        On.RainWorldGame.Win -= Win;
        On.RainWorldGame.ExitGame -= ExitGame;
        On.RainWorldGame.ExitToMenu -= ExitToMenu;
        On.ArenaSitting.NextLevel -= NextLevel;
        On.ArenaSitting.SessionEnded -= SessionEnded;
        IdLabelRegistry.ClearAll();
    }

    private static void CreatureUpdate(On.Creature.orig_Update orig, Creature self, bool eu)
    {
        orig(self, eu);
        if (!MoreSlugHUDPlugin.Active || !MoreSlugHUDConfig.IdAttachable || !IdEligibility.Matches(self))
        {
            return;
        }

        IdLabelRegistry.EnsureAttached(self);
    }

    private static void Win(On.RainWorldGame.orig_Win orig, RainWorldGame self, bool malnourished, bool fromWarpPoint)
    {
        orig(self, malnourished, fromWarpPoint);
        IdLabelRegistry.ClearAll();
    }

    private static void ExitGame(On.RainWorldGame.orig_ExitGame orig, RainWorldGame self, bool asDeath, bool asQuit)
    {
        orig(self, asDeath, asQuit);
        IdLabelRegistry.ClearAll();
    }

    private static void ExitToMenu(On.RainWorldGame.orig_ExitToMenu orig, RainWorldGame self)
    {
        orig(self);
        IdLabelRegistry.ClearAll();
    }

    private static void NextLevel(On.ArenaSitting.orig_NextLevel orig, ArenaSitting self, ProcessManager manager)
    {
        orig(self, manager);
        IdLabelRegistry.ClearAll();
    }

    private static void SessionEnded(On.ArenaSitting.orig_SessionEnded orig, ArenaSitting self, ArenaGameSession session)
    {
        orig(self, session);
        IdLabelRegistry.ClearAll();
    }
}
