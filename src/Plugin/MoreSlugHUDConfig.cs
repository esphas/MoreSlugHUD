namespace MoreSlugHUD;

internal enum LayoutStyle
{
    Row = 0,
    Cross = 1,
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
}

internal static class MoreSlugHUDConfig
{
    internal const float SlotSize = 32f;
    internal const float SlotGap = 36f;
    internal const float EmptyAlpha = 0.35f;
    internal const float OffHandAlpha = 0.55f;
    internal const float MainHandAlpha = 1f;
    internal const float CraftPulseSpeed = 0.06f;
    internal const float CraftPulseGrey = 0.22f;
    internal const float CraftPulseAmplitude = 1f / 7f;
    internal const float CraftGreyMix = 0.87f;
    internal const float StackOffset = 6f;
    internal const int MaxStack = 3;
    internal const float ScreenPadding = 12f;
    internal const float BottomPulseExtent = 12f;

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
    internal static LayoutStyle Layout { get; set; } = LayoutStyle.Row;
    internal static PositionPreset Position { get; set; } = PositionPreset.BottomCenter;
    internal static float CustomX { get; set; } = 10f;
    internal static float CustomY { get; set; } = 10f;
    internal static bool ShowLeft { get; set; } = true;
    internal static bool ShowRight { get; set; } = true;
    internal static bool ShowStomach { get; set; } = true;
    internal static bool ShowBack { get; set; } = true;
    internal static bool ShowCraft { get; set; } = true;
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
        _ => false,
    };

    internal static bool ShowEmpty(SlotId id) => id != SlotId.Craft;

    internal static IdArrange ParseIdArrange(string value) => value switch
    {
        "stack" => IdArrange.Stack,
        "cycle" => IdArrange.Cycle,
        _ => IdArrange.Row,
    };

    internal static LayoutStyle ParseLayout(string value) => value switch
    {
        "cross" => LayoutStyle.Cross,
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
