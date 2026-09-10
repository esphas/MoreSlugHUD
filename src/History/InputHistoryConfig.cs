using UnityEngine;

namespace MoreSlugHUD;

internal enum LayoutMode
{
    Left = 0,
    Right = 1,
}

internal enum DisplayMode
{
    Icons = 0,
    Letters = 1,
}

internal enum HistoryDensity
{
    Compact = 0,
    Loose = 1,
}

internal static class InputHistoryConfig
{
    internal static bool Enabled { get; set; } = true;
    internal static LayoutMode Layout { get; set; } = LayoutMode.Left;
    internal static DisplayMode DisplayMode { get; set; } = DisplayMode.Letters;
    internal static HistoryDensity Density { get; set; } = HistoryDensity.Loose;
    internal static int MaxRows { get; set; } = 17;
    internal static float Opacity { get; set; } = 0.85f;
    internal static bool TrackDirection { get; set; } = true;
    internal static bool TrackActions { get; set; } = true;
    internal static MovementTags TrackedStateMask { get; set; } = AllStates;

    internal const MovementTags AllStates =
        MovementTags.Incapacitated
        | MovementTags.Grabbed
        | MovementTags.Shortcut
        | MovementTags.Corridor
        | MovementTags.ZeroG
        | MovementTags.ZeroGPole
        | MovementTags.SurfaceSwim
        | MovementTags.DeepSwim
        | MovementTags.WallClimb
        | MovementTags.Pole
        | MovementTags.Stand
        | MovementTags.Crawl
        | MovementTags.Air;

    internal static int EffectiveMaxRows => MaxRows < 1 ? 1 : MaxRows;

    internal static bool ShowStates => TrackedStateMask != MovementTags.None;

    internal static int TrackedActionCount => TrackActions ? HudIcons.ActionCount : 0;

    internal static float SideInset => Density == HistoryDensity.Compact ? 18f : 32f;

    internal static float TopInset => Density == HistoryDensity.Compact ? 76f : 100f;

    internal static float LineHeight => Density == HistoryDensity.Compact ? 18f : 24f;

    internal static void ResolveAnchor(Vector2 screen, Vector2 safe, out float x, out float y, out bool alignLeft)
    {
        y = screen.y - safe.y - TopInset;
        if (Layout == LayoutMode.Right)
        {
            x = screen.x - SideInset - safe.x;
            alignLeft = false;
            return;
        }

        x = SideInset + safe.x;
        alignLeft = true;
    }
}
