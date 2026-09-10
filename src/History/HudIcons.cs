using System.Collections.Generic;

namespace MoreSlugHUD;

internal static class HudIcons
{
    internal const string AtlasPath = "atlases/input_history";

    internal static readonly MovementTags[] StateOrder =
    {
        MovementTags.Incapacitated,
        MovementTags.Grabbed,
        MovementTags.Shortcut,
        MovementTags.Corridor,
        MovementTags.ZeroG,
        MovementTags.ZeroGPole,
        MovementTags.SurfaceSwim,
        MovementTags.DeepSwim,
        MovementTags.WallClimb,
        MovementTags.Pole,
        MovementTags.Stand,
        MovementTags.Crawl,
        MovementTags.Air,
    };

    internal static readonly string[] StateElements =
    {
        "ih_incap",
        "ih_grabbed",
        "ih_shortcut",
        "ih_corridor",
        "ih_zerog",
        "ih_zerog_pole",
        "ih_swim_surface",
        "ih_swim_deep",
        "ih_wall",
        "ih_pole",
        "ih_stand",
        "ih_crawl",
        "ih_air",
    };

    internal static readonly string[] ActionElements =
    {
        "ih_jump",
        "ih_throw",
        "ih_pickup",
        "ih_special",
    };

    internal static readonly string[] ActionLetters =
    {
        "J",
        "T",
        "G",
        "S",
    };

    internal static readonly string[] DirectionElements =
    {
        "ih_dir_nw", "ih_dir_n", "ih_dir_ne",
        "ih_dir_w", "ih_neutral", "ih_dir_e",
        "ih_dir_sw", "ih_dir_s", "ih_dir_se",
    };

    internal static readonly string[] DirectionLetters =
    {
        "UL", "U", "UR",
        "L", "N", "R",
        "DL", "D", "DR",
    };

    internal static readonly string[] StateLetters =
    {
        "XX",
        "GX",
        "TR",
        "TN",
        "ZR",
        "ZP",
        "FS",
        "DS",
        "WL",
        "PO",
        "ST",
        "CR",
        "AR",
    };

    internal const string NeutralLetter = "N";

    internal const float IconSize = 16f;
    internal const float DurationScale = 0.55f;
    internal const float Slot = 16f;
    internal const float LetterStateSlot = 20f;
    internal const float LetterActionSlot = 12f;
    internal const float DurationColumn = 16f;
    internal const float DirectionGap = 2f;
    internal const int MaxSimultaneousStates = 3;

    private static readonly HashSet<string> LoggedMissing = new();
    private static bool _loadAttempted;

    internal static int StateCount => StateOrder.Length;
    internal static int ActionCount => ActionElements.Length;

    internal static float StateSlot =>
        InputHistoryConfig.DisplayMode == DisplayMode.Letters ? LetterStateSlot : Slot;

    internal static float ActionSlot =>
        InputHistoryConfig.DisplayMode == DisplayMode.Letters ? LetterActionSlot : Slot;

    internal static float RowWidth
    {
        get
        {
            var width = DurationColumn;
            if (InputHistoryConfig.ShowStates)
            {
                width += MaxSimultaneousStates * StateSlot;
            }

            var actions = InputHistoryConfig.TrackedActionCount;
            if (InputHistoryConfig.TrackDirection)
            {
                width += DirectionGap + Slot;
            }
            else if (actions > 0)
            {
                width += DirectionGap;
            }

            return width + actions * ActionSlot;
        }
    }

    internal static void Reset()
    {
        _loadAttempted = false;
    }

    internal static void EnsureLoaded()
    {
        if (_loadAttempted)
        {
            return;
        }

        _loadAttempted = true;
        try
        {
            if (Futile.atlasManager.DoesContainAtlas(AtlasPath))
            {
                return;
            }

            Futile.atlasManager.LoadAtlas(AtlasPath);
        }
        catch (System.Exception exception)
        {
            MoreSlugHUDLog.Error("atlas load failed", exception);
        }
    }

    internal static bool HasElement(string name)
    {
        EnsureLoaded();
        if (Futile.atlasManager.DoesContainElementWithName(name))
        {
            return true;
        }

        if (LoggedMissing.Add(name))
        {
            MoreSlugHUDLog.Warning($"atlas missing element {name}, slot hidden");
        }

        return false;
    }

    internal static string DirectionElement(int x, int y)
    {
        if (x == 0 && y == 0) return "ih_neutral";
        if (x > 0 && y > 0) return "ih_dir_ne";
        if (x > 0 && y < 0) return "ih_dir_se";
        if (x < 0 && y < 0) return "ih_dir_sw";
        if (x < 0 && y > 0) return "ih_dir_nw";
        if (y > 0) return "ih_dir_n";
        if (y < 0) return "ih_dir_s";
        if (x < 0) return "ih_dir_w";
        return "ih_dir_e";
    }
}
