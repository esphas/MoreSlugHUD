using UnityEngine;

namespace MoreSlugHUD;

internal readonly struct IdLabelBar
{
    internal IdLabelBar(bool visible, float width, float height, float x, float y)
    {
        Visible = visible;
        Width = width;
        Height = height;
        X = x;
        Y = y;
    }

    internal bool Visible { get; }
    internal float Width { get; }
    internal float Height { get; }
    internal float X { get; }
    internal float Y { get; }

    internal static IdLabelBar Hidden => new(false, 0f, 0f, 0f, 0f);
}

internal static class IdLabelBars
{
    internal static void Layout(
        bool enabled,
        bool row,
        bool bothLines,
        bool showNumber,
        bool showName,
        float numberWidth,
        float nameWidth,
        float nameHeight,
        float textWidth,
        float rowWidth,
        float numberY,
        float nameY,
        out IdLabelBar number,
        out IdLabelBar name)
    {
        number = IdLabelBar.Hidden;
        name = IdLabelBar.Hidden;
        if (!enabled || (!showNumber && !showName))
        {
            return;
        }

        if (bothLines)
        {
            if (showNumber)
            {
                number = Bar(numberWidth, MoreSlugHUDConfig.IdBarHeight, 0f, numberY);
            }

            if (showName)
            {
                name = Bar(nameWidth, nameHeight + 6f, 0f, nameY);
            }

            return;
        }

        var height = 0f;
        if (showNumber)
        {
            height = MoreSlugHUDConfig.IdBarHeight;
        }

        if (showName)
        {
            height = Mathf.Max(height, nameHeight + 6f);
        }

        var width = row ? textWidth : showNumber ? numberWidth : nameWidth;
        var x = row ? -rowWidth * 0.5f + textWidth * 0.5f : 0f;
        number = Bar(width, height, x, 0f);
    }

    private static IdLabelBar Bar(float contentWidth, float height, float x, float y)
    {
        if (contentWidth <= 0f || height <= 0f)
        {
            return IdLabelBar.Hidden;
        }

        return new IdLabelBar(true, contentWidth + MoreSlugHUDConfig.IdPadX, height, x, y);
    }
}
