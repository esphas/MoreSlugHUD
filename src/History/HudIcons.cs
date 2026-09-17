using System.Collections.Generic;

namespace MoreSlugHUD;

internal static class HudIcons
{
    internal const string AtlasPath = "atlases/input_history";

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

    internal const string NeutralLetter = "N";

    private static readonly HashSet<string> LoggedMissing = new();
    private static bool _loadAttempted;

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
