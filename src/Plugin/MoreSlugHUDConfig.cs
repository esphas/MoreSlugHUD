using UnityEngine;

namespace MoreSlugHUD;

internal enum LayoutStyle
{
    Row = 0,
    Cross = 1,
    Custom = 2,
}

internal enum PositionPreset
{
    BottomCenter = 0,
    TopCenter = 1,
    TopLeft = 2,
    TopRight = 3,
    BottomRight = 4,
    Custom = 5,
}

internal enum IdArrange
{
    Row = 0,
    Stack = 1,
    Cycle = 2,
}

internal enum SlotId
{
    Craft = 0,
    Left = 1,
    Right = 2,
    Stomach = 3,
    Back = 4,
    Pyro = 5,
}

internal static class InventorySlots
{
    internal const int Count = 6;

    internal static readonly SlotId[] All =
    {
        SlotId.Craft, SlotId.Left, SlotId.Right, SlotId.Stomach, SlotId.Back, SlotId.Pyro,
    };
}

internal static class MoreSlugHUDConfig
{
    internal const float SlotSize = 32f;
    internal const float SlotGap = 36f;
    internal const float EmptyAlpha = 0.35f;
    internal const float OffHandAlpha = 0.55f;
    internal const float MainHandAlpha = 1f;
    internal const float PickUpCandidateAlpha = 0.9f;
    internal const float CraftPulseSpeed = 0.06f;
    internal const float CraftPulseGrey = 0.22f;
    internal const float CraftPulseAmplitude = 1f / 7f;
    internal const float CraftGreyMix = 0.87f;
    internal const float StackOffset = 6f;
    internal const int MaxStack = 3;
    internal const float BottomPulseExtent = 12f;
    internal const int PyroSmokeBelowCap = 5;
    internal const int PyroStunBelowCap = 3;

    internal static readonly Color PyroNormal = new(0.88f, 0.32f, 0.36f);
    internal static readonly Color PyroSmoke = new(1f, 0.55f, 0.16f);
    internal static readonly Color PyroStun = new(0.85f, 0.12f, 0.1f);

    internal static Color PyroWarningColor(int heat, int capacity)
    {
        var cap = Mathf.Max(1, capacity);
        if (heat >= Mathf.Max(1, cap - PyroStunBelowCap))
        {
            return PyroStun;
        }

        if (heat >= Mathf.Max(1, cap - PyroSmokeBelowCap))
        {
            return PyroSmoke;
        }

        return PyroNormal;
    }

    internal const float IdLift = 48f;
    internal const float IdLabelScale = 0.7f;
    internal const float IdGlyphScale = 0.85f;
    internal const float IdPadX = 10f;
    internal const float IdBarHeight = 14f;
    internal const float IdBarAlpha = 0.45f;
    internal const int IdIntentHostileEnterTicks = 2;
    internal const int IdIntentLeaveHostileTicks = 10;
    internal const int IdIntentSettleTicks = 16;
    internal const int IdIntentShakeTicks = 6;
    internal const float IdIntentShakePx = 2.4f;
    internal const float IdIntentGap = 5f;
    internal const float IdIntentHeight = 18f;
    internal const float IdItemGap = 6f;
    internal const int IdCycleTicks = 120;

    internal static bool Enabled { get; set; } = true;
    internal static float CustomX { get; set; } = 50f;
    internal static float CustomY { get; set; } = 30f;
    internal static float SlotCraftX { get; set; } = -136f;
    internal static float SlotCraftY { get; set; }
    internal static float SlotLeftX { get; set; } = -68f;
    internal static float SlotLeftY { get; set; }
    internal static float SlotRightX { get; set; }
    internal static float SlotRightY { get; set; }
    internal static float SlotStomachX { get; set; } = 68f;
    internal static float SlotStomachY { get; set; }
    internal static float SlotBackX { get; set; } = 136f;
    internal static float SlotBackY { get; set; }
    internal static float SlotPyroX { get; set; }
    internal static float SlotPyroY { get; set; } = -68f;
    internal static bool ShowLeft { get; set; } = true;
    internal static bool ShowRight { get; set; } = true;
    internal static bool ShowStomach { get; set; } = true;
    internal static bool ShowBack { get; set; } = true;
    internal static bool ShowCraft { get; set; } = true;
    internal static bool ShowPyro { get; set; } = true;
    internal static bool ShowPickUpCandidate { get; set; } = true;
    internal static bool IdEnabled { get; set; } = true;
    internal static bool ShowId { get; set; } = true;
    internal static bool ShowName { get; set; } = true;
    internal static float FamiliarityThreshold { get; set; } = 0.5f;
    internal static bool IdStrictWatch { get; set; } = true;
    internal static float IdWatchSeconds { get; set; } = 20f;
    internal static bool ShowDead { get; set; } = true;
    internal static bool ShowIntent { get; set; }
    internal static bool ShowLabelBackground { get; set; }
    internal static IdArrange IdArrange { get; set; } = IdArrange.Row;

    internal static bool IdAttachable => IdEnabled && (ShowId || ShowName || ShowIntent);

    internal static bool IsSlotEnabled(SlotId id) => id switch
    {
        SlotId.Craft => ShowCraft,
        SlotId.Left => ShowLeft,
        SlotId.Right => ShowRight,
        SlotId.Stomach => ShowStomach,
        SlotId.Back => ShowBack,
        SlotId.Pyro => ShowPyro,
        _ => false,
    };

    internal static Vector2 SlotLocal(SlotId id) => id switch
    {
        SlotId.Craft => new Vector2(SlotCraftX, SlotCraftY),
        SlotId.Left => new Vector2(SlotLeftX, SlotLeftY),
        SlotId.Right => new Vector2(SlotRightX, SlotRightY),
        SlotId.Stomach => new Vector2(SlotStomachX, SlotStomachY),
        SlotId.Back => new Vector2(SlotBackX, SlotBackY),
        SlotId.Pyro => new Vector2(SlotPyroX, SlotPyroY),
        _ => Vector2.zero,
    };

    internal static void SetSlotLocal(SlotId id, Vector2 local)
    {
        local.x = Mathf.Round(Mathf.Clamp(local.x, InventoryLayoutPresets.LocalMin, InventoryLayoutPresets.LocalMax));
        local.y = Mathf.Round(Mathf.Clamp(local.y, InventoryLayoutPresets.LocalMin, InventoryLayoutPresets.LocalMax));
        switch (id)
        {
            case SlotId.Craft:
                SlotCraftX = local.x;
                SlotCraftY = local.y;
                break;
            case SlotId.Left:
                SlotLeftX = local.x;
                SlotLeftY = local.y;
                break;
            case SlotId.Right:
                SlotRightX = local.x;
                SlotRightY = local.y;
                break;
            case SlotId.Stomach:
                SlotStomachX = local.x;
                SlotStomachY = local.y;
                break;
            case SlotId.Back:
                SlotBackX = local.x;
                SlotBackY = local.y;
                break;
            case SlotId.Pyro:
                SlotPyroX = local.x;
                SlotPyroY = local.y;
                break;
        }
    }

    internal static bool ShowEmpty(SlotId id) => id != SlotId.Craft && id != SlotId.Pyro;

    internal static IdArrange ParseIdArrange(string value) => value switch
    {
        "stack" => IdArrange.Stack,
        "cycle" => IdArrange.Cycle,
        _ => IdArrange.Row,
    };

    internal static LayoutStyle ParseLayout(string value) => value switch
    {
        "cross" => LayoutStyle.Cross,
        "custom" => LayoutStyle.Custom,
        _ => LayoutStyle.Row,
    };

    internal static PositionPreset ParsePosition(string value) => value switch
    {
        "topCenter" => PositionPreset.TopCenter,
        "topLeft" => PositionPreset.TopLeft,
        "topRight" => PositionPreset.TopRight,
        "bottomRight" => PositionPreset.BottomRight,
        "custom" => PositionPreset.Custom,
        _ => PositionPreset.BottomCenter,
    };
}
