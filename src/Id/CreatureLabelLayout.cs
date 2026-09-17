namespace MoreSlugHUD;

internal readonly struct CreatureLabelMetrics
{
    internal CreatureLabelMetrics(
        bool showNumber,
        bool showName,
        bool hasIntent,
        float numberWidth,
        float nameWidth,
        float nameHeight,
        float intentWidth,
        float glyphAdvance)
    {
        ShowNumber = showNumber;
        ShowName = showName;
        HasIntent = hasIntent;
        NumberWidth = numberWidth;
        NameWidth = nameWidth;
        NameHeight = nameHeight;
        IntentWidth = intentWidth;
        GlyphAdvance = glyphAdvance;
    }

    internal bool ShowNumber { get; }
    internal bool ShowName { get; }
    internal bool HasIntent { get; }
    internal float NumberWidth { get; }
    internal float NameWidth { get; }
    internal float NameHeight { get; }
    internal float IntentWidth { get; }
    internal float GlyphAdvance { get; }
}

internal readonly struct CreatureLabelPlacement
{
    internal CreatureLabelPlacement(
        bool row,
        bool bothLines,
        bool showNumber,
        bool showName,
        float numberX,
        float numberY,
        float nameStartX,
        float nameY,
        float intentX,
        float intentY,
        float textWidth,
        float rowWidth,
        float glyphAdvance)
    {
        Row = row;
        BothLines = bothLines;
        ShowNumber = showNumber;
        ShowName = showName;
        NumberX = numberX;
        NumberY = numberY;
        NameStartX = nameStartX;
        NameY = nameY;
        IntentX = intentX;
        IntentY = intentY;
        TextWidth = textWidth;
        RowWidth = rowWidth;
        GlyphAdvance = glyphAdvance;
    }

    internal bool Row { get; }
    internal bool BothLines { get; }
    internal bool ShowNumber { get; }
    internal bool ShowName { get; }
    internal float NumberX { get; }
    internal float NumberY { get; }
    internal float NameStartX { get; }
    internal float NameY { get; }
    internal float IntentX { get; }
    internal float IntentY { get; }
    internal float TextWidth { get; }
    internal float RowWidth { get; }
    internal float GlyphAdvance { get; }
}

internal static class CreatureLabelLayout
{
    internal static CreatureLabelPlacement Arrange(
        in CreatureLabelMetrics metrics,
        IdArrange arrange,
        float itemGap,
        float intentGap)
    {
        var row = arrange == IdArrange.Row;
        var bothLines = !row && metrics.ShowNumber && metrics.ShowName;
        var line = metrics.NameHeight * 0.5f + 4f;
        var numberY = bothLines ? line : 0f;
        var nameY = bothLines ? -line : 0f;
        var numberX = 0f;
        var nameStartX = -metrics.NameWidth * 0.5f;
        var intentX = 0f;
        var intentY = 0f;
        var textWidth = 0f;
        if (metrics.ShowNumber)
        {
            textWidth += metrics.NumberWidth;
        }

        if (metrics.ShowNumber && metrics.ShowName)
        {
            textWidth += itemGap;
        }

        if (metrics.ShowName)
        {
            textWidth += metrics.NameWidth;
        }

        var rowWidth = textWidth;
        if (row)
        {
            if (metrics.HasIntent && (metrics.ShowNumber || metrics.ShowName))
            {
                rowWidth += intentGap + metrics.IntentWidth;
            }
            else if (metrics.HasIntent)
            {
                rowWidth += metrics.IntentWidth;
            }

            var cursor = -rowWidth * 0.5f;
            numberX = cursor + metrics.NumberWidth * 0.5f;
            if (metrics.ShowNumber)
            {
                cursor += metrics.NumberWidth;
            }

            if (metrics.ShowName)
            {
                if (metrics.ShowNumber)
                {
                    cursor += itemGap;
                }

                nameStartX = cursor;
                cursor += metrics.NameWidth;
            }

            if (metrics.HasIntent && (metrics.ShowNumber || metrics.ShowName))
            {
                cursor += intentGap;
            }

            intentX = cursor;
            intentY = 0f;
            numberY = 0f;
            nameY = 0f;
        }
        else
        {
            numberX = 0f;
            nameStartX = -metrics.NameWidth * 0.5f;
            if (metrics.ShowNumber)
            {
                intentX = metrics.NumberWidth * 0.5f + intentGap;
                intentY = numberY;
            }
            else if (metrics.ShowName)
            {
                intentX = metrics.NameWidth * 0.5f + intentGap;
                intentY = nameY;
            }
            else
            {
                intentX = -metrics.IntentWidth * 0.5f;
                intentY = 0f;
            }
        }

        return new CreatureLabelPlacement(
            row,
            bothLines,
            metrics.ShowNumber,
            metrics.ShowName,
            numberX,
            numberY,
            nameStartX,
            nameY,
            intentX,
            intentY,
            textWidth,
            rowWidth,
            metrics.GlyphAdvance);
    }
}
