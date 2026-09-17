namespace MoreSlugHUD;

internal readonly struct HistoryRowPlacement
{
    internal HistoryRowPlacement(
        float durationX,
        float stateBandLeft,
        float directionX,
        float actionOrigin,
        float stateSlot,
        float actionSlot,
        bool alignLeft)
    {
        DurationX = durationX;
        StateBandLeft = stateBandLeft;
        DirectionX = directionX;
        ActionOrigin = actionOrigin;
        StateSlot = stateSlot;
        ActionSlot = actionSlot;
        AlignLeft = alignLeft;
    }

    internal float DurationX { get; }
    internal float StateBandLeft { get; }
    internal float DirectionX { get; }
    internal float ActionOrigin { get; }
    internal float StateSlot { get; }
    internal float ActionSlot { get; }
    internal bool AlignLeft { get; }
}

internal static class HistoryLayout
{
    internal const float IconSize = 16f;
    internal const float DurationScale = 0.55f;
    internal const float Slot = 16f;
    internal const float LetterStateSlot = 20f;
    internal const float LetterActionSlot = 12f;
    internal const float DurationColumn = 16f;
    internal const float DirectionGap = 2f;
    internal const int MaxSimultaneousStates = 3;

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

    internal static HistoryRowPlacement Arrange(float left, bool alignLeft)
    {
        var showStates = InputHistoryConfig.ShowStates;
        var showDir = InputHistoryConfig.TrackDirection;
        var stateSlot = StateSlot;
        var dirSlot = Slot;
        var gap = DirectionGap;
        var stateWidth = showStates ? MaxSimultaneousStates * stateSlot : 0f;
        var dirBlock = showDir ? gap + dirSlot : InputHistoryConfig.TrackedActionCount > 0 ? gap : 0f;
        float stateBandLeft;
        float durationX;
        float directionX;
        float actionOrigin;
        if (alignLeft)
        {
            stateBandLeft = left;
            durationX = left + stateWidth + DurationColumn;
            directionX = durationX + gap + dirSlot * 0.5f;
            actionOrigin = durationX + dirBlock;
        }
        else
        {
            var right = left + RowWidth;
            stateBandLeft = right - stateWidth;
            durationX = stateBandLeft - DurationColumn;
            directionX = durationX - gap - dirSlot * 0.5f;
            actionOrigin = durationX - dirBlock;
        }

        return new HistoryRowPlacement(
            durationX,
            stateBandLeft,
            directionX,
            actionOrigin,
            stateSlot,
            ActionSlot,
            alignLeft);
    }
}
