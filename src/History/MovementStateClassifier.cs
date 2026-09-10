using System.Collections.Generic;

namespace MoreSlugHUD;

internal static class MovementStateClassifier
{
    private const MovementTags EnvironmentMask =
        MovementTags.Shortcut
        | MovementTags.Corridor
        | MovementTags.ZeroG
        | MovementTags.ZeroGPole
        | MovementTags.SurfaceSwim
        | MovementTags.DeepSwim
        | MovementTags.WallClimb
        | MovementTags.Pole;

    private static readonly HashSet<string> LoggedUnknown = new();

    internal static MovementTags Classify(Player player)
    {
        var tags = MovementTags.None;

        if (player.dead
            || !player.Consious
            || Is(player.bodyMode, Player.BodyModeIndex.Stunned)
            || Is(player.bodyMode, Player.BodyModeIndex.Dead)
            || Is(player.animation, Player.AnimationIndex.Dead))
        {
            tags |= MovementTags.Incapacitated;
        }

        var grabbedBy = player.grabbedBy;
        if (grabbedBy != null && grabbedBy.Count > 0)
        {
            tags |= MovementTags.Grabbed;
        }

        if (player.inShortcut
            || player.inShortcutVessel != null
            || player.enteringShortCut.HasValue
            || Is(player.bodyMode, Player.BodyModeIndex.ClimbIntoShortCut))
        {
            tags |= MovementTags.Shortcut;
        }

        var body = player.bodyMode;
        var animation = player.animation;

        if (Is(body, Player.BodyModeIndex.CorridorClimb)
            || Is(animation, Player.AnimationIndex.CorridorTurn))
        {
            tags |= MovementTags.Corridor;
        }

        if (Is(animation, Player.AnimationIndex.ZeroGPoleGrab))
        {
            tags |= MovementTags.ZeroGPole;
        }
        else if (Is(body, Player.BodyModeIndex.ZeroG)
            || Is(animation, Player.AnimationIndex.ZeroGSwim))
        {
            tags |= MovementTags.ZeroG;
        }

        if (Is(animation, Player.AnimationIndex.SurfaceSwim))
        {
            tags |= MovementTags.SurfaceSwim;
        }
        else if (Is(animation, Player.AnimationIndex.DeepSwim)
            || Is(body, Player.BodyModeIndex.Swimming))
        {
            tags |= MovementTags.DeepSwim;
        }

        if (Is(body, Player.BodyModeIndex.WallClimb))
        {
            tags |= MovementTags.WallClimb;
        }

        if (Is(body, Player.BodyModeIndex.ClimbingOnBeam) || IsPoleAnimation(animation))
        {
            tags |= MovementTags.Pole;
        }

        if (Is(body, Player.BodyModeIndex.Stand))
        {
            tags |= MovementTags.Stand;
        }

        if (Is(body, Player.BodyModeIndex.Crawl))
        {
            tags |= MovementTags.Crawl;
        }

        if (Is(body, Player.BodyModeIndex.Default) && (tags & EnvironmentMask) == 0)
        {
            tags |= MovementTags.Air;
        }

        NoteUnknown(body, animation);
        return tags;
    }

    private static bool IsPoleAnimation(Player.AnimationIndex? animation) =>
        Is(animation, Player.AnimationIndex.HangFromBeam)
        || Is(animation, Player.AnimationIndex.GetUpOnBeam)
        || Is(animation, Player.AnimationIndex.StandOnBeam)
        || Is(animation, Player.AnimationIndex.ClimbOnBeam)
        || Is(animation, Player.AnimationIndex.GetUpToBeamTip)
        || Is(animation, Player.AnimationIndex.HangUnderVerticalBeam)
        || Is(animation, Player.AnimationIndex.BeamTip)
        || Is(animation, Player.AnimationIndex.VineGrab)
        || Is(animation, Player.AnimationIndex.AntlerClimb);

    private static bool Is(Player.BodyModeIndex? actual, Player.BodyModeIndex expected) =>
        actual != null && actual == expected;

    private static bool Is(Player.AnimationIndex? actual, Player.AnimationIndex expected) =>
        actual != null && actual == expected;

    private static void NoteUnknown(Player.BodyModeIndex? body, Player.AnimationIndex? animation)
    {
        if (body != null
            && body != Player.BodyModeIndex.Default
            && body != Player.BodyModeIndex.Crawl
            && body != Player.BodyModeIndex.Stand
            && body != Player.BodyModeIndex.CorridorClimb
            && body != Player.BodyModeIndex.ClimbIntoShortCut
            && body != Player.BodyModeIndex.WallClimb
            && body != Player.BodyModeIndex.ClimbingOnBeam
            && body != Player.BodyModeIndex.Swimming
            && body != Player.BodyModeIndex.ZeroG
            && body != Player.BodyModeIndex.Stunned
            && body != Player.BodyModeIndex.Dead)
        {
            LogUnknown("BodyMode", body.ToString());
        }

        if (animation != null
            && animation != Player.AnimationIndex.None
            && animation != Player.AnimationIndex.CrawlTurn
            && animation != Player.AnimationIndex.StandUp
            && animation != Player.AnimationIndex.DownOnFours
            && animation != Player.AnimationIndex.LedgeCrawl
            && animation != Player.AnimationIndex.LedgeGrab
            && animation != Player.AnimationIndex.HangFromBeam
            && animation != Player.AnimationIndex.GetUpOnBeam
            && animation != Player.AnimationIndex.StandOnBeam
            && animation != Player.AnimationIndex.ClimbOnBeam
            && animation != Player.AnimationIndex.GetUpToBeamTip
            && animation != Player.AnimationIndex.HangUnderVerticalBeam
            && animation != Player.AnimationIndex.BeamTip
            && animation != Player.AnimationIndex.CorridorTurn
            && animation != Player.AnimationIndex.SurfaceSwim
            && animation != Player.AnimationIndex.DeepSwim
            && animation != Player.AnimationIndex.Roll
            && animation != Player.AnimationIndex.Flip
            && animation != Player.AnimationIndex.RocketJump
            && animation != Player.AnimationIndex.BellySlide
            && animation != Player.AnimationIndex.AntlerClimb
            && animation != Player.AnimationIndex.GrapplingSwing
            && animation != Player.AnimationIndex.ZeroGSwim
            && animation != Player.AnimationIndex.ZeroGPoleGrab
            && animation != Player.AnimationIndex.VineGrab
            && animation != Player.AnimationIndex.Dead)
        {
            LogUnknown("Animation", animation.ToString());
        }
    }

    private static void LogUnknown(string layer, string? name)
    {
        var key = layer + ":" + (name ?? "null");
        if (!LoggedUnknown.Add(key))
        {
            return;
        }

        MoreSlugHUDLog.Warning($"unrecognized {layer} '{name}', that layer adds no tag");
    }
}
