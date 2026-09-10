namespace MoreSlugHUD;

internal static class PlayerInputHooks
{
    internal static void Apply()
    {
        On.Player.Update += Update;
    }

    internal static void Remove()
    {
        On.Player.Update -= Update;
    }

    private static void Update(On.Player.orig_Update orig, Player self, bool eu)
    {
        orig(self, eu);
        InputSampler.AfterPlayerUpdate(self);
    }
}
